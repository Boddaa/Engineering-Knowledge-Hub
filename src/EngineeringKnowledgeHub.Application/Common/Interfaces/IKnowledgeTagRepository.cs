namespace EngineeringKnowledgeHub.Application.Common.Interfaces;

using EngineeringKnowledgeHub.Domain.Entities;

public interface IKnowledgeTagRepository
{
    Task<KnowledgeTag?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<KnowledgeTag?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task AddAsync(KnowledgeTag tag, CancellationToken cancellationToken = default);
    Task UpdateAsync(KnowledgeTag tag, CancellationToken cancellationToken = default);
}
