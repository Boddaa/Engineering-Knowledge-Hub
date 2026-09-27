namespace EngineeringKnowledgeHub.Infrastructure.Persistence.Repositories;

using EngineeringKnowledgeHub.Application.Common.Interfaces;
using EngineeringKnowledgeHub.Domain.Entities;
using EngineeringKnowledgeHub.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

public sealed class KnowledgeArticleRepository(AppDbContext dbContext) : IKnowledgeArticleRepository
{
    public async Task<KnowledgeArticle?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.KnowledgeArticles
            .Include(a => a.SourceLinks)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<KnowledgeArticle>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.KnowledgeArticles
            .Include(a => a.SourceLinks)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsBySlugAsync(Slug slug, CancellationToken cancellationToken = default)
    {
        return await dbContext.KnowledgeArticles
            .AnyAsync(a => a.Slug == slug, cancellationToken);
    }

    public async Task AddAsync(KnowledgeArticle article, CancellationToken cancellationToken = default)
    {
        await dbContext.KnowledgeArticles.AddAsync(article, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(KnowledgeArticle article, CancellationToken cancellationToken = default)
    {
        dbContext.KnowledgeArticles.Update(article);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
