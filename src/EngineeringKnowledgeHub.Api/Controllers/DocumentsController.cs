namespace EngineeringKnowledgeHub.Api.Controllers;

using EngineeringKnowledgeHub.Api.Models;
using EngineeringKnowledgeHub.Application.Documents;
using EngineeringKnowledgeHub.Application.DTOs;
using EngineeringKnowledgeHub.Application.Enrichment;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Endpoints for document upload, retrieval, text processing, chunking, and knowledge proposal.
/// </summary>
[ApiController]
[Route("api/documents")]
public sealed class DocumentsController(
    IDocumentService documentService,
    IExternalEnrichmentService enrichmentService) : ControllerBase
{
    private readonly IDocumentService _documentService = documentService ?? throw new ArgumentNullException(nameof(documentService));
    private readonly IExternalEnrichmentService _enrichmentService = enrichmentService ?? throw new ArgumentNullException(nameof(enrichmentService));

    /// <summary>
    /// Registers/uploads a new document attached to a knowledge source.
    /// </summary>
    [HttpPost]
    [ProducesResponseType<DocumentDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Upload(
        [FromBody] UploadDocumentRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.SourceId == Guid.Empty)
        {
            return EmptyGuidError("SourceId");
        }

        if (request.UploadedById == Guid.Empty)
        {
            return EmptyGuidError("UploadedById");
        }

        var result = await _documentService.UploadDocumentAsync(request.ToCommand(), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Retrieves a document by its unique identifier.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<DocumentDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return EmptyGuidError("Document ID");
        }

        var document = await _documentService.GetDocumentByIdAsync(id, cancellationToken);
        return Ok(document);
    }

    /// <summary>
    /// Retrieves all documents registered under a specific knowledge source.
    /// </summary>
    [HttpGet("source/{sourceId:guid}")]
    [ProducesResponseType<IReadOnlyList<DocumentDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBySource(
        Guid sourceId,
        CancellationToken cancellationToken = default)
    {
        if (sourceId == Guid.Empty)
        {
            return EmptyGuidError("Source ID");
        }

        var documents = await _documentService.GetDocumentsBySourceIdAsync(sourceId, cancellationToken);
        return Ok(documents);
    }

    /// <summary>
    /// Triggers text extraction and processing on an uploaded document.
    /// </summary>
    [HttpPost("{id:guid}/process")]
    [ProducesResponseType<DocumentDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Process(
        Guid id,
        [FromBody] ProcessDocumentRequest? request = null,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return EmptyGuidError("Document ID");
        }

        var command = (request ?? new ProcessDocumentRequest()).ToCommand(id);
        var result = await _documentService.ProcessDocumentAsync(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Chunks a processed document into semantic chunks for vector indexing and search.
    /// </summary>
    [HttpPost("{id:guid}/chunk")]
    [ProducesResponseType<IReadOnlyList<DocumentChunkDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Chunk(
        Guid id,
        [FromBody] ChunkDocumentRequest? request = null,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return EmptyGuidError("Document ID");
        }

        var result = await _documentService.ChunkDocumentAsync(id, request?.ToOptions(), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Proposes a new draft knowledge article synthesized from an imported document.
    /// Proposing creates an Unverified Draft article, strictly requiring human review before publication.
    /// </summary>
    [HttpPost("{id:guid}/propose-article")]
    [ProducesResponseType<KnowledgeArticleDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ProposeArticle(
        Guid id,
        [FromBody] ProposeArticleFromDocumentRequest request,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return EmptyGuidError("Document ID");
        }

        if (request is null)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation Error",
                Detail = "Request body cannot be null."
            });
        }

        if (request.AuthorId == Guid.Empty)
        {
            return EmptyGuidError("AuthorId");
        }

        if (request.KnowledgeTypeId == Guid.Empty)
        {
            return EmptyGuidError("KnowledgeTypeId");
        }

        var result = await _enrichmentService.ProposeArticleFromDocumentAsync(request.ToCommand(id), cancellationToken);
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
