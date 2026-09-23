namespace EngineeringKnowledgeHub.Application.Common.Interfaces;

using EngineeringKnowledgeHub.Domain.Entities;

public interface IKnowledgeTypeRepository
{
    Task<KnowledgeType?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(KnowledgeType type, CancellationToken cancellationToken = default);
    Task UpdateAsync(KnowledgeType type, CancellationToken cancellationToken = default);
}
