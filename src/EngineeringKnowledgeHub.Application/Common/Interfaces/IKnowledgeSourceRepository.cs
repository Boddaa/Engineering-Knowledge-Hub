namespace EngineeringKnowledgeHub.Application.Common.Interfaces;

using EngineeringKnowledgeHub.Domain.Entities;
using EngineeringKnowledgeHub.Domain.Enums;

public interface IKnowledgeSourceRepository
{
    Task<KnowledgeSource?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<KnowledgeSource>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<KnowledgeSource>> GetFilteredAsync(
        SourceType? sourceType = null,
        TrustLevel? trustLevel = null,
        bool? isApproved = null,
        CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
    Task AddAsync(KnowledgeSource source, CancellationToken cancellationToken = default);
    Task UpdateAsync(KnowledgeSource source, CancellationToken cancellationToken = default);
}
