namespace EngineeringKnowledgeHub.Domain.Entities;

using EngineeringKnowledgeHub.Domain.Exceptions;

/// <summary>
/// Represents a company software or hardware system/project (e.g. Toll Collection, ANPR, Barrier Control).
/// </summary>
public sealed class Product
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Code { get; private set; }
    public string Description { get; private set; }
    public bool IsActive { get; private set; }

    public Product(Guid id, string name, string code, string? description = null, bool isActive = true)
    {
        if (id == Guid.Empty) throw new DomainRuleViolationException("Product Id cannot be empty.");
        if (string.IsNullOrWhiteSpace(name)) throw new DomainRuleViolationException("Product name cannot be null or whitespace.");
        if (string.IsNullOrWhiteSpace(code)) throw new DomainRuleViolationException("Product code cannot be null or whitespace.");

        Id = id;
        Name = name.Trim();
        Code = code.Trim().ToUpperInvariant();
        Description = description?.Trim() ?? string.Empty;
        IsActive = isActive;
    }

    public void Update(string name, string code, string? description)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainRuleViolationException("Product name cannot be null or whitespace.");
        if (string.IsNullOrWhiteSpace(code)) throw new DomainRuleViolationException("Product code cannot be null or whitespace.");

        Name = name.Trim();
        Code = code.Trim().ToUpperInvariant();
        Description = description?.Trim() ?? string.Empty;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
