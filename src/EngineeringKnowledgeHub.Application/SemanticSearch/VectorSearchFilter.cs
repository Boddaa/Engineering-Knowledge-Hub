namespace EngineeringKnowledgeHub.Application.SemanticSearch;

using EngineeringKnowledgeHub.Domain.Enums;

/// <summary>
/// Structured filter dimensions for vector similarity candidate filtering.
/// Aligned with knowledge domain taxonomy, product, technology, and trust state filters.
/// </summary>
public sealed record VectorSearchFilter(
    Guid? ArticleId = null,
    Guid? DocumentId = null,
    Guid? ProductId = null,
    Guid? TechnologyId = null,
    Guid? KnowledgeTypeId = null,
    Guid? TagId = null,
    KnowledgeStatus? Status = null,
    VerificationStatus? VerificationStatus = null,
    IReadOnlyDictionary<string, string>? CustomMetadata = null);
