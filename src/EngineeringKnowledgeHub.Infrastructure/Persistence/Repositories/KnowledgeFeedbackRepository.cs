namespace EngineeringKnowledgeHub.Infrastructure.Persistence.Repositories;

using EngineeringKnowledgeHub.Application.Common.Interfaces;
using EngineeringKnowledgeHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

public sealed class KnowledgeFeedbackRepository(AppDbContext dbContext) : IKnowledgeFeedbackRepository
{
    public async Task AddAsync(KnowledgeFeedback feedback, CancellationToken cancellationToken = default)
    {
        await dbContext.KnowledgeFeedbacks.AddAsync(feedback, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<KnowledgeFeedback>> GetByArticleIdAsync(Guid articleId, CancellationToken cancellationToken = default)
    {
        return await dbContext.KnowledgeFeedbacks
            .Where(f => f.ArticleId == articleId)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
