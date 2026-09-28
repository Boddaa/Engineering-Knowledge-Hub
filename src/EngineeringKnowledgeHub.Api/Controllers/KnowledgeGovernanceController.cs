namespace EngineeringKnowledgeHub.Api.Controllers;

using EngineeringKnowledgeHub.Application.DTOs;
using EngineeringKnowledgeHub.Application.Governance;
using EngineeringKnowledgeHub.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Endpoints for inspecting historical knowledge versions and governance audit records.
/// Historical versions are immutable audit records and cannot be directly mutated.
/// </summary>
[ApiController]
public sealed class KnowledgeGovernanceController(IKnowledgeGovernanceService governanceService) : ControllerBase
{
    private readonly IKnowledgeGovernanceService _governanceService = governanceService ?? throw new ArgumentNullException(nameof(governanceService));

    /// <summary>
    /// Retrieves the immutable version history of a knowledge article.
    /// </summary>
    [HttpGet("api/knowledge/articles/{articleId:guid}/versions")]
    [ProducesResponseType<IReadOnlyList<KnowledgeVersionDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVersionHistory(
        Guid articleId,
        CancellationToken cancellationToken = default)
    {
        if (articleId == Guid.Empty)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation Error",
                Detail = "Article ID cannot be empty."
            });
        }

        var versions = await _governanceService.GetVersionHistoryAsync(articleId, cancellationToken);
        return Ok(versions);
    }

    /// <summary>
    /// Retrieves a specific historical snapshot of a knowledge article by its version number.
    /// </summary>
    [HttpGet("api/knowledge/articles/{articleId:guid}/versions/{versionNumber:int}")]
    [ProducesResponseType<KnowledgeVersionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVersionByNumber(
        Guid articleId,
        int versionNumber,
        CancellationToken cancellationToken = default)
    {
        if (articleId == Guid.Empty)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation Error",
                Detail = "Article ID cannot be empty."
            });
        }

        if (versionNumber <= 0)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation Error",
                Detail = "Version number must be greater than zero."
            });
        }

        var version = await _governanceService.GetVersionByNumberAsync(articleId, versionNumber, cancellationToken);
        return Ok(version);
    }

    /// <summary>
    /// Retrieves a historical version snapshot by its unique version ID.
    /// </summary>
    [HttpGet("api/knowledge/versions/{versionId:guid}")]
    [ProducesResponseType<KnowledgeVersionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVersionById(
        Guid versionId,
        CancellationToken cancellationToken = default)
    {
        if (versionId == Guid.Empty)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation Error",
                Detail = "Version ID cannot be empty."
            });
        }

        var version = await _governanceService.GetVersionByIdAsync(versionId, cancellationToken);
        return Ok(version);
    }

    /// <summary>
    /// Retrieves the governance audit trail for a specific knowledge article.
    /// </summary>
    [HttpGet("api/knowledge/articles/{articleId:guid}/audit")]
    [ProducesResponseType<IReadOnlyList<AuditLogDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetArticleAuditTrail(
        Guid articleId,
        CancellationToken cancellationToken = default)
    {
        if (articleId == Guid.Empty)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation Error",
                Detail = "Article ID cannot be empty."
            });
        }

        var auditLogs = await _governanceService.GetAuditLogsByEntityAsync(nameof(KnowledgeArticle), articleId, cancellationToken);
        return Ok(auditLogs);
    }
}
