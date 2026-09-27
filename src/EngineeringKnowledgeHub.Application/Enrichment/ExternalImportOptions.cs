namespace EngineeringKnowledgeHub.Application.Enrichment;

/// <summary>
/// Configuration options for external enrichment and content retrieval limits.
/// </summary>
public sealed class ExternalImportOptions
{
    public const long DefaultMaxContentSizeBytes = 10 * 1024 * 1024; // 10 MB limit
    public const int DefaultTimeoutSeconds = 30;

    public long MaxContentSizeBytes { get; set; } = DefaultMaxContentSizeBytes;
    public int TimeoutSeconds { get; set; } = DefaultTimeoutSeconds;
    public bool ChunkOnImport { get; set; } = true;
}
