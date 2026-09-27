namespace EngineeringKnowledgeHub.Infrastructure.Persistence.Repositories;

using EngineeringKnowledgeHub.Application.Common.Interfaces;
using EngineeringKnowledgeHub.Domain.Entities;
using EngineeringKnowledgeHub.Domain.Enums;
using Microsoft.EntityFrameworkCore;

public sealed class KnowledgeSourceRepository(AppDbContext dbContext) : IKnowledgeSourceRepository
{
    public async Task<KnowledgeSource?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.KnowledgeSources
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<KnowledgeSource>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.KnowledgeSources
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<KnowledgeSource>> GetFilteredAsync(
        SourceType? sourceType = null,
        TrustLevel? trustLevel = null,
        bool? isApproved = null,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.KnowledgeSources.AsQueryable();

        if (sourceType.HasValue)
        {
            query = query.Where(s => s.SourceType == sourceType.Value);
        }

        if (trustLevel.HasValue)
        {
            query = query.Where(s => s.TrustLevel == trustLevel.Value);
        }

        if (isApproved.HasValue)
        {
            query = query.Where(s => s.IsApproved == isApproved.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var normalizedName = name.Trim();
        return await dbContext.KnowledgeSources
            .AnyAsync(s => s.Name == normalizedName, cancellationToken);
    }

    public async Task AddAsync(KnowledgeSource source, CancellationToken cancellationToken = default)
    {
        await dbContext.KnowledgeSources.AddAsync(source, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(KnowledgeSource source, CancellationToken cancellationToken = default)
    {
        dbContext.KnowledgeSources.Update(source);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
