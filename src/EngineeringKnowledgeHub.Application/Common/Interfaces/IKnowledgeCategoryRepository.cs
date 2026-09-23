namespace EngineeringKnowledgeHub.Application.Common.Interfaces;

using EngineeringKnowledgeHub.Domain.Entities;

public interface IKnowledgeCategoryRepository
{
    Task<KnowledgeCategory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(KnowledgeCategory category, CancellationToken cancellationToken = default);
    Task UpdateAsync(KnowledgeCategory category, CancellationToken cancellationToken = default);
}
