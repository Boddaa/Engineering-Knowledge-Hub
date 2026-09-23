namespace EngineeringKnowledgeHub.Application.Common.Interfaces;

using EngineeringKnowledgeHub.Domain.Entities;

/// <summary>
/// Repository abstraction for operational incident persistence and retrieval.
/// </summary>
public interface IIncidentRepository
{
    Task<Incident?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Incident>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Incident>> GetByArticleIdAsync(Guid articleId, CancellationToken cancellationToken = default);
    Task AddAsync(Incident incident, CancellationToken cancellationToken = default);
    Task UpdateAsync(Incident incident, CancellationToken cancellationToken = default);
}
