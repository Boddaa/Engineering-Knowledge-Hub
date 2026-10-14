namespace EngineeringKnowledgeHub.Application.ProductsAndTechnologies;

using EngineeringKnowledgeHub.Application.DTOs;

public interface IProductService
{
    Task<ProductDto> CreateProductAsync(CreateProductCommand command, CancellationToken cancellationToken = default);
    Task<ProductDto> UpdateProductAsync(UpdateProductCommand command, CancellationToken cancellationToken = default);
    Task<ProductDto> ActivateProductAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ProductDto> DeactivateProductAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ProductDto> GetProductByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductDto>> GetProductsAsync(bool? isActive = null, CancellationToken cancellationToken = default);
}
