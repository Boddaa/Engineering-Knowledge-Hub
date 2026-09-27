namespace EngineeringKnowledgeHub.Application.DTOs;

using EngineeringKnowledgeHub.Domain.Enums;

/// <summary>
/// Result card projection for a knowledge article matching search criteria.
/// Contains excerpt and metadata relevant to search inspection.
/// </summary>
public sealed record SearchArticleResultDto(
    Guid Id,
    string Title,
    string Slug,
    string Summary,
    string Excerpt,
    Guid KnowledgeTypeId,
    KnowledgeStatus Status,
    VerificationStatus VerificationStatus,
    IReadOnlyCollection<Guid> ProductIds,
    IReadOnlyCollection<Guid> TechnologyIds,
    IReadOnlyCollection<Guid> TagIds,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    DateTimeOffset? PublishedAt,
    double? KeywordScore = null,
    double? SemanticScore = null,
    double? HybridScore = null);
