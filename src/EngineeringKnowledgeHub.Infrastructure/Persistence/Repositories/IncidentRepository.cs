namespace EngineeringKnowledgeHub.Infrastructure.Persistence.Repositories;

using EngineeringKnowledgeHub.Application.Common.Interfaces;
using EngineeringKnowledgeHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

public sealed class IncidentRepository(AppDbContext dbContext) : IIncidentRepository
{
    public async Task<Incident?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Incidents
            .Include(i => i.ArticleLinks)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Incident>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Incidents
            .Include(i => i.ArticleLinks)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Incident>> GetByArticleIdAsync(Guid articleId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Incidents
            .Include(i => i.ArticleLinks)
            .Where(i => i.ArticleLinks.Any(l => l.ArticleId == articleId))
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Incident incident, CancellationToken cancellationToken = default)
    {
        await dbContext.Incidents.AddAsync(incident, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Incident incident, CancellationToken cancellationToken = default)
    {
        dbContext.Incidents.Update(incident);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
