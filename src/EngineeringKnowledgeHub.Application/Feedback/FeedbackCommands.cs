namespace EngineeringKnowledgeHub.Application.Feedback;

using EngineeringKnowledgeHub.Domain.Enums;

public sealed record SubmitFeedbackCommand(
    Guid ArticleId,
    Guid UserId,
    FeedbackType FeedbackType,
    string? Comment = null);
