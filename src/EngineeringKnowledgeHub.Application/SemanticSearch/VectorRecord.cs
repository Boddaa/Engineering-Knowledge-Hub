namespace EngineeringKnowledgeHub.Application.SemanticSearch;

using EngineeringKnowledgeHub.Domain.Enums;

/// <summary>
/// Record stored within vector storage representing an embedding vector and its source metadata.
/// </summary>
public sealed record VectorRecord(
    string Id,
    Vector Vector,
    Guid? ArticleId,
    Guid? DocumentId,
    Guid? ChunkId,
    string Text,
    IReadOnlyDictionary<string, string> Metadata)
{
    public static VectorRecord ForDocumentChunk(
        Guid documentId,
        Guid chunkId,
        int chunkIndex,
        string text,
        Vector vector,
        IReadOnlyDictionary<string, string>? additionalMetadata = null)
    {
        var meta = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["SourceType"] = "DocumentChunk",
            ["DocumentId"] = documentId.ToString(),
            ["ChunkId"] = chunkId.ToString(),
            ["ChunkIndex"] = chunkIndex.ToString()
        };

        if (additionalMetadata != null)
        {
            foreach (var kvp in additionalMetadata)
            {
                meta[kvp.Key] = kvp.Value;
            }
        }

        return new VectorRecord(
            $"doc_{documentId}_chunk_{chunkIndex}",
            vector,
            ArticleId: null,
            DocumentId: documentId,
            ChunkId: chunkId,
            Text: text,
            Metadata: meta);
    }

    public static VectorRecord ForArticle(
        Guid articleId,
        string text,
        Vector vector,
        Guid? knowledgeTypeId = null,
        KnowledgeStatus? status = null,
        VerificationStatus? verificationStatus = null,
        IReadOnlyCollection<Guid>? productIds = null,
        IReadOnlyCollection<Guid>? technologyIds = null,
        IReadOnlyCollection<Guid>? tagIds = null,
        IReadOnlyDictionary<string, string>? additionalMetadata = null)
    {
        var meta = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["SourceType"] = "KnowledgeArticle",
            ["ArticleId"] = articleId.ToString()
        };

        if (knowledgeTypeId.HasValue) meta["KnowledgeTypeId"] = knowledgeTypeId.Value.ToString();
        if (status.HasValue) meta["Status"] = status.Value.ToString();
        if (verificationStatus.HasValue) meta["VerificationStatus"] = verificationStatus.Value.ToString();
        if (productIds != null && productIds.Count > 0) meta["ProductIds"] = string.Join(",", productIds);
        if (technologyIds != null && technologyIds.Count > 0) meta["TechnologyIds"] = string.Join(",", technologyIds);
        if (tagIds != null && tagIds.Count > 0) meta["TagIds"] = string.Join(",", tagIds);

        if (additionalMetadata != null)
        {
            foreach (var kvp in additionalMetadata)
            {
                meta[kvp.Key] = kvp.Value;
            }
        }

        return new VectorRecord(
            $"article_{articleId}",
            vector,
            ArticleId: articleId,
            DocumentId: null,
            ChunkId: null,
            Text: text,
            Metadata: meta);
    }
}
