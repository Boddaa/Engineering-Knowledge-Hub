namespace EngineeringKnowledgeHub.Api.Models;

using EngineeringKnowledgeHub.Application.Feedback;
using EngineeringKnowledgeHub.Domain.Enums;

/// <summary>
/// HTTP request contract for submitting helpfulness feedback on a published article.
/// </summary>
public sealed record SubmitFeedbackRequest(
    Guid ArticleId,
    Guid UserId,
    FeedbackType FeedbackType,
    string? Comment = null)
{
    public SubmitFeedbackCommand ToCommand() =>
        new(ArticleId, UserId, FeedbackType, Comment);
}
