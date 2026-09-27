namespace EngineeringKnowledgeHub.Application.Enrichment;

/// <summary>
/// Provider-independent result model representing the outcome of an external content retrieval attempt.
/// Preserves provenance, status codes, and deterministic error categories.
/// </summary>
public sealed record ExternalContentResult(
    bool IsSuccess,
    string Url,
    byte[]? Content,
    string? ContentType,
    string? ErrorMessage,
    string? ErrorCategory,
    int? StatusCode,
    DateTimeOffset RetrievedAt)
{
    public static ExternalContentResult Success(
        string url,
        byte[] content,
        string contentType,
        int statusCode,
        DateTimeOffset retrievedAt) =>
        new(
            IsSuccess: true,
            Url: url,
            Content: content,
            ContentType: contentType,
            ErrorMessage: null,
            ErrorCategory: null,
            StatusCode: statusCode,
            RetrievedAt: retrievedAt);

    public static ExternalContentResult Failure(
        string url,
        string errorMessage,
        string errorCategory,
        int? statusCode,
        DateTimeOffset retrievedAt) =>
        new(
            IsSuccess: false,
            Url: url,
            Content: null,
            ContentType: null,
            ErrorMessage: errorMessage,
            ErrorCategory: errorCategory,
            StatusCode: statusCode,
            RetrievedAt: retrievedAt);
}
