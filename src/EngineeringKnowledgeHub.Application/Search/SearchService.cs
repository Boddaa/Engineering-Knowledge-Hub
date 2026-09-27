namespace EngineeringKnowledgeHub.Application.Search;

using EngineeringKnowledgeHub.Application.Common.Exceptions;
using EngineeringKnowledgeHub.Application.Common.Interfaces;
using EngineeringKnowledgeHub.Application.DTOs;
using EngineeringKnowledgeHub.Application.SemanticSearch;
using EngineeringKnowledgeHub.Domain.Entities;
using EngineeringKnowledgeHub.Domain.Enums;

/// <summary>
/// Local-first knowledge article search engine supporting keyword substring scoring across text content
/// (Title, Summary, Content), structured metadata filtering (Product, Technology, KnowledgeType, Tag, Status, VerificationStatus),
/// explicit sorting, and bounded pagination.
/// NOTE: In this pre-persistence phase, search executes in-memory over retrieved articles.
/// </summary>
public sealed class SearchService : ISearchService
{
    private readonly IKnowledgeArticleRepository _articleRepository;
    private readonly ISemanticSearchService? _semanticSearchService;

    public SearchService(
        IKnowledgeArticleRepository articleRepository,
        ISemanticSearchService? semanticSearchService = null)
    {
        _articleRepository = articleRepository ?? throw new ArgumentNullException(nameof(articleRepository));
        _semanticSearchService = semanticSearchService;
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

    public async Task<PagedResult<SearchArticleResultDto>> HybridSearchArticlesAsync(
        HybridSearchArticleRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateHybridRequest(request);

        var articles = await _articleRepository.GetAllAsync(cancellationToken);

        var trimmedQuery = request.Query?.Trim();
        var hasQuery = !string.IsNullOrWhiteSpace(trimmedQuery);
        var terms = hasQuery
            ? trimmedQuery!.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            : Array.Empty<string>();

        var filteredArticles = articles
            .Where(a => MatchesHybridFilters(a, request))
            .ToList();

        if (filteredArticles.Count == 0)
        {
            return PagedResult<SearchArticleResultDto>.Empty(request.Page, request.PageSize);
        }

        if (!hasQuery)
        {
            var noQueryScored = filteredArticles
                .Select(a => (Article: a, KeywordScore: 0.0, SemanticScore: 0.0, HybridScore: 0.0))
                .ToList();

            return BuildPagedHybridResult(noQueryScored, request.SortBy, request.Page, request.PageSize, terms);
        }

        var keywordScores = new Dictionary<Guid, int>();
        int maxKeywordScore = 0;

        foreach (var article in filteredArticles)
        {
            var score = ComputeRelevanceScore(article, trimmedQuery!, terms);
            keywordScores[article.Id] = score;
            if (score > maxKeywordScore)
            {
                maxKeywordScore = score;
            }
        }

        var semanticScores = new Dictionary<Guid, double>();
        if (_semanticSearchService != null && request.Options.SemanticWeight > 0.0)
        {
            var semanticResults = await _semanticSearchService.SearchAsync(
                new SemanticSearchRequest
                {
                    Query = trimmedQuery!,
                    TopK = Math.Max(1, filteredArticles.Count),
                    ProductId = request.ProductId,
                    TechnologyId = request.TechnologyId,
                    KnowledgeTypeId = request.KnowledgeTypeId,
                    TagId = request.TagId,
                    Status = request.Status,
                    VerificationStatus = request.VerificationStatus
                },
                cancellationToken);

            foreach (var result in semanticResults)
            {
                if (result.ArticleId.HasValue)
                {
                    if (!semanticScores.TryGetValue(result.ArticleId.Value, out var existingSim) || result.SimilarityScore > existingSim)
                    {
                        semanticScores[result.ArticleId.Value] = result.SimilarityScore;
                    }
                }
            }
        }

        var candidates = new List<(KnowledgeArticle Article, double KeywordScore, double SemanticScore, double HybridScore)>();

        foreach (var article in filteredArticles)
        {
            var rawKeyword = keywordScores[article.Id];
            var normKeyword = maxKeywordScore > 0 ? (double)rawKeyword / maxKeywordScore : 0.0;
            semanticScores.TryGetValue(article.Id, out var normSemantic);

            if (rawKeyword <= 0 && normSemantic <= 0.0)
            {
                continue;
            }

            var hybridScore = (request.Options.KeywordWeight * normKeyword) +
                              (request.Options.SemanticWeight * normSemantic);

            if (article.VerificationStatus == VerificationStatus.Verified)
            {
                hybridScore *= request.Options.VerifiedBoost;
            }

            candidates.Add((article, normKeyword, normSemantic, hybridScore));
        }

        return BuildPagedHybridResult(candidates, request.SortBy, request.Page, request.PageSize, terms);
    }

    private static void ValidateHybridRequest(HybridSearchArticleRequest request)
    {
        if (request is null)
        {
            throw new ValidationException("Hybrid search request cannot be null.");
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

        if (request.Options is null)
        {
            throw new ValidationException("HybridSearchOptions cannot be null.");
        }

        request.Options.Validate();
    }

    private static bool MatchesHybridFilters(KnowledgeArticle article, HybridSearchArticleRequest request)
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

    private static PagedResult<SearchArticleResultDto> BuildPagedHybridResult(
        List<(KnowledgeArticle Article, double KeywordScore, double SemanticScore, double HybridScore)> candidates,
        SearchSortOption sortBy,
        int page,
        int pageSize,
        string[] terms)
    {
        var sorted = sortBy switch
        {
            SearchSortOption.Relevance =>
                candidates
                    .OrderByDescending(c => c.HybridScore)
                    .ThenByDescending(c => c.Article.UpdatedAt ?? c.Article.CreatedAt)
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

        var totalCount = sorted.Count;
        if (totalCount == 0)
        {
            return PagedResult<SearchArticleResultDto>.Empty(page, pageSize);
        }

        var paged = sorted
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => MapToHybridResultDto(c.Article, terms, c.KeywordScore, c.SemanticScore, c.HybridScore))
            .ToList();

        return new PagedResult<SearchArticleResultDto>(paged, page, pageSize, totalCount);
    }

    private static SearchArticleResultDto MapToHybridResultDto(
        KnowledgeArticle article,
        string[] terms,
        double keywordScore,
        double semanticScore,
        double hybridScore)
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
            article.PublishedAt,
            KeywordScore: keywordScore,
            SemanticScore: semanticScore,
            HybridScore: hybridScore);
    }
}
