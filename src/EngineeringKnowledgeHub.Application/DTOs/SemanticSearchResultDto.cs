namespace EngineeringKnowledgeHub.Application.DTOs;

/// <summary>
/// Result projection for semantic vector search candidates.
/// Exposes relevance score, source identity, and excerpt without exposing provider-specific embedding vectors.
/// </summary>
public sealed record SemanticSearchResultDto(
    string Id,
    Guid? ArticleId,
    Guid? DocumentId,
    Guid? ChunkId,
    string Text,
    double SimilarityScore,
    IReadOnlyDictionary<string, string> Metadata);
