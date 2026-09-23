namespace EngineeringKnowledgeHub.Application.DTOs;

using EngineeringKnowledgeHub.Domain.Entities;

public sealed record KnowledgeCategoryDto(
    Guid Id,
    string Name,
    string Description,
    Guid? ParentCategoryId)
{
    public static KnowledgeCategoryDto FromDomain(KnowledgeCategory category) =>
        new(category.Id, category.Name, category.Description, category.ParentCategoryId);
}
