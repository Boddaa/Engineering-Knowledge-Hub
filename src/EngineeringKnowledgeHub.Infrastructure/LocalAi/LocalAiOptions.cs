namespace EngineeringKnowledgeHub.Infrastructure.LocalAi;

using EngineeringKnowledgeHub.Application.Common.Exceptions;

/// <summary>
/// Strongly-typed configuration options for the local AI subsystem (e.g., Ollama runtime).
/// </summary>
public sealed class LocalAiOptions
{
    public const string SectionName = "LocalAI";
    public const string DefaultBaseUrl = "http://127.0.0.1:11434";
    public const int DefaultTimeoutSeconds = 120;
    public const int DefaultEmbeddingDimensions = 768;

    /// <summary>
    /// Indicates whether local AI providers should be activated instead of deterministic fallbacks.
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// The base HTTP/HTTPS URL for the local AI runtime. Default is loopback: http://127.0.0.1:11434.
    /// Non-loopback addresses change the trust boundary and must be explicitly configured.
    /// </summary>
    public string BaseUrl { get; set; } = DefaultBaseUrl;

    /// <summary>
    /// The name/tag of the local model to use for text generation and RAG answers (e.g., "llama3").
    /// </summary>
    public string? GenerationModel { get; set; }

    /// <summary>
    /// The name/tag of the local model to use for vector embeddings (e.g., "nomic-embed-text").
    /// </summary>
    public string? EmbeddingModel { get; set; }

    /// <summary>
    /// Timeout in seconds for local AI requests. Must be strictly positive.
    /// </summary>
    public int TimeoutSeconds { get; set; } = DefaultTimeoutSeconds;

    /// <summary>
    /// The expected vector dimensionality of the configured embedding model (e.g., 768, 384).
    /// Used for strict dimension safety verification to prevent truncation or padding.
    /// </summary>
    public int EmbeddingDimensions { get; set; } = DefaultEmbeddingDimensions;

    /// <summary>
    /// Validates configuration values when local AI is enabled.
    /// </summary>
    public void Validate()
    {
        if (!Enabled)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(BaseUrl))
        {
            throw new ValidationException("Local AI BaseUrl cannot be empty when Local AI is enabled.");
        }

        if (!Uri.TryCreate(BaseUrl, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new ValidationException($"Local AI BaseUrl '{BaseUrl}' is invalid. Must be an absolute HTTP or HTTPS URL.");
        }

        if (TimeoutSeconds <= 0)
        {
            throw new ValidationException($"Local AI TimeoutSeconds ({TimeoutSeconds}) must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(GenerationModel))
        {
            throw new ValidationException("Local AI GenerationModel must be specified when Local AI is enabled.");
        }

        if (string.IsNullOrWhiteSpace(EmbeddingModel))
        {
            throw new ValidationException("Local AI EmbeddingModel must be specified when Local AI is enabled.");
        }

        if (EmbeddingDimensions <= 0)
        {
            throw new ValidationException($"Local AI EmbeddingDimensions ({EmbeddingDimensions}) must be greater than zero.");
        }
    }
}
