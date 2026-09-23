namespace EngineeringKnowledgeHub.Application.Taxonomy;

public sealed record CreateKnowledgeCategoryCommand(
    string Name,
    string? Description = null,
    Guid? ParentCategoryId = null);

public sealed record UpdateKnowledgeCategoryCommand(
    Guid Id,
    string Name,
    string? Description = null,
    Guid? ParentCategoryId = null);

public sealed record CreateKnowledgeTypeCommand(
    string Name,
    string? Description = null);

public sealed record UpdateKnowledgeTypeCommand(
    Guid Id,
    string Name,
    string? Description = null);

public sealed record CreateKnowledgeTagCommand(
    string Name);

public sealed record UpdateKnowledgeTagCommand(
    Guid Id,
    string Name);
