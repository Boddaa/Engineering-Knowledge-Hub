namespace EngineeringKnowledgeHub.Api.Controllers;

using EngineeringKnowledgeHub.Application.DTOs;
using EngineeringKnowledgeHub.Application.Search;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Endpoints for keyword, structured metadata, and hybrid search across knowledge articles.
/// </summary>
[ApiController]
[Route("api/search")]
public sealed class SearchController(ISearchService searchService) : ControllerBase
{
    private readonly ISearchService _searchService = searchService ?? throw new ArgumentNullException(nameof(searchService));

    /// <summary>
    /// Searches knowledge articles using query parameters for keyword terms, structured filters, and pagination.
    /// </summary>
    [HttpGet("articles")]
    [ProducesResponseType<PagedResult<SearchArticleResultDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchArticlesFromQuery(
        [FromQuery] SearchArticleRequest? request = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _searchService.SearchArticlesAsync(request ?? new SearchArticleRequest(), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Searches knowledge articles using a structured request payload for keyword terms, metadata filters, and pagination.
    /// </summary>
    [HttpPost("articles")]
    [ProducesResponseType<PagedResult<SearchArticleResultDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SearchArticles(
        [FromBody] SearchArticleRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation Error",
                Detail = "Search request cannot be null."
            });
        }

        var result = await _searchService.SearchArticlesAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Executes hybrid search combining BM25 keyword matching and semantic vector similarity.
    /// </summary>
    [HttpPost("articles/hybrid")]
    [ProducesResponseType<PagedResult<SearchArticleResultDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> HybridSearchArticles(
        [FromBody] HybridSearchArticleRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation Error",
                Detail = "Hybrid search request cannot be null."
            });
        }

        var result = await _searchService.HybridSearchArticlesAsync(request, cancellationToken);
        return Ok(result);
    }
}
