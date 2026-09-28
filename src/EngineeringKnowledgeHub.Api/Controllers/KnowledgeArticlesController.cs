namespace EngineeringKnowledgeHub.Api.Controllers;

using EngineeringKnowledgeHub.Api.Models;
using EngineeringKnowledgeHub.Application.Articles;
using EngineeringKnowledgeHub.Application.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Endpoints for authoring, lifecycle management, and metadata association of knowledge articles.
/// </summary>
[ApiController]
[Route("api/knowledge/articles")]
public sealed class KnowledgeArticlesController(IKnowledgeArticleService articleService) : ControllerBase
{
    private readonly IKnowledgeArticleService _articleService = articleService ?? throw new ArgumentNullException(nameof(articleService));

    /// <summary>
    /// Creates a new knowledge article in Draft status.
    /// </summary>
    [HttpPost]
    [ProducesResponseType<KnowledgeArticleDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateArticleRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.KnowledgeTypeId == Guid.Empty)
        {
            return EmptyGuidError("KnowledgeTypeId");
        }

        if (request.AuthorId == Guid.Empty)
        {
            return EmptyGuidError("AuthorId");
        }

        var result = await _articleService.CreateArticleAsync(request.ToCommand(), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Retrieves a knowledge article by its unique identifier.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<KnowledgeArticleDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return EmptyGuidError("Article ID");
        }

        var article = await _articleService.GetArticleByIdAsync(id, cancellationToken);
        return Ok(article);
    }

    /// <summary>
    /// Updates the editable content and metadata of a draft knowledge article.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<KnowledgeArticleDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateArticleRequest request,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return EmptyGuidError("Article ID");
        }

        var command = new UpdateKnowledgeArticleCommand(id, request.Title, request.Summary, request.Content, request.Slug);
        var result = await _articleService.UpdateArticleAsync(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Submits a draft knowledge article for peer review.
    /// </summary>
    [HttpPost("{id:guid}/submit-review")]
    [ProducesResponseType<KnowledgeArticleDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SubmitForReview(
        Guid id,
        [FromBody] SubmitArticleReviewRequest? request = null,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return EmptyGuidError("Article ID");
        }

        var result = request?.SubmitterId.HasValue == true && request.SubmitterId.Value != Guid.Empty
            ? await _articleService.SubmitForReviewAsync(id, request.SubmitterId.Value, cancellationToken)
            : await _articleService.SubmitForReviewAsync(id, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Approves an article in review, atomically publishing it and generating a historical version snapshot.
    /// </summary>
    [HttpPost("{id:guid}/approve")]
    [ProducesResponseType<KnowledgeArticleDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Approve(
        Guid id,
        [FromBody] ApproveArticleRequest request,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return EmptyGuidError("Article ID");
        }

        if (request.ReviewerId == Guid.Empty)
        {
            return EmptyGuidError("Reviewer ID");
        }

        var command = new ApproveKnowledgeArticleCommand(id, request.ReviewerId, request.ChangeSummary);
        var result = await _articleService.ApproveArticleAsync(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Rejects an article in review, returning it to Draft with feedback.
    /// </summary>
    [HttpPost("{id:guid}/reject")]
    [ProducesResponseType<KnowledgeArticleDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Reject(
        Guid id,
        [FromBody] RejectArticleRequest request,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return EmptyGuidError("Article ID");
        }

        if (request.ReviewerId == Guid.Empty)
        {
            return EmptyGuidError("Reviewer ID");
        }

        var command = new RejectKnowledgeArticleCommand(id, request.ReviewerId);
        var result = await _articleService.RejectArticleAsync(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Deprecates a published article.
    /// </summary>
    [HttpPost("{id:guid}/deprecate")]
    [ProducesResponseType<KnowledgeArticleDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Deprecate(
        Guid id,
        [FromBody] DeprecateArticleRequest? request = null,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return EmptyGuidError("Article ID");
        }

        var result = request?.ActorId.HasValue == true && request.ActorId.Value != Guid.Empty
            ? await _articleService.DeprecateArticleAsync(id, request.ActorId.Value, cancellationToken)
            : await _articleService.DeprecateArticleAsync(id, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Archives a knowledge article.
    /// </summary>
    [HttpPost("{id:guid}/archive")]
    [ProducesResponseType<KnowledgeArticleDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Archive(
        Guid id,
        [FromBody] ArchiveArticleRequest? request = null,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return EmptyGuidError("Article ID");
        }

        var result = request?.ActorId.HasValue == true && request.ActorId.Value != Guid.Empty
            ? await _articleService.ArchiveArticleAsync(id, request.ActorId.Value, cancellationToken)
            : await _articleService.ArchiveArticleAsync(id, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Associates a tag with an article.
    /// </summary>
    [HttpPost("{id:guid}/tags/{tagId:guid}")]
    [ProducesResponseType<KnowledgeArticleDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddTag(
        Guid id,
        Guid tagId,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return EmptyGuidError("Article ID");
        }

        if (tagId == Guid.Empty)
        {
            return EmptyGuidError("Tag ID");
        }

        var result = await _articleService.AddTagAsync(id, tagId, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Removes a tag association from an article.
    /// </summary>
    [HttpDelete("{id:guid}/tags/{tagId:guid}")]
    [ProducesResponseType<KnowledgeArticleDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveTag(
        Guid id,
        Guid tagId,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return EmptyGuidError("Article ID");
        }

        if (tagId == Guid.Empty)
        {
            return EmptyGuidError("Tag ID");
        }

        var result = await _articleService.RemoveTagAsync(id, tagId, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Associates a product with an article.
    /// </summary>
    [HttpPost("{id:guid}/products/{productId:guid}")]
    [ProducesResponseType<KnowledgeArticleDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddProduct(
        Guid id,
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return EmptyGuidError("Article ID");
        }

        if (productId == Guid.Empty)
        {
            return EmptyGuidError("Product ID");
        }

        var result = await _articleService.AddProductAsync(id, productId, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Removes a product association from an article.
    /// </summary>
    [HttpDelete("{id:guid}/products/{productId:guid}")]
    [ProducesResponseType<KnowledgeArticleDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveProduct(
        Guid id,
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return EmptyGuidError("Article ID");
        }

        if (productId == Guid.Empty)
        {
            return EmptyGuidError("Product ID");
        }

        var result = await _articleService.RemoveProductAsync(id, productId, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Associates a technology with an article.
    /// </summary>
    [HttpPost("{id:guid}/technologies/{technologyId:guid}")]
    [ProducesResponseType<KnowledgeArticleDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddTechnology(
        Guid id,
        Guid technologyId,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return EmptyGuidError("Article ID");
        }

        if (technologyId == Guid.Empty)
        {
            return EmptyGuidError("Technology ID");
        }

        var result = await _articleService.AddTechnologyAsync(id, technologyId, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Removes a technology association from an article.
    /// </summary>
    [HttpDelete("{id:guid}/technologies/{technologyId:guid}")]
    [ProducesResponseType<KnowledgeArticleDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveTechnology(
        Guid id,
        Guid technologyId,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return EmptyGuidError("Article ID");
        }

        if (technologyId == Guid.Empty)
        {
            return EmptyGuidError("Technology ID");
        }

        var result = await _articleService.RemoveTechnologyAsync(id, technologyId, cancellationToken);
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
