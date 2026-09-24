namespace EngineeringKnowledgeHub.Domain.Constants;

/// <summary>
/// Explicit, standardized action identifiers for governance audit logs.
/// Avoids magic strings across domain and application layers.
/// </summary>
public static class GovernanceAuditActions
{
    public const string ArticleSubmittedForReview = "ArticleSubmittedForReview";
    public const string ArticleApproved = "ArticleApproved";
    public const string ArticleRejected = "ArticleRejected";
    public const string ArticleDeprecated = "ArticleDeprecated";
    public const string ArticleArchived = "ArticleArchived";
    public const string KnowledgeVersionCreated = "KnowledgeVersionCreated";
}
