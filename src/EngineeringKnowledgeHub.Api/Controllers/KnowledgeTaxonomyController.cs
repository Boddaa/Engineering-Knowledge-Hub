namespace EngineeringKnowledgeHub.Api.Controllers;

using EngineeringKnowledgeHub.Api.Models;
using EngineeringKnowledgeHub.Application.DTOs;
using EngineeringKnowledgeHub.Application.Taxonomy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Endpoints for managing the knowledge taxonomy (categories, types, and tags).
/// </summary>
[ApiController]
[Route("api/taxonomy")]
public sealed class KnowledgeTaxonomyController(IKnowledgeTaxonomyService taxonomyService) : ControllerBase
{
    private readonly IKnowledgeTaxonomyService _taxonomyService = taxonomyService ?? throw new ArgumentNullException(nameof(taxonomyService));

    // ================= Categories =================

    /// <summary>
    /// Creates a new knowledge category.
    /// </summary>
    [HttpPost("categories")]
    [ProducesResponseType<KnowledgeCategoryDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateCategory(
        [FromBody] CreateCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _taxonomyService.CreateCategoryAsync(request.ToCommand(), cancellationToken);
        return CreatedAtAction(nameof(GetCategoryById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Retrieves a knowledge category by its unique identifier.
    /// </summary>
    [HttpGet("categories/{id:guid}")]
    [ProducesResponseType<KnowledgeCategoryDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCategoryById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return EmptyGuidError("Category ID");
        }

        var result = await _taxonomyService.GetCategoryByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Updates the metadata of an existing knowledge category.
    /// </summary>
    [HttpPut("categories/{id:guid}")]
    [ProducesResponseType<KnowledgeCategoryDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateCategory(
        Guid id,
        [FromBody] UpdateCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return EmptyGuidError("Category ID");
        }

        var result = await _taxonomyService.UpdateCategoryAsync(request.ToCommand(id), cancellationToken);
        return Ok(result);
    }

    // ================= Knowledge Types =================

    /// <summary>
    /// Creates a new knowledge type defining the structural intent of articles.
    /// </summary>
    [HttpPost("types")]
    [ProducesResponseType<KnowledgeTypeDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateType(
        [FromBody] CreateKnowledgeTypeRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _taxonomyService.CreateTypeAsync(request.ToCommand(), cancellationToken);
        return CreatedAtAction(nameof(GetTypeById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Retrieves a knowledge type by its unique identifier.
    /// </summary>
    [HttpGet("types/{id:guid}")]
    [ProducesResponseType<KnowledgeTypeDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTypeById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return EmptyGuidError("KnowledgeType ID");
        }

        var result = await _taxonomyService.GetTypeByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Updates the metadata of an existing knowledge type.
    /// </summary>
    [HttpPut("types/{id:guid}")]
    [ProducesResponseType<KnowledgeTypeDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateType(
        Guid id,
        [FromBody] UpdateKnowledgeTypeRequest request,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return EmptyGuidError("KnowledgeType ID");
        }

        var result = await _taxonomyService.UpdateTypeAsync(request.ToCommand(id), cancellationToken);
        return Ok(result);
    }

    // ================= Knowledge Tags =================

    /// <summary>
    /// Creates a new knowledge tag for freeform cross-domain classification.
    /// </summary>
    [HttpPost("tags")]
    [ProducesResponseType<KnowledgeTagDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateTag(
        [FromBody] CreateKnowledgeTagRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _taxonomyService.CreateTagAsync(request.ToCommand(), cancellationToken);
        return CreatedAtAction(nameof(GetTagById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Retrieves a knowledge tag by its unique identifier.
    /// </summary>
    [HttpGet("tags/{id:guid}")]
    [ProducesResponseType<KnowledgeTagDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTagById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return EmptyGuidError("Tag ID");
        }

        var result = await _taxonomyService.GetTagByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Updates the label of an existing knowledge tag.
    /// </summary>
    [HttpPut("tags/{id:guid}")]
    [ProducesResponseType<KnowledgeTagDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateTag(
        Guid id,
        [FromBody] UpdateKnowledgeTagRequest request,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return EmptyGuidError("Tag ID");
        }

        var result = await _taxonomyService.UpdateTagAsync(request.ToCommand(id), cancellationToken);
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
