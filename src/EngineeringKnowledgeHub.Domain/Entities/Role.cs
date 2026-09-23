namespace EngineeringKnowledgeHub.Domain.Entities;

using EngineeringKnowledgeHub.Domain.Exceptions;

/// <summary>
/// Security role defining authorization permissions (e.g. Admin, KnowledgeAdmin, SeniorEngineer, Engineer, SupportEngineer, Viewer).
/// </summary>
public sealed class Role
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }

    public Role(Guid id, string name, string? description = null)
    {
        if (id == Guid.Empty) throw new DomainRuleViolationException("Role Id cannot be empty.");
        if (string.IsNullOrWhiteSpace(name)) throw new DomainRuleViolationException("Role name cannot be null or whitespace.");

        Id = id;
        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;
    }

    public void Update(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainRuleViolationException("Role name cannot be null or whitespace.");

        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;
    }
}
