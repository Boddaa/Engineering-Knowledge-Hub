namespace EngineeringKnowledgeHub.Infrastructure.Persistence.Repositories;

using EngineeringKnowledgeHub.Application.Common.Interfaces;
using EngineeringKnowledgeHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

public sealed class KnowledgeTypeRepository(AppDbContext dbContext) : IKnowledgeTypeRepository
{
    public async Task<IReadOnlyList<KnowledgeType>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.KnowledgeTypes
            .AsNoTracking()
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<KnowledgeType?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.KnowledgeTypes
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task AddAsync(KnowledgeType type, CancellationToken cancellationToken = default)
    {
        await dbContext.KnowledgeTypes.AddAsync(type, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(KnowledgeType type, CancellationToken cancellationToken = default)
    {
        dbContext.KnowledgeTypes.Update(type);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
