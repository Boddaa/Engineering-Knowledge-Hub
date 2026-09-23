namespace EngineeringKnowledgeHub.Domain.Entities;

using EngineeringKnowledgeHub.Domain.Exceptions;

/// <summary>
/// Hierarchical classification category for knowledge organization.
/// </summary>
public sealed class KnowledgeCategory
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public Guid? ParentCategoryId { get; private set; }

    public KnowledgeCategory(Guid id, string name, string? description = null, Guid? parentCategoryId = null)
    {
        if (id == Guid.Empty) throw new DomainRuleViolationException("Category Id cannot be empty.");
        ValidateName(name);

        if (parentCategoryId == id)
        {
            throw new DomainRuleViolationException("Category cannot be its own parent.");
        }

        Id = id;
        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;
        ParentCategoryId = parentCategoryId;
    }

    public void Update(string name, string? description, Guid? parentCategoryId)
    {
        ValidateName(name);

        if (parentCategoryId == Id)
        {
            throw new DomainRuleViolationException("Category cannot be its own parent.");
        }

        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;
        ParentCategoryId = parentCategoryId;
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainRuleViolationException("Category name cannot be null or whitespace.");
        }

        if (name.Trim().Length > 100)
        {
            throw new DomainRuleViolationException("Category name exceeds maximum length of 100 characters.");
        }
    }
}
