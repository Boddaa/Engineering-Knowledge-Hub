namespace EngineeringKnowledgeHub.Infrastructure.Persistence.Repositories;

using EngineeringKnowledgeHub.Application.Common.Interfaces;
using EngineeringKnowledgeHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

public sealed class TechnologyRepository(AppDbContext dbContext) : ITechnologyRepository
{
    public async Task<Technology?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Technologies
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task AddAsync(Technology technology, CancellationToken cancellationToken = default)
    {
        await dbContext.Technologies.AddAsync(technology, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Technology technology, CancellationToken cancellationToken = default)
    {
        dbContext.Technologies.Update(technology);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
