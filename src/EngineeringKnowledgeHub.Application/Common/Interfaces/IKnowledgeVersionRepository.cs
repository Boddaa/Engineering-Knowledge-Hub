namespace EngineeringKnowledgeHub.Application.Common.Interfaces;

using EngineeringKnowledgeHub.Domain.Entities;

public interface IKnowledgeVersionRepository
{
    Task<KnowledgeVersion?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<KnowledgeVersion?> GetByArticleAndVersionNumberAsync(Guid articleId, int versionNumber, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<KnowledgeVersion>> GetByArticleIdAsync(Guid articleId, CancellationToken cancellationToken = default);
    Task<int> GetLatestVersionNumberAsync(Guid articleId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid articleId, int versionNumber, CancellationToken cancellationToken = default);
    Task AddAsync(KnowledgeVersion version, CancellationToken cancellationToken = default);
}
