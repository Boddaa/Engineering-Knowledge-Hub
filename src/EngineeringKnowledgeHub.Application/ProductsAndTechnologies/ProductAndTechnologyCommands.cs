namespace EngineeringKnowledgeHub.Application.ProductsAndTechnologies;

public sealed record CreateProductCommand(
    string Name,
    string Code,
    string? Description = null,
    bool IsActive = true);

public sealed record UpdateProductCommand(
    Guid Id,
    string Name,
    string Code,
    string? Description = null);

public sealed record CreateTechnologyCommand(
    string Name,
    string? Description = null);

public sealed record UpdateTechnologyCommand(
    Guid Id,
    string Name,
    string? Description = null);
