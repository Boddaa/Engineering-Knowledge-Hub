namespace EngineeringKnowledgeHub.Application.DTOs;

using EngineeringKnowledgeHub.Domain.Entities;

public sealed record AuditLogDto(
    Guid Id,
    Guid UserId,
    string Action,
    string EntityName,
    Guid EntityId,
    string Details,
    DateTimeOffset CreatedAt)
{
    public static AuditLogDto FromDomain(AuditLog log) =>
        new(
            log.Id,
            log.UserId,
            log.Action,
            log.EntityName,
            log.EntityId,
            log.Details,
            log.CreatedAt);
}
