namespace EngineeringKnowledgeHub.Application.Sources;

using EngineeringKnowledgeHub.Domain.Enums;

public sealed record CreateKnowledgeSourceCommand(
    string Name,
    SourceType SourceType,
    string? Url = null,
    string? Description = null,
    TrustLevel TrustLevel = TrustLevel.Untrusted,
    bool IsApproved = false);

public sealed record UpdateKnowledgeSourceCommand(
    Guid Id,
    string Name,
    string? Url = null,
    string? Description = null);

public sealed record UpdateTrustLevelCommand(
    Guid Id,
    TrustLevel TrustLevel);

public sealed record AttachArticleSourceCommand(
    Guid ArticleId,
    Guid SourceId,
    string? ReferenceText = null);

public sealed record DetachArticleSourceCommand(
    Guid ArticleId,
    Guid SourceId);
