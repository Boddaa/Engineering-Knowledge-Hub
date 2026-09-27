namespace EngineeringKnowledgeHub.Application.SemanticSearch;

/// <summary>
/// Provider-independent vector storage contract for dense vector indexing and similarity retrieval.
/// Implementations may range from in-memory test stores to dedicated local or cloud vector engines.
/// </summary>
public interface IVectorStore
{
    Task UpsertAsync(VectorRecord record, CancellationToken cancellationToken = default);
    Task UpsertBatchAsync(IReadOnlyList<VectorRecord> records, CancellationToken cancellationToken = default);
    Task<VectorRecord?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
    Task<int> DeleteByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken = default);
    Task<int> DeleteByArticleIdAsync(Guid articleId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<VectorSearchResult>> SearchNearestAsync(
        Vector queryVector,
        int topK,
        VectorSearchFilter? filter = null,
        CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    Task ClearAsync(CancellationToken cancellationToken = default);
}
