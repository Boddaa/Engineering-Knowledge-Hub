namespace EngineeringKnowledgeHub.Infrastructure.Persistence.Repositories;

using EngineeringKnowledgeHub.Application.Common.Interfaces;
using EngineeringKnowledgeHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

public sealed class KnowledgeTagRepository(AppDbContext dbContext) : IKnowledgeTagRepository
{
    public async Task<KnowledgeTag?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.KnowledgeTags
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<KnowledgeTag?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var normalizedName = name.Trim().ToLowerInvariant();
        return await dbContext.KnowledgeTags
            .FirstOrDefaultAsync(t => t.Name == normalizedName, cancellationToken);
    }

    public async Task AddAsync(KnowledgeTag tag, CancellationToken cancellationToken = default)
    {
        await dbContext.KnowledgeTags.AddAsync(tag, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(KnowledgeTag tag, CancellationToken cancellationToken = default)
    {
        dbContext.KnowledgeTags.Update(tag);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
