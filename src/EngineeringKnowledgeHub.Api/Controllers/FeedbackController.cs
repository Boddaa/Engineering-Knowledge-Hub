namespace EngineeringKnowledgeHub.Api.Controllers;

using EngineeringKnowledgeHub.Api.Models;
using EngineeringKnowledgeHub.Application.DTOs;
using EngineeringKnowledgeHub.Application.Feedback;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Endpoints for submitting helpfulness ratings and retrieving user feedback on published knowledge articles.
/// </summary>
[ApiController]
[Route("api/feedback")]
public sealed class FeedbackController(IKnowledgeFeedbackService feedbackService) : ControllerBase
{
    private readonly IKnowledgeFeedbackService _feedbackService = feedbackService ?? throw new ArgumentNullException(nameof(feedbackService));

    /// <summary>
    /// Submits helpfulness rating and feedback for a published knowledge article.
    /// </summary>
    [HttpPost]
    [HttpPost("/api/knowledge/articles/{articleId:guid}/feedback")]
    [ProducesResponseType<KnowledgeFeedbackDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SubmitFeedback(
        [FromBody] SubmitFeedbackRequest request,
        Guid? articleId = null,
        CancellationToken cancellationToken = default)
    {
        var targetArticleId = articleId ?? request.ArticleId;

        if (targetArticleId == Guid.Empty)
        {
            return EmptyGuidError("ArticleId");
        }

        if (request.UserId == Guid.Empty)
        {
            return EmptyGuidError("UserId");
        }

        var command = (request with { ArticleId = targetArticleId }).ToCommand();
        var result = await _feedbackService.SubmitFeedbackAsync(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves all user feedback recorded for a specific knowledge article.
    /// </summary>
    [HttpGet("article/{articleId:guid}")]
    [HttpGet("/api/knowledge/articles/{articleId:guid}/feedback")]
    [ProducesResponseType<IReadOnlyList<KnowledgeFeedbackDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetFeedbackForArticle(
        Guid articleId,
        CancellationToken cancellationToken = default)
    {
        if (articleId == Guid.Empty)
        {
            return EmptyGuidError("Article ID");
        }

        var result = await _feedbackService.GetFeedbackForArticleAsync(articleId, cancellationToken);
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
