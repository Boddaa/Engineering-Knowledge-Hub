namespace EngineeringKnowledgeHub.Infrastructure.Persistence.Repositories;

using EngineeringKnowledgeHub.Application.Common.Exceptions;
using EngineeringKnowledgeHub.Application.Common.Interfaces;
using EngineeringKnowledgeHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

public sealed class KnowledgeVersionRepository(AppDbContext dbContext) : IKnowledgeVersionRepository
{
    public async Task<KnowledgeVersion?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.KnowledgeVersions
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
    }

    public async Task<KnowledgeVersion?> GetByArticleAndVersionNumberAsync(Guid articleId, int versionNumber, CancellationToken cancellationToken = default)
    {
        return await dbContext.KnowledgeVersions
            .FirstOrDefaultAsync(v => v.ArticleId == articleId && v.VersionNumber == versionNumber, cancellationToken);
    }

    public async Task<IReadOnlyList<KnowledgeVersion>> GetByArticleIdAsync(Guid articleId, CancellationToken cancellationToken = default)
    {
        return await dbContext.KnowledgeVersions
            .Where(v => v.ArticleId == articleId)
            .OrderByDescending(v => v.VersionNumber)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetLatestVersionNumberAsync(Guid articleId, CancellationToken cancellationToken = default)
    {
        var max = await dbContext.KnowledgeVersions
            .Where(v => v.ArticleId == articleId)
            .Select(v => (int?)v.VersionNumber)
            .MaxAsync(cancellationToken);

        return max ?? 0;
    }

    public async Task<bool> ExistsAsync(Guid articleId, int versionNumber, CancellationToken cancellationToken = default)
    {
        return await dbContext.KnowledgeVersions
            .AnyAsync(v => v.ArticleId == articleId && v.VersionNumber == versionNumber, cancellationToken);
    }

    public async Task AddAsync(KnowledgeVersion version, CancellationToken cancellationToken = default)
    {
        if (await ExistsAsync(version.ArticleId, version.VersionNumber, cancellationToken))
        {
            throw new ConflictException($"Version {version.VersionNumber} already exists for article '{version.ArticleId}'.");
        }

        await dbContext.KnowledgeVersions.AddAsync(version, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
