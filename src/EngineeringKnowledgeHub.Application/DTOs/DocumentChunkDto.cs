namespace EngineeringKnowledgeHub.Application.DTOs;

using EngineeringKnowledgeHub.Domain.Entities;

public sealed record DocumentChunkDto(
    Guid Id,
    Guid DocumentId,
    int ChunkIndex,
    string Content,
    int TokenCount,
    string? EmbeddingReference,
    DateTimeOffset CreatedAt)
{
    public static DocumentChunkDto FromDomain(DocumentChunk chunk)
    {
        ArgumentNullException.ThrowIfNull(chunk);
        return new DocumentChunkDto(
            chunk.Id,
            chunk.DocumentId,
            chunk.ChunkIndex,
            chunk.Content,
            chunk.TokenCount,
            chunk.EmbeddingReference,
            chunk.CreatedAt);
    }
}
