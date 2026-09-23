namespace EngineeringKnowledgeHub.Domain.Enums;

/// <summary>
/// Represents the editorial and publication state of a knowledge article.
/// </summary>
public enum KnowledgeStatus
{
    Draft = 1,
    PendingReview = 2,
    Verified = 3,
    Published = 4,
    Deprecated = 5,
    Archived = 6
}
