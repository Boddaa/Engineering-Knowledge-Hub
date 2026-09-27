namespace EngineeringKnowledgeHub.Application.SemanticSearch;

using EngineeringKnowledgeHub.Domain.Entities;

/// <summary>
/// Service contract for indexing knowledge articles and document chunks into vector storage.
/// </summary>
public interface ISemanticIndexService
{
    Task<VectorRecord> IndexArticleAsync(KnowledgeArticle article, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<VectorRecord>> IndexDocumentChunksAsync(Guid documentId, IReadOnlyList<DocumentChunk> chunks, CancellationToken cancellationToken = default);
    Task<bool> RemoveArticleIndexAsync(Guid articleId, CancellationToken cancellationToken = default);
    Task<int> RemoveDocumentIndexAsync(Guid documentId, CancellationToken cancellationToken = default);
}
