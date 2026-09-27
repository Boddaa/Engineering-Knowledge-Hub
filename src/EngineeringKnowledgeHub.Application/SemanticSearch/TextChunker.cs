namespace EngineeringKnowledgeHub.Application.SemanticSearch;

using EngineeringKnowledgeHub.Application.Common.Exceptions;
using EngineeringKnowledgeHub.Domain.Entities;

/// <summary>
/// Deterministic sliding-window text chunker with configurable size and overlap.
/// Preserves source identity, sequential chunk ordering, and computes token approximations.
/// </summary>
public sealed class TextChunker : ITextChunker
{
    public IReadOnlyList<TextChunk> ChunkText(string text, ChunkingOptions? options = null)
    {
        var opts = options ?? ChunkingOptions.Default;
        opts.Validate();

        if (string.IsNullOrWhiteSpace(text))
        {
            return Array.Empty<TextChunk>();
        }

        var trimmedText = text.Trim();
        if (trimmedText.Length <= opts.ChunkSize)
        {
            return [new TextChunk(0, trimmedText, 0, trimmedText.Length, EstimateTokenCount(trimmedText))];
        }

        var chunks = new List<TextChunk>();
        int step = opts.ChunkSize - opts.ChunkOverlap;
        int offset = 0;
        int chunkIndex = 0;

        while (offset < trimmedText.Length)
        {
            int length = Math.Min(opts.ChunkSize, trimmedText.Length - offset);
            var slice = trimmedText.Substring(offset, length);

            if (!string.IsNullOrWhiteSpace(slice))
            {
                chunks.Add(new TextChunk(
                    chunkIndex++,
                    slice,
                    offset,
                    length,
                    EstimateTokenCount(slice)));
            }

            if (offset + length >= trimmedText.Length)
            {
                break;
            }

            offset += step;
        }

        return chunks;
    }

    public IReadOnlyList<DocumentChunk> ChunkDocument(
        Guid documentId,
        string text,
        ChunkingOptions? options = null,
        DateTimeOffset? createdAt = null)
    {
        if (documentId == Guid.Empty)
        {
            throw new ValidationException("DocumentId cannot be empty when chunking a document.");
        }

        var textChunks = ChunkText(text, options);
        var created = createdAt ?? DateTimeOffset.UtcNow;

        var documentChunks = new List<DocumentChunk>(textChunks.Count);
        foreach (var chunk in textChunks)
        {
            documentChunks.Add(new DocumentChunk(
                Guid.NewGuid(),
                documentId,
                chunk.Index,
                chunk.Content,
                chunk.TokenCount,
                embeddingReference: null,
                created));
        }

        return documentChunks;
    }

    private static int EstimateTokenCount(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return 0;
        }

        // Standard local heuristic: word count approximated via whitespace splitting
        var words = text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        return Math.Max(1, words.Length);
    }
}
