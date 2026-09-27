namespace EngineeringKnowledgeHub.Application.Governance;


public sealed record RecordAuditLogCommand(
    Guid UserId,
    string Action,
    string EntityName,
    Guid EntityId,
    string Details);
