namespace EngineeringKnowledgeHub.Domain.Entities;

using EngineeringKnowledgeHub.Domain.Exceptions;

/// <summary>
/// Freeform tag used to index and relate technical articles across domains.
/// </summary>
public sealed class KnowledgeTag
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }

    public KnowledgeTag(Guid id, string name)
    {
        if (id == Guid.Empty) throw new DomainRuleViolationException("Tag Id cannot be empty.");
        if (string.IsNullOrWhiteSpace(name)) throw new DomainRuleViolationException("Tag name cannot be null or whitespace.");

        Id = id;
        Name = name.Trim().ToLowerInvariant();
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainRuleViolationException("Tag name cannot be null or whitespace.");
        Name = name.Trim().ToLowerInvariant();
    }
}
