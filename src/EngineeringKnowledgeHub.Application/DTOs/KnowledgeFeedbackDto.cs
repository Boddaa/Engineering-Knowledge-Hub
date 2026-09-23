namespace EngineeringKnowledgeHub.Application.DTOs;

using EngineeringKnowledgeHub.Domain.Entities;
using EngineeringKnowledgeHub.Domain.Enums;

/// <summary>
/// Data transfer object projecting user article feedback.
/// </summary>
public sealed record KnowledgeFeedbackDto(
    Guid Id,
    Guid ArticleId,
    Guid UserId,
    FeedbackType FeedbackType,
    string? Comment,
    DateTimeOffset CreatedAt)
{
    public static KnowledgeFeedbackDto FromDomain(KnowledgeFeedback feedback) =>
        new(
            feedback.Id,
            feedback.ArticleId,
            feedback.UserId,
            feedback.FeedbackType,
            feedback.Comment,
            feedback.CreatedAt);
}
