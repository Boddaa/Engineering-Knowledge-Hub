namespace EngineeringKnowledgeHub.Application.Governance;

using EngineeringKnowledgeHub.Application.DTOs;

public interface IKnowledgeGovernanceService
{
    Task<KnowledgeVersionDto> CreateVersionAsync(CreateKnowledgeVersionCommand command, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<KnowledgeVersionDto>> GetVersionHistoryAsync(Guid articleId, CancellationToken cancellationToken = default);
    Task<KnowledgeVersionDto> GetVersionByNumberAsync(Guid articleId, int versionNumber, CancellationToken cancellationToken = default);
    Task<KnowledgeVersionDto> GetVersionByIdAsync(Guid versionId, CancellationToken cancellationToken = default);

    Task<AuditLogDto> RecordAuditLogAsync(RecordAuditLogCommand command, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuditLogDto>> GetAuditLogsByEntityAsync(string entityName, Guid entityId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuditLogDto>> GetAuditLogsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuditLogDto>> GetAllAuditLogsAsync(CancellationToken cancellationToken = default);
}
