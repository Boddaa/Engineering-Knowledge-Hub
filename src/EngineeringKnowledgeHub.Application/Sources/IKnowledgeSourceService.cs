namespace EngineeringKnowledgeHub.Application.Sources;

using EngineeringKnowledgeHub.Application.DTOs;
using EngineeringKnowledgeHub.Domain.Enums;

public interface IKnowledgeSourceService
{
    Task<KnowledgeSourceDto> CreateSourceAsync(CreateKnowledgeSourceCommand command, CancellationToken cancellationToken = default);
    Task<KnowledgeSourceDto> UpdateSourceAsync(UpdateKnowledgeSourceCommand command, CancellationToken cancellationToken = default);
    Task<KnowledgeSourceDto> GetSourceByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<KnowledgeSourceDto>> GetSourcesAsync(
        SourceType? sourceType = null,
        TrustLevel? trustLevel = null,
        bool? isApproved = null,
        CancellationToken cancellationToken = default);
    Task<KnowledgeSourceDto> ApproveSourceAsync(Guid sourceId, CancellationToken cancellationToken = default);
    Task<KnowledgeSourceDto> RevokeSourceApprovalAsync(Guid sourceId, CancellationToken cancellationToken = default);
    Task<KnowledgeSourceDto> UpdateTrustLevelAsync(UpdateTrustLevelCommand command, CancellationToken cancellationToken = default);
    Task<KnowledgeArticleDto> AttachSourceToArticleAsync(AttachArticleSourceCommand command, CancellationToken cancellationToken = default);
    Task<KnowledgeArticleDto> DetachSourceFromArticleAsync(DetachArticleSourceCommand command, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<KnowledgeSourceDto>> GetSourcesByArticleIdAsync(Guid articleId, CancellationToken cancellationToken = default);
}
