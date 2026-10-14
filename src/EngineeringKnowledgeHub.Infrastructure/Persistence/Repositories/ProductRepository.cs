namespace EngineeringKnowledgeHub.Infrastructure.Persistence.Repositories;

using EngineeringKnowledgeHub.Application.Common.Interfaces;
using EngineeringKnowledgeHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

public sealed class ProductRepository(AppDbContext dbContext) : IProductRepository
{
    public async Task<IReadOnlyList<Product>> GetAllAsync(bool? isActive = null, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Products.AsNoTracking();
        if (isActive.HasValue)
        {
            query = query.Where(p => p.IsActive == isActive.Value);
        }

        return await query
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Products
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var normalizedCode = code.Trim().ToUpperInvariant();
        return await dbContext.Products
            .AnyAsync(p => p.Code == normalizedCode, cancellationToken);
    }

    public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        await dbContext.Products.AddAsync(product, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        dbContext.Products.Update(product);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
