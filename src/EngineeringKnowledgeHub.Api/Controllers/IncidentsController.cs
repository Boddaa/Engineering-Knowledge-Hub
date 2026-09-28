namespace EngineeringKnowledgeHub.Api.Controllers;

using EngineeringKnowledgeHub.Api.Models;
using EngineeringKnowledgeHub.Application.DTOs;
using EngineeringKnowledgeHub.Application.Incidents;
using EngineeringKnowledgeHub.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Endpoints for managing operational incidents, incident-to-knowledge relationships,
/// and knowledge proposal workflows.
/// </summary>
[ApiController]
[Route("api/incidents")]
public sealed class IncidentsController(IIncidentService incidentService) : ControllerBase
{
    private readonly IIncidentService _incidentService = incidentService ?? throw new ArgumentNullException(nameof(incidentService));

    /// <summary>
    /// Creates and records a new operational incident.
    /// </summary>
    [HttpPost]
    [ProducesResponseType<IncidentDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateIncidentRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.CreatedById == Guid.Empty)
        {
            return EmptyGuidError("CreatedById");
        }

        var result = await _incidentService.CreateIncidentAsync(request.ToCommand(), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Retrieves an incident by its unique identifier.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<IncidentDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return EmptyGuidError("Incident ID");
        }

        var incident = await _incidentService.GetIncidentByIdAsync(id, cancellationToken);
        return Ok(incident);
    }

    /// <summary>
    /// Lists all incidents, optionally filtered by status.
    /// </summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<IncidentDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetIncidents(
        [FromQuery] IncidentStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _incidentService.GetIncidentsAsync(status, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves all incidents linked to a specific knowledge article.
    /// </summary>
    [HttpGet("article/{articleId:guid}")]
    [ProducesResponseType<IReadOnlyList<IncidentDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetByArticle(
        Guid articleId,
        CancellationToken cancellationToken = default)
    {
        if (articleId == Guid.Empty)
        {
            return EmptyGuidError("Article ID");
        }

        var result = await _incidentService.GetIncidentsByArticleIdAsync(articleId, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Starts investigation on an incident, transitioning its status to Investigating.
    /// </summary>
    [HttpPost("{id:guid}/investigate")]
    [ProducesResponseType<IncidentDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> StartInvestigation(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return EmptyGuidError("Incident ID");
        }

        var result = await _incidentService.StartInvestigationAsync(id, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Resolves an active incident with resolution details and optional root cause analysis.
    /// </summary>
    [HttpPost("{id:guid}/resolve")]
    [ProducesResponseType<IncidentDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Resolve(
        Guid id,
        [FromBody] ResolveIncidentRequest request,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return EmptyGuidError("Incident ID");
        }

        if (request.ResolvedById == Guid.Empty)
        {
            return EmptyGuidError("ResolvedById");
        }

        var result = await _incidentService.ResolveIncidentAsync(request.ToCommand(id), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Closes a resolved incident.
    /// </summary>
    [HttpPost("{id:guid}/close")]
    [ProducesResponseType<IncidentDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Close(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return EmptyGuidError("Incident ID");
        }

        var result = await _incidentService.CloseIncidentAsync(id, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Links a knowledge article to an incident with a specific relationship type.
    /// </summary>
    [HttpPost("{id:guid}/articles")]
    [ProducesResponseType<IncidentDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> LinkArticle(
        Guid id,
        [FromBody] LinkArticleRequest request,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return EmptyGuidError("Incident ID");
        }

        if (request.ArticleId == Guid.Empty)
        {
            return EmptyGuidError("Article ID");
        }

        var result = await _incidentService.LinkArticleToIncidentAsync(request.ToCommand(id), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Removes a linked article association from an incident.
    /// </summary>
    [HttpDelete("{id:guid}/articles/{articleId:guid}")]
    [ProducesResponseType<IncidentDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnlinkArticle(
        Guid id,
        Guid articleId,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return EmptyGuidError("Incident ID");
        }

        if (articleId == Guid.Empty)
        {
            return EmptyGuidError("Article ID");
        }

        var result = await _incidentService.UnlinkArticleFromIncidentAsync(id, articleId, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Proposes a new draft knowledge article synthesized from a resolved incident.
    /// </summary>
    [HttpPost("{id:guid}/propose-article")]
    [ProducesResponseType<KnowledgeArticleDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateKnowledgeFromIncident(
        Guid id,
        [FromBody] CreateKnowledgeFromIncidentRequest request,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return EmptyGuidError("Incident ID");
        }

        if (request.AuthorId == Guid.Empty)
        {
            return EmptyGuidError("AuthorId");
        }

        if (request.KnowledgeTypeId == Guid.Empty)
        {
            return EmptyGuidError("KnowledgeTypeId");
        }

        var result = await _incidentService.CreateKnowledgeFromIncidentAsync(request.ToCommand(id), cancellationToken);
        return Ok(result);
    }

    private static BadRequestObjectResult EmptyGuidError(string fieldName) =>
        new(new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Validation Error",
            Detail = $"{fieldName} cannot be empty."
        });
}
