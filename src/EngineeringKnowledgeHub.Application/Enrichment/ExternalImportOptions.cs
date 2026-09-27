namespace EngineeringKnowledgeHub.Application.Enrichment;

/// <summary>
/// Configuration options for external enrichment and content retrieval limits.
/// </summary>
public sealed class ExternalImportOptions
{
    public const long DefaultMaxContentSizeBytes = 10 * 1024 * 1024; // 10 MB limit
    public const int DefaultTimeoutSeconds = 30;

    private long _maxContentSizeBytes = DefaultMaxContentSizeBytes;
    private int _timeoutSeconds = DefaultTimeoutSeconds;

    public long MaxContentSizeBytes
    {
        get => _maxContentSizeBytes;
        set
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "MaxContentSizeBytes must be greater than zero.");
            }
            _maxContentSizeBytes = value;
        }
    }

    public int TimeoutSeconds
    {
        get => _timeoutSeconds;
        set
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "TimeoutSeconds must be greater than zero.");
            }
            _timeoutSeconds = value;
        }
    }

    public bool ChunkOnImport { get; set; } = true;
}
