namespace EngineeringKnowledgeHub.Infrastructure.Persistence.Repositories;

using EngineeringKnowledgeHub.Application.Common.Interfaces;
using EngineeringKnowledgeHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

public sealed class KnowledgeCategoryRepository(AppDbContext dbContext) : IKnowledgeCategoryRepository
{
    public async Task<KnowledgeCategory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.KnowledgeCategories
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task AddAsync(KnowledgeCategory category, CancellationToken cancellationToken = default)
    {
        await dbContext.KnowledgeCategories.AddAsync(category, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(KnowledgeCategory category, CancellationToken cancellationToken = default)
    {
        dbContext.KnowledgeCategories.Update(category);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
