namespace EngineeringKnowledgeHub.Application.Search;

using EngineeringKnowledgeHub.Application.DTOs;

/// <summary>
/// Application service contract for local-first knowledge article search.
/// </summary>
public interface ISearchService
{
    Task<PagedResult<SearchArticleResultDto>> SearchArticlesAsync(
        SearchArticleRequest request,
        CancellationToken cancellationToken = default);
}
