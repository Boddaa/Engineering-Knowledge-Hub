namespace EngineeringKnowledgeHub.Domain.Entities;

using EngineeringKnowledgeHub.Domain.Enums;
using EngineeringKnowledgeHub.Domain.Exceptions;

/// <summary>
/// User feedback and helpfulness rating on a published knowledge article.
/// </summary>
public sealed class KnowledgeFeedback
{
    public Guid Id { get; private set; }
    public Guid ArticleId { get; private set; }
    public Guid UserId { get; private set; }
    public FeedbackType FeedbackType { get; private set; }
    public string? Comment { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public KnowledgeFeedback(
        Guid id,
        Guid articleId,
        Guid userId,
        FeedbackType feedbackType,
        DateTimeOffset createdAt,
        string? comment = null)
    {
        if (id == Guid.Empty) throw new DomainRuleViolationException("Feedback Id cannot be empty.");
        if (articleId == Guid.Empty) throw new DomainRuleViolationException("Article Id cannot be empty.");
        if (userId == Guid.Empty) throw new DomainRuleViolationException("User Id cannot be empty.");
        if (!Enum.IsDefined(typeof(FeedbackType), feedbackType))
        {
            throw new DomainRuleViolationException("Invalid feedback type.");
        }

        Id = id;
        ArticleId = articleId;
        UserId = userId;
        FeedbackType = feedbackType;
        CreatedAt = createdAt;
        Comment = comment?.Trim();
    }
}
