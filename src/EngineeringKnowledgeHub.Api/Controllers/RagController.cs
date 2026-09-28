namespace EngineeringKnowledgeHub.Api.Controllers;

using EngineeringKnowledgeHub.Api.Models;
using EngineeringKnowledgeHub.Application.DTOs;
using EngineeringKnowledgeHub.Application.Rag;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Endpoints for assisted Retrieval-Augmented Generation (RAG) queries and contextual answer synthesis.
/// </summary>
[ApiController]
[Route("api/rag")]
public sealed class RagController(IRagService ragService) : ControllerBase
{
    private readonly IRagService _ragService = ragService ?? throw new ArgumentNullException(nameof(ragService));

    /// <summary>
    /// Submits a natural-language engineering question to the RAG knowledge assistant.
    /// Retrieves grounded evidence and synthesizes a citation-backed response.
    /// </summary>
    [HttpPost("query")]
    [HttpPost("ask")]
    [ProducesResponseType<RagResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Query(
        [FromBody] RagQueryRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _ragService.AskAsync(request.ToApplicationRequest(), cancellationToken);
        return Ok(result);
    }
}
