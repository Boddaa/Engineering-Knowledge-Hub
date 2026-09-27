namespace EngineeringKnowledgeHub.Application.SemanticSearch;

using EngineeringKnowledgeHub.Application.Common.Exceptions;
using EngineeringKnowledgeHub.Domain.Entities;

/// <summary>
/// Orchestrates embedding generation and vector store ingestion for articles and document chunks.
/// </summary>
public sealed class SemanticIndexService : ISemanticIndexService
{
    private readonly IEmbeddingProvider _embeddingProvider;
    private readonly IVectorStore _vectorStore;

    public SemanticIndexService(
        IEmbeddingProvider embeddingProvider,
        IVectorStore vectorStore)
    {
        _embeddingProvider = embeddingProvider ?? throw new ArgumentNullException(nameof(embeddingProvider));
        _vectorStore = vectorStore ?? throw new ArgumentNullException(nameof(vectorStore));
    }

    public async Task<VectorRecord> IndexArticleAsync(
        KnowledgeArticle article,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(article);

        var text = $"{article.Title}\n{article.Summary}\n{article.Content}".Trim();
        var vector = await _embeddingProvider.GenerateEmbeddingAsync(text, cancellationToken);

        var record = VectorRecord.ForArticle(
            article.Id,
            text,
            vector,
            article.KnowledgeTypeId,
            article.Status,
            article.VerificationStatus,
            article.ProductIds,
            article.TechnologyIds,
            article.TagIds);

        await _vectorStore.UpsertAsync(record, cancellationToken);
        return record;
    }

    public async Task<IReadOnlyList<VectorRecord>> IndexDocumentChunksAsync(
        Guid documentId,
        IReadOnlyList<DocumentChunk> chunks,
        CancellationToken cancellationToken = default)
    {
        if (documentId == Guid.Empty)
        {
            throw new ValidationException("DocumentId cannot be empty when indexing document chunks.");
        }

        ArgumentNullException.ThrowIfNull(chunks);

        if (chunks.Count == 0)
        {
            return Array.Empty<VectorRecord>();
        }

        var records = new List<VectorRecord>(chunks.Count);
        foreach (var chunk in chunks)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var vector = await _embeddingProvider.GenerateEmbeddingAsync(chunk.Content, cancellationToken);
            var record = VectorRecord.ForDocumentChunk(
                documentId,
                chunk.Id,
                chunk.ChunkIndex,
                chunk.Content,
                vector);

            chunk.SetEmbeddingReference(record.Id);
            records.Add(record);
        }

        await _vectorStore.UpsertBatchAsync(records, cancellationToken);
        return records;
    }

    public async Task<bool> RemoveArticleIndexAsync(
        Guid articleId,
        CancellationToken cancellationToken = default)
    {
        if (articleId == Guid.Empty)
        {
            throw new ValidationException("ArticleId cannot be empty.");
        }

        var deletedCount = await _vectorStore.DeleteByArticleIdAsync(articleId, cancellationToken);
        return deletedCount > 0;
    }

    public async Task<int> RemoveDocumentIndexAsync(
        Guid documentId,
        CancellationToken cancellationToken = default)
    {
        if (documentId == Guid.Empty)
        {
            throw new ValidationException("DocumentId cannot be empty.");
        }

        return await _vectorStore.DeleteByDocumentIdAsync(documentId, cancellationToken);
    }
}
