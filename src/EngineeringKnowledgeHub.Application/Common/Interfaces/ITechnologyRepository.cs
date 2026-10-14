namespace EngineeringKnowledgeHub.Application.Common.Interfaces;

using EngineeringKnowledgeHub.Domain.Entities;

public interface ITechnologyRepository
{
    Task<IReadOnlyList<Technology>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Technology?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Technology technology, CancellationToken cancellationToken = default);
    Task UpdateAsync(Technology technology, CancellationToken cancellationToken = default);
}
