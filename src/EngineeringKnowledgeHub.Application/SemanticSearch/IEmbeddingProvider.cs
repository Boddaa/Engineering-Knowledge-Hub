namespace EngineeringKnowledgeHub.Application.SemanticSearch;

/// <summary>
/// Provider-independent embedding contract for converting textual content into dense vectors.
/// Enables seamless substitution of local, cloud, or test embedding backends.
/// </summary>
public interface IEmbeddingProvider
{
    int Dimensions { get; }
    Task<Vector> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Vector>> GenerateBatchEmbeddingsAsync(IReadOnlyList<string> texts, CancellationToken cancellationToken = default);
}
