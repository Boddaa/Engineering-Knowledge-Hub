namespace EngineeringKnowledgeHub.Domain.Entities;

using EngineeringKnowledgeHub.Domain.Exceptions;

/// <summary>
/// Immutable historical record of an important governance action performed on an entity.
/// Captures who performed what action on which entity, when, and with what details.
/// </summary>
public sealed class AuditLog
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Action { get; private set; }
    public string EntityName { get; private set; }
    public Guid EntityId { get; private set; }
    public string Details { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public AuditLog(
        Guid id,
        Guid userId,
        string action,
        string entityName,
        Guid entityId,
        string details,
        DateTimeOffset createdAt)
    {
        if (id == Guid.Empty)
        {
            throw new DomainRuleViolationException("AuditLog Id cannot be empty.");
        }

        if (userId == Guid.Empty)
        {
            throw new DomainRuleViolationException("User Id cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(action))
        {
            throw new DomainRuleViolationException("Audit action cannot be null or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(entityName))
        {
            throw new DomainRuleViolationException("Entity name cannot be null or whitespace.");
        }

        if (entityId == Guid.Empty)
        {
            throw new DomainRuleViolationException("Entity Id cannot be empty.");
        }

        Id = id;
        UserId = userId;
        Action = action.Trim();
        EntityName = entityName.Trim();
        EntityId = entityId;
        Details = details?.Trim() ?? string.Empty;
        CreatedAt = createdAt;
    }
}
