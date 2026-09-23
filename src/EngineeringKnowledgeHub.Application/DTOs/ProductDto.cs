namespace EngineeringKnowledgeHub.Application.DTOs;

using EngineeringKnowledgeHub.Domain.Entities;

public sealed record ProductDto(
    Guid Id,
    string Name,
    string Code,
    string Description,
    bool IsActive)
{
    public static ProductDto FromDomain(Product product) =>
        new(product.Id, product.Name, product.Code, product.Description, product.IsActive);
}
