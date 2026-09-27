namespace EngineeringKnowledgeHub.Application.SemanticSearch;

/// <summary>
/// Nearest-neighbor candidate returned from vector store similarity retrieval.
/// </summary>
public sealed record VectorSearchResult(
    VectorRecord Record,
    double SimilarityScore);
