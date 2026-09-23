namespace EngineeringKnowledgeHub.Application.Search;

using EngineeringKnowledgeHub.Application.Common.Exceptions;
using EngineeringKnowledgeHub.Application.Common.Interfaces;
using EngineeringKnowledgeHub.Application.DTOs;
using EngineeringKnowledgeHub.Domain.Entities;
using EngineeringKnowledgeHub.Domain.Enums;

/// <summary>
/// Local-first knowledge article search engine supporting keyword scoring,
/// multi-dimensional filtering, explicit sorting, and bounded pagination.
/// </summary>
public sealed class SearchService : ISearchService
{
    private readonly IKnowledgeArticleRepository _articleRepository;

    public SearchService(IKnowledgeArticleRepository articleRepository)
    {
        _articleRepository = articleRepository ?? throw new ArgumentNullException(nameof(articleRepository));
    }

    public async Task<PagedResult<SearchArticleResultDto>> SearchArticlesAsync(
        SearchArticleRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        var articles = await _articleRepository.GetAllAsync(cancellationToken);

        var trimmedQuery = request.Query?.Trim();
        var hasQuery = !string.IsNullOrWhiteSpace(trimmedQuery);
        var terms = hasQuery
            ? trimmedQuery!.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            : Array.Empty<string>();

        var scored = new List<(KnowledgeArticle Article, int Score)>();

        foreach (var article in articles)
        {
            if (!MatchesFilters(article, request))
            {
                continue;
            }

            if (hasQuery)
            {
                var score = ComputeRelevanceScore(article, trimmedQuery!, terms);
                if (score <= 0)
                {
                    continue;
                }

                scored.Add((article, score));
            }
            else
            {
                scored.Add((article, 0));
            }
        }

        var sorted = SortCandidates(scored, request.SortBy, hasQuery);
        var totalCount = sorted.Count;

        if (totalCount == 0)
        {
            return PagedResult<SearchArticleResultDto>.Empty(request.Page, request.PageSize);
        }

        var paged = sorted
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => MapToResultDto(x.Article, terms))
            .ToList();

        return new PagedResult<SearchArticleResultDto>(
            paged,
            request.Page,
            request.PageSize,
            totalCount);
    }

    private static void ValidateRequest(SearchArticleRequest request)
    {
        if (request is null)
        {
            throw new ValidationException("Search request cannot be null.");
        }

        if (request.Page < 1)
        {
            throw new ValidationException("Page number must be at least 1.");
        }

        if (request.PageSize < 1 || request.PageSize > 100)
        {
            throw new ValidationException("Page size must be between 1 and 100.");
        }

        if (request.ProductId == Guid.Empty)
        {
            throw new ValidationException("ProductId cannot be an empty GUID.");
        }

        if (request.TechnologyId == Guid.Empty)
        {
            throw new ValidationException("TechnologyId cannot be an empty GUID.");
        }

        if (request.KnowledgeTypeId == Guid.Empty)
        {
            throw new ValidationException("KnowledgeTypeId cannot be an empty GUID.");
        }

        if (request.TagId == Guid.Empty)
        {
            throw new ValidationException("TagId cannot be an empty GUID.");
        }

        if (!Enum.IsDefined(typeof(SearchSortOption), request.SortBy))
        {
            throw new ValidationException("Invalid sort option.");
        }

        if (request.Status.HasValue && !Enum.IsDefined(typeof(KnowledgeStatus), request.Status.Value))
        {
            throw new ValidationException("Invalid knowledge status filter.");
        }

        if (request.VerificationStatus.HasValue && !Enum.IsDefined(typeof(VerificationStatus), request.VerificationStatus.Value))
        {
            throw new ValidationException("Invalid verification status filter.");
        }
    }

    private static bool MatchesFilters(KnowledgeArticle article, SearchArticleRequest request)
    {
        if (request.Status.HasValue && article.Status != request.Status.Value)
        {
            return false;
        }

        if (request.VerificationStatus.HasValue && article.VerificationStatus != request.VerificationStatus.Value)
        {
            return false;
        }

        if (request.ProductId.HasValue && !article.ProductIds.Contains(request.ProductId.Value))
        {
            return false;
        }

        if (request.TechnologyId.HasValue && !article.TechnologyIds.Contains(request.TechnologyId.Value))
        {
            return false;
        }

        if (request.KnowledgeTypeId.HasValue && article.KnowledgeTypeId != request.KnowledgeTypeId.Value)
        {
            return false;
        }

        if (request.TagId.HasValue && !article.TagIds.Contains(request.TagId.Value))
        {
            return false;
        }

        return true;
    }

    private static int ComputeRelevanceScore(KnowledgeArticle article, string fullQuery, string[] terms)
    {
        var score = 0;

        if (string.Equals(article.Title, fullQuery, StringComparison.OrdinalIgnoreCase))
        {
            score += 50;
        }

        foreach (var term in terms)
        {
            if (article.Title.Contains(term, StringComparison.OrdinalIgnoreCase))
            {
                score += 10;
            }

            if (!string.IsNullOrWhiteSpace(article.Summary) &&
                article.Summary.Contains(term, StringComparison.OrdinalIgnoreCase))
            {
                score += 5;
            }

            if (!string.IsNullOrWhiteSpace(article.Content) &&
                article.Content.Contains(term, StringComparison.OrdinalIgnoreCase))
            {
                score += 2;
            }
        }

        return score;
    }

    private static List<(KnowledgeArticle Article, int Score)> SortCandidates(
        List<(KnowledgeArticle Article, int Score)> candidates,
        SearchSortOption sortOption,
        bool hasQuery)
    {
        return sortOption switch
        {
            SearchSortOption.Relevance when hasQuery =>
                candidates
                    .OrderByDescending(c => c.Score)
                    .ThenByDescending(c => c.Article.UpdatedAt ?? c.Article.CreatedAt)
                    .ThenBy(c => c.Article.Title, StringComparer.OrdinalIgnoreCase)
                    .ToList(),

            SearchSortOption.Relevance =>
                candidates
                    .OrderByDescending(c => c.Article.UpdatedAt ?? c.Article.CreatedAt)
                    .ThenBy(c => c.Article.Title, StringComparer.OrdinalIgnoreCase)
                    .ToList(),

            SearchSortOption.RecentlyUpdated =>
                candidates
                    .OrderByDescending(c => c.Article.UpdatedAt ?? c.Article.CreatedAt)
                    .ThenBy(c => c.Article.Title, StringComparer.OrdinalIgnoreCase)
                    .ToList(),

            SearchSortOption.RecentlyCreated =>
                candidates
                    .OrderByDescending(c => c.Article.CreatedAt)
                    .ThenBy(c => c.Article.Title, StringComparer.OrdinalIgnoreCase)
                    .ToList(),

            SearchSortOption.TitleAsc =>
                candidates
                    .OrderBy(c => c.Article.Title, StringComparer.OrdinalIgnoreCase)
                    .ToList(),

            SearchSortOption.TitleDesc =>
                candidates
                    .OrderByDescending(c => c.Article.Title, StringComparer.OrdinalIgnoreCase)
                    .ToList(),

            _ => candidates
        };
    }

    private static SearchArticleResultDto MapToResultDto(KnowledgeArticle article, string[] terms)
    {
        var excerpt = GenerateExcerpt(article, terms);

        return new SearchArticleResultDto(
            article.Id,
            article.Title,
            article.Slug.Value,
            article.Summary,
            excerpt,
            article.KnowledgeTypeId,
            article.Status,
            article.VerificationStatus,
            article.ProductIds,
            article.TechnologyIds,
            article.TagIds,
            article.CreatedAt,
            article.UpdatedAt,
            article.PublishedAt);
    }

    private static string GenerateExcerpt(KnowledgeArticle article, string[] terms)
    {
        const int maxLength = 160;

        if (terms.Length > 0 && !string.IsNullOrWhiteSpace(article.Content))
        {
            var content = article.Content;
            foreach (var term in terms)
            {
                var index = content.IndexOf(term, StringComparison.OrdinalIgnoreCase);
                if (index >= 0)
                {
                    var start = Math.Max(0, index - 40);
                    var length = Math.Min(content.Length - start, maxLength);
                    var snippet = content.Substring(start, length).Trim();

                    if (start > 0) snippet = "..." + snippet;
                    if (start + length < content.Length) snippet += "...";

                    return snippet;
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(article.Summary))
        {
            return article.Summary.Length <= maxLength
                ? article.Summary
                : article.Summary.Substring(0, maxLength).Trim() + "...";
        }

        if (!string.IsNullOrWhiteSpace(article.Content))
        {
            return article.Content.Length <= maxLength
                ? article.Content
                : article.Content.Substring(0, maxLength).Trim() + "...";
        }

        return string.Empty;
    }
}
