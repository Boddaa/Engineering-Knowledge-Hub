namespace EngineeringKnowledgeHub.Domain.Entities;

using EngineeringKnowledgeHub.Domain.Exceptions;

/// <summary>
/// Defines the structural intent of an article (e.g. Troubleshooting, Procedure, Configuration, Reference, Note, FAQ).
/// </summary>
public sealed class KnowledgeType
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }

    public KnowledgeType(Guid id, string name, string? description = null)
    {
        if (id == Guid.Empty) throw new DomainRuleViolationException("KnowledgeType Id cannot be empty.");
        if (string.IsNullOrWhiteSpace(name)) throw new DomainRuleViolationException("KnowledgeType name cannot be null or whitespace.");

        Id = id;
        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;
    }

    public void Update(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainRuleViolationException("KnowledgeType name cannot be null or whitespace.");

        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;
    }
}
