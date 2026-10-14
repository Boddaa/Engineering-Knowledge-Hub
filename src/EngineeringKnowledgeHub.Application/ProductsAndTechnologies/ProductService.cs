namespace EngineeringKnowledgeHub.Application.ProductsAndTechnologies;

using EngineeringKnowledgeHub.Application.Common.Exceptions;
using EngineeringKnowledgeHub.Application.Common.Interfaces;
using EngineeringKnowledgeHub.Application.DTOs;
using EngineeringKnowledgeHub.Domain.Entities;

public sealed class ProductService(IProductRepository productRepository) : IProductService
{
    public async Task<ProductDto> CreateProductAsync(
        CreateProductCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.Name))
        {
            throw new ValidationException("Product name is required.");
        }

        if (string.IsNullOrWhiteSpace(command.Code))
        {
            throw new ValidationException("Product code is required.");
        }

        var normalizedCode = command.Code.Trim().ToUpperInvariant();
        if (await productRepository.ExistsByCodeAsync(normalizedCode, cancellationToken))
        {
            throw new ConflictException($"Product with code '{normalizedCode}' already exists.");
        }

        var product = new Product(
            Guid.NewGuid(),
            command.Name,
            normalizedCode,
            command.Description,
            command.IsActive);

        await productRepository.AddAsync(product, cancellationToken);

        return ProductDto.FromDomain(product);
    }

    public async Task<ProductDto> UpdateProductAsync(
        UpdateProductCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.Id == Guid.Empty) throw new ValidationException("Product Id cannot be empty.");
        if (string.IsNullOrWhiteSpace(command.Name)) throw new ValidationException("Product name is required.");
        if (string.IsNullOrWhiteSpace(command.Code)) throw new ValidationException("Product code is required.");

        var product = await productRepository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new NotFoundException("Product", command.Id);

        var normalizedCode = command.Code.Trim().ToUpperInvariant();
        if (!string.Equals(product.Code, normalizedCode, StringComparison.OrdinalIgnoreCase))
        {
            if (await productRepository.ExistsByCodeAsync(normalizedCode, cancellationToken))
            {
                throw new ConflictException($"Product with code '{normalizedCode}' already exists.");
            }
        }

        product.Update(command.Name, normalizedCode, command.Description);

        await productRepository.UpdateAsync(product, cancellationToken);

        return ProductDto.FromDomain(product);
    }

    public async Task<ProductDto> ActivateProductAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty) throw new ValidationException("Product Id cannot be empty.");

        var product = await productRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Product", id);

        product.Activate();

        await productRepository.UpdateAsync(product, cancellationToken);

        return ProductDto.FromDomain(product);
    }

    public async Task<ProductDto> DeactivateProductAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty) throw new ValidationException("Product Id cannot be empty.");

        var product = await productRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Product", id);

        product.Deactivate();

        await productRepository.UpdateAsync(product, cancellationToken);

        return ProductDto.FromDomain(product);
    }

    public async Task<ProductDto> GetProductByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty) throw new ValidationException("Product Id cannot be empty.");

        var product = await productRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Product", id);

        return ProductDto.FromDomain(product);
    }

    public async Task<IReadOnlyList<ProductDto>> GetProductsAsync(
        bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var products = await productRepository.GetAllAsync(isActive, cancellationToken);
        return products.Select(ProductDto.FromDomain).ToList();
    }
}
