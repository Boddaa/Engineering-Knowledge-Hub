namespace EngineeringKnowledgeHub.Api.Controllers;

using EngineeringKnowledgeHub.Api.Security;
using EngineeringKnowledgeHub.Application.DTOs;
using EngineeringKnowledgeHub.Application.Governance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Endpoints for inspecting system-wide governance audit logs.
/// Protected by AuditGovernancePolicy: requires authenticated user with Admin or Auditor role.
/// </summary>
[ApiController]
[Route("api/audit")]
[Authorize(Policy = GovernanceAuthenticationDefaults.AuditGovernancePolicy)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public sealed class AuditController(IKnowledgeGovernanceService governanceService) : ControllerBase
{
    private readonly IKnowledgeGovernanceService _governanceService = governanceService ?? throw new ArgumentNullException(nameof(governanceService));

    /// <summary>
    /// Retrieves all system audit logs in descending chronological order.
    /// </summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<AuditLogDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default)
    {
        var logs = await _governanceService.GetAllAuditLogsAsync(cancellationToken);
        return Ok(logs);
    }

    /// <summary>
    /// Retrieves audit logs associated with a specific entity type and entity ID.
    /// </summary>
    [HttpGet("entities/{entityName}/{entityId:guid}")]
    [ProducesResponseType<IReadOnlyList<AuditLogDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetByEntity(
        string entityName,
        Guid entityId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(entityName))
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation Error",
                Detail = "Entity name is required."
            });
        }

        if (entityId == Guid.Empty)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation Error",
                Detail = "Entity ID cannot be empty."
            });
        }

        var logs = await _governanceService.GetAuditLogsByEntityAsync(entityName, entityId, cancellationToken);
        return Ok(logs);
    }

    /// <summary>
    /// Retrieves audit logs recorded for a specific user ID.
    /// </summary>
    [HttpGet("users/{userId:guid}")]
    [ProducesResponseType<IReadOnlyList<AuditLogDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetByUser(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation Error",
                Detail = "User ID cannot be empty."
            });
        }

        var logs = await _governanceService.GetAuditLogsByUserIdAsync(userId, cancellationToken);
        return Ok(logs);
    }
}
