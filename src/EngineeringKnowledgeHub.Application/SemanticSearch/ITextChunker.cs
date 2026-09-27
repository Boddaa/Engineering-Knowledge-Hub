namespace EngineeringKnowledgeHub.Application.SemanticSearch;

using EngineeringKnowledgeHub.Domain.Entities;

/// <summary>
/// Contract for splitting source documents and arbitrary text into deterministic, ordered chunks.
/// </summary>
public interface ITextChunker
{
    IReadOnlyList<TextChunk> ChunkText(string text, ChunkingOptions? options = null);
    IReadOnlyList<DocumentChunk> ChunkDocument(Guid documentId, string text, ChunkingOptions? options = null, DateTimeOffset? createdAt = null);
}
