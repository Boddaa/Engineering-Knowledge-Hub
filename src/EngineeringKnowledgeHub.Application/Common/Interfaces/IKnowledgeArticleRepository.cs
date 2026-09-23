namespace EngineeringKnowledgeHub.Application.Common.Interfaces;

using EngineeringKnowledgeHub.Domain.Entities;
using EngineeringKnowledgeHub.Domain.ValueObjects;

public interface IKnowledgeArticleRepository
{
    Task<KnowledgeArticle?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<KnowledgeArticle>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsBySlugAsync(Slug slug, CancellationToken cancellationToken = default);
    Task AddAsync(KnowledgeArticle article, CancellationToken cancellationToken = default);
    Task UpdateAsync(KnowledgeArticle article, CancellationToken cancellationToken = default);
}
