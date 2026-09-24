namespace EngineeringKnowledgeHub.Application.Governance;

public sealed record CreateKnowledgeVersionCommand(
    Guid ArticleId,
    Guid ChangedById,
    string ChangeSummary,
    string? Title = null,
    string? Content = null,
    int? VersionNumber = null);

public sealed record RecordAuditLogCommand(
    Guid UserId,
    string Action,
    string EntityName,
    Guid EntityId,
    string Details);
