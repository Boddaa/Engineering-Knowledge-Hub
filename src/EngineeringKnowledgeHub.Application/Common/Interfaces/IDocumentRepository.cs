namespace EngineeringKnowledgeHub.Application.Common.Interfaces;

using EngineeringKnowledgeHub.Domain.Entities;

public interface IDocumentRepository
{
    Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Document?> GetByHashAsync(string fileHash, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Document>> GetBySourceIdAsync(Guid sourceId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByHashAsync(string fileHash, CancellationToken cancellationToken = default);
    Task AddAsync(Document document, CancellationToken cancellationToken = default);
    Task UpdateAsync(Document document, CancellationToken cancellationToken = default);
}
