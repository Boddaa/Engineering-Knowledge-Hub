namespace EngineeringKnowledgeHub.Application.Enrichment;

/// <summary>
/// Provider-independent request model for retrieving external content from a remote URI.
/// </summary>
public sealed record ExternalContentRequest(
    string Url,
    TimeSpan? Timeout = null,
    long? MaxContentSizeBytes = null);
