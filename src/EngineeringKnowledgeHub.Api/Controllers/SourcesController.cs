namespace EngineeringKnowledgeHub.Api.Controllers;

using EngineeringKnowledgeHub.Api.Models;
using EngineeringKnowledgeHub.Application.Common.Exceptions;
using EngineeringKnowledgeHub.Application.DTOs;
using EngineeringKnowledgeHub.Application.Enrichment;
using EngineeringKnowledgeHub.Application.Sources;
using EngineeringKnowledgeHub.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Endpoints for managing knowledge sources, approval lifecycle, trust levels, article associations,
/// and external source enrichment import.
/// </summary>
[ApiController]
[Route("api/sources")]
public sealed class SourcesController : ControllerBase
{
    private readonly IKnowledgeSourceService? _sourceService;
    private readonly IExternalEnrichmentService? _enrichmentService;

    [ActivatorUtilitiesConstructor]
    public SourcesController(
        IKnowledgeSourceService sourceService,
        IExternalEnrichmentService? enrichmentService = null)
    {
        _sourceService = sourceService;
        _enrichmentService = enrichmentService;
    }

    public SourcesController() : this(null!, null)
    {
    }

    public SourcesController(IExternalEnrichmentService? enrichmentService)
        : this(null!, enrichmentService)
    {
    }

    /// <summary>
    /// Registers a new knowledge source.
    /// </summary>
    [HttpPost]
    [ProducesResponseType<KnowledgeSourceDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateKnowledgeSourceRequest request,
        CancellationToken cancellationToken = default)
    {
        if (_sourceService == null)
        {
            return ServiceUnavailable("Knowledge source service is not configured.");
        }

        var result = await _sourceService.CreateSourceAsync(request.ToCommand(), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Retrieves a knowledge source by its unique identifier.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<KnowledgeSourceDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return EmptyGuidError("Source ID");
        }

        if (_sourceService == null)
        {
            return ServiceUnavailable("Knowledge source service is not configured.");
        }

        var source = await _sourceService.GetSourceByIdAsync(id, cancellationToken);
        return Ok(source);
    }

    /// <summary>
    /// Lists all knowledge sources, optionally filtered by source type, trust level, and approval status.
    /// </summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<KnowledgeSourceDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSources(
        [FromQuery] SourceType? sourceType = null,
        [FromQuery] TrustLevel? trustLevel = null,
        [FromQuery] bool? isApproved = null,
        CancellationToken cancellationToken = default)
    {
        if (_sourceService == null)
        {
            return ServiceUnavailable("Knowledge source service is not configured.");
        }

        var sources = await _sourceService.GetSourcesAsync(sourceType, trustLevel, isApproved, cancellationToken);
        return Ok(sources);
    }

    /// <summary>
    /// Updates the metadata of an existing knowledge source.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<KnowledgeSourceDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateKnowledgeSourceRequest request,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return EmptyGuidError("Source ID");
        }

        if (_sourceService == null)
        {
            return ServiceUnavailable("Knowledge source service is not configured.");
        }

        var result = await _sourceService.UpdateSourceAsync(request.ToCommand(id), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Approves a knowledge source for use in the engineering knowledge hub.
    /// </summary>
    [HttpPost("{id:guid}/approve")]
    [ProducesResponseType<KnowledgeSourceDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Approve(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return EmptyGuidError("Source ID");
        }

        if (_sourceService == null)
        {
            return ServiceUnavailable("Knowledge source service is not configured.");
        }

        var result = await _sourceService.ApproveSourceAsync(id, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Revokes approval of an approved knowledge source.
    /// </summary>
    [HttpPost("{id:guid}/revoke-approval")]
    [ProducesResponseType<KnowledgeSourceDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RevokeApproval(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return EmptyGuidError("Source ID");
        }

        if (_sourceService == null)
        {
            return ServiceUnavailable("Knowledge source service is not configured.");
        }

        var result = await _sourceService.RevokeSourceApprovalAsync(id, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Updates the organizational trust level of a knowledge source.
    /// </summary>
    [HttpPut("{id:guid}/trust-level")]
    [ProducesResponseType<KnowledgeSourceDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTrustLevel(
        Guid id,
        [FromBody] UpdateTrustLevelRequest request,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return EmptyGuidError("Source ID");
        }

        if (_sourceService == null)
        {
            return ServiceUnavailable("Knowledge source service is not configured.");
        }

        var result = await _sourceService.UpdateTrustLevelAsync(request.ToCommand(id), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Attaches a knowledge source to a knowledge article.
    /// </summary>
    [HttpPost("{id:guid}/articles/{articleId:guid}")]
    [HttpPost("/api/knowledge/articles/{articleId:guid}/sources/{id:guid}")]
    [ProducesResponseType<KnowledgeArticleDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AttachSource(
        Guid id,
        Guid articleId,
        [FromBody] AttachSourceRequest? request = null,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return EmptyGuidError("Source ID");
        }

        if (articleId == Guid.Empty)
        {
            return EmptyGuidError("Article ID");
        }

        if (_sourceService == null)
        {
            return ServiceUnavailable("Knowledge source service is not configured.");
        }

        var command = (request ?? new AttachSourceRequest()).ToCommand(articleId, id);
        var result = await _sourceService.AttachSourceToArticleAsync(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Detaches a knowledge source from a knowledge article.
    /// </summary>
    [HttpDelete("{id:guid}/articles/{articleId:guid}")]
    [HttpDelete("/api/knowledge/articles/{articleId:guid}/sources/{id:guid}")]
    [ProducesResponseType<KnowledgeArticleDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DetachSource(
        Guid id,
        Guid articleId,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return EmptyGuidError("Source ID");
        }

        if (articleId == Guid.Empty)
        {
            return EmptyGuidError("Article ID");
        }

        if (_sourceService == null)
        {
            return ServiceUnavailable("Knowledge source service is not configured.");
        }

        var command = new DetachArticleSourceCommand(articleId, id);
        var result = await _sourceService.DetachSourceFromArticleAsync(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves all knowledge sources attached to a specific article.
    /// </summary>
    [HttpGet("article/{articleId:guid}")]
    [HttpGet("/api/knowledge/articles/{articleId:guid}/sources")]
    [ProducesResponseType<IReadOnlyList<KnowledgeSourceDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSourcesByArticle(
        Guid articleId,
        CancellationToken cancellationToken = default)
    {
        if (articleId == Guid.Empty)
        {
            return EmptyGuidError("Article ID");
        }

        if (_sourceService == null)
        {
            return ServiceUnavailable("Knowledge source service is not configured.");
        }

        var sources = await _sourceService.GetSourcesByArticleIdAsync(articleId, cancellationToken);
        return Ok(sources);
    }

    /// <summary>
    /// Triggers automated document import and text enrichment from an external source.
    /// </summary>
    [HttpPost("{id:guid}/import")]
    [ProducesResponseType<ExternalImportResultDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> ImportFromSource(
        Guid id,
        [FromBody] ExternalImportCommand? command = null,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return EmptyGuidError("Source ID");
        }

        if (_enrichmentService == null)
        {
            return ServiceUnavailable("External enrichment service is not configured.");
        }

        try
        {
            var result = await _enrichmentService.ImportFromSourceAsync(id, command, cancellationToken);
            return Ok(result);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Source Not Found",
                Detail = ex.Message
            });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation Error",
                Detail = ex.Message
            });
        }
    }

    private static BadRequestObjectResult EmptyGuidError(string fieldName) =>
        new(new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Validation Error",
            Detail = $"{fieldName} cannot be empty."
        });

    private static ObjectResult ServiceUnavailable(string detail) =>
        new(new ProblemDetails
        {
            Status = StatusCodes.Status503ServiceUnavailable,
            Title = "Service Unavailable",
            Detail = detail
        })
        {
            StatusCode = StatusCodes.Status503ServiceUnavailable
        };
}
