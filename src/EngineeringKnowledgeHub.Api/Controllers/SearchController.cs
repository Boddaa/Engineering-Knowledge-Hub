namespace EngineeringKnowledgeHub.Api.Controllers;

using EngineeringKnowledgeHub.Api.Models;
using EngineeringKnowledgeHub.Application.DTOs;
using EngineeringKnowledgeHub.Application.Search;
using EngineeringKnowledgeHub.Application.SemanticSearch;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Endpoints for keyword, structured metadata, hybrid, and semantic search across knowledge articles.
/// </summary>
[ApiController]
[Route("api/search")]
public sealed class SearchController(
    ISearchService searchService,
    ISemanticSearchService semanticSearchService) : ControllerBase
{
    private readonly ISearchService _searchService = searchService ?? throw new ArgumentNullException(nameof(searchService));
    private readonly ISemanticSearchService _semanticSearchService = semanticSearchService ?? throw new ArgumentNullException(nameof(semanticSearchService));

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

    /// <summary>
    /// Executes semantic vector similarity search across indexed knowledge content.
    /// </summary>
    [HttpPost("semantic")]
    [ProducesResponseType<IReadOnlyList<SemanticSearchResultDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SemanticSearch(
        [FromBody] SemanticSearchApiRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation Error",
                Detail = "Semantic search request cannot be null."
            });
        }

        var results = await _semanticSearchService.SearchAsync(request.ToApplicationRequest(), cancellationToken);
        return Ok(results);
    }
}
