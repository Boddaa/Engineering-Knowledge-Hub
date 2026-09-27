namespace EngineeringKnowledgeHub.Domain.Entities;

using EngineeringKnowledgeHub.Domain.Exceptions;

/// <summary>
/// A chunk of extracted text from a Document, serving as the unit for embedding and retrieval.
/// Invariant: DocumentChunk is an immutable content unit with source identity and ordering.
/// </summary>
public sealed class DocumentChunk
{
    public Guid Id { get; private set; }
    public Guid DocumentId { get; private set; }
    public int ChunkIndex { get; private set; }
    public string Content { get; private set; }
    public int TokenCount { get; private set; }
    public string? EmbeddingReference { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public DocumentChunk(
        Guid id,
        Guid documentId,
        int chunkIndex,
        string content,
        int tokenCount,
        string? embeddingReference,
        DateTimeOffset createdAt)
    {
        if (id == Guid.Empty)
        {
            throw new DomainRuleViolationException("DocumentChunk Id cannot be empty.");
        }

        if (documentId == Guid.Empty)
        {
            throw new DomainRuleViolationException("DocumentChunk DocumentId cannot be empty.");
        }

        if (chunkIndex < 0)
        {
            throw new DomainRuleViolationException("DocumentChunk ChunkIndex cannot be negative.");
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new DomainRuleViolationException("DocumentChunk Content cannot be null or whitespace.");
        }

        if (tokenCount < 0)
        {
            throw new DomainRuleViolationException("DocumentChunk TokenCount cannot be negative.");
        }

        Id = id;
        DocumentId = documentId;
        ChunkIndex = chunkIndex;
        Content = content.Trim();
        TokenCount = tokenCount;
        EmbeddingReference = string.IsNullOrWhiteSpace(embeddingReference) ? null : embeddingReference.Trim();
        CreatedAt = createdAt;
    }

    public void SetEmbeddingReference(string embeddingReference)
    {
        if (string.IsNullOrWhiteSpace(embeddingReference))
        {
            throw new DomainRuleViolationException("EmbeddingReference cannot be null or whitespace.");
        }

        EmbeddingReference = embeddingReference.Trim();
    }
}
