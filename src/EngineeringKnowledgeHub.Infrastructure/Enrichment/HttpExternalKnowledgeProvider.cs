namespace EngineeringKnowledgeHub.Infrastructure.Enrichment;

using EngineeringKnowledgeHub.Application.Enrichment;

/// <summary>
/// Infrastructure HTTP implementation of IExternalKnowledgeProvider for approved external sources.
/// Enforces DI-managed HttpClient, HTTP/HTTPS only, Content-Type validation, hard configuration bounds,
/// bounded response sizes, timeout controls, and deterministic error handling.
/// </summary>
public sealed class HttpExternalKnowledgeProvider : IExternalKnowledgeProvider
{
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "text/plain",
        "text/html",
        "text/markdown",
        "text/x-markdown",
        "application/json",
        "application/xml",
        "text/xml",
        "text/csv"
    };

    private readonly HttpClient _httpClient;

    public HttpExternalKnowledgeProvider(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<ExternalContentResult> FetchContentAsync(
        ExternalContentRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var now = DateTimeOffset.UtcNow;

        if (string.IsNullOrWhiteSpace(request.Url))
        {
            return ExternalContentResult.Failure(
                url: request.Url ?? string.Empty,
                errorMessage: "URL cannot be null or whitespace.",
                errorCategory: "InvalidUrl",
                statusCode: null,
                retrievedAt: now);
        }

        if (!Uri.TryCreate(request.Url.Trim(), UriKind.Absolute, out var uri))
        {
            return ExternalContentResult.Failure(
                url: request.Url,
                errorMessage: "The provided URL is not a valid absolute URI.",
                errorCategory: "InvalidUrl",
                statusCode: null,
                retrievedAt: now);
        }

        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
        {
            return ExternalContentResult.Failure(
                url: request.Url,
                errorMessage: $"Unsupported URI scheme '{uri.Scheme}'. Only HTTP and HTTPS are permitted.",
                errorCategory: "InvalidScheme",
                statusCode: null,
                retrievedAt: now);
        }

        // Hard configuration safety boundary: callers cannot increase configured maximums
        if (request.Timeout.HasValue && request.Timeout.Value <= TimeSpan.Zero)
        {
            return ExternalContentResult.Failure(
                url: request.Url,
                errorMessage: "Timeout must be greater than zero.",
                errorCategory: "InvalidConfiguration",
                statusCode: null,
                retrievedAt: now);
        }

        if (request.MaxContentSizeBytes.HasValue && request.MaxContentSizeBytes.Value <= 0)
        {
            return ExternalContentResult.Failure(
                url: request.Url,
                errorMessage: "MaxContentSizeBytes must be greater than zero.",
                errorCategory: "InvalidConfiguration",
                statusCode: null,
                retrievedAt: now);
        }

        var maxAllowedTimeout = TimeSpan.FromSeconds(ExternalImportOptions.DefaultTimeoutSeconds);
        var timeout = request.Timeout.HasValue && request.Timeout.Value < maxAllowedTimeout
            ? request.Timeout.Value
            : maxAllowedTimeout;

        var maxAllowedBytes = ExternalImportOptions.DefaultMaxContentSizeBytes;
        var maxBytes = request.MaxContentSizeBytes.HasValue && request.MaxContentSizeBytes.Value < maxAllowedBytes
            ? request.MaxContentSizeBytes.Value
            : maxAllowedBytes;

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(timeout);

        try
        {
            using var httpRequest = new HttpRequestMessage(HttpMethod.Get, uri);
            httpRequest.Headers.TryAddWithoutValidation("User-Agent", "EngineeringKnowledgeHub/1.0 (+http://localhost)");

            using var response = await _httpClient.SendAsync(
                httpRequest,
                HttpCompletionOption.ResponseHeadersRead,
                cts.Token);

            var statusCode = (int)response.StatusCode;

            if (!response.IsSuccessStatusCode)
            {
                return ExternalContentResult.Failure(
                    url: request.Url,
                    errorMessage: $"Remote server returned HTTP status code {statusCode} ({response.ReasonPhrase}).",
                    errorCategory: "HttpError",
                    statusCode: statusCode,
                    retrievedAt: DateTimeOffset.UtcNow);
            }

            // Enforce Content-Type validation BEFORE reading or buffering body payload
            var rawContentType = response.Content.Headers.ContentType?.MediaType?.Trim();
            if (string.IsNullOrWhiteSpace(rawContentType))
            {
                return ExternalContentResult.Failure(
                    url: request.Url,
                    errorMessage: "Remote server did not specify a Content-Type header in the response.",
                    errorCategory: "UnsupportedContentType",
                    statusCode: statusCode,
                    retrievedAt: DateTimeOffset.UtcNow);
            }

            if (!AllowedContentTypes.Contains(rawContentType))
            {
                return ExternalContentResult.Failure(
                    url: request.Url,
                    errorMessage: $"Remote Content-Type '{rawContentType}' is unsupported. Only safe text-oriented formats (plain text, markdown, HTML, JSON, XML, CSV) are permitted.",
                    errorCategory: "UnsupportedContentType",
                    statusCode: statusCode,
                    retrievedAt: DateTimeOffset.UtcNow);
            }

            // Check Content-Length header if reported
            if (response.Content.Headers.ContentLength.HasValue &&
                response.Content.Headers.ContentLength.Value > maxBytes)
            {
                return ExternalContentResult.Failure(
                    url: request.Url,
                    errorMessage: $"Remote content length ({response.Content.Headers.ContentLength.Value} bytes) exceeds maximum configured limit ({maxBytes} bytes).",
                    errorCategory: "ExceededSizeLimit",
                    statusCode: statusCode,
                    retrievedAt: DateTimeOffset.UtcNow);
            }

            // Enforce size limit while reading the response stream
            await using var stream = await response.Content.ReadAsStreamAsync(cts.Token);
            using var ms = new MemoryStream();
            var buffer = new byte[81920]; // 80 KB chunk buffer
            long totalRead = 0;
            int read;

            while ((read = await stream.ReadAsync(buffer, 0, buffer.Length, cts.Token)) > 0)
            {
                totalRead += read;
                if (totalRead > maxBytes)
                {
                    return ExternalContentResult.Failure(
                        url: request.Url,
                        errorMessage: $"Remote response stream exceeded maximum configured limit ({maxBytes} bytes).",
                        errorCategory: "ExceededSizeLimit",
                        statusCode: statusCode,
                        retrievedAt: DateTimeOffset.UtcNow);
                }

                ms.Write(buffer, 0, read);
            }

            return ExternalContentResult.Success(
                url: request.Url,
                content: ms.ToArray(),
                contentType: rawContentType,
                statusCode: statusCode,
                retrievedAt: DateTimeOffset.UtcNow);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return ExternalContentResult.Failure(
                url: request.Url,
                errorMessage: "Remote content retrieval was cancelled by the caller.",
                errorCategory: "Cancelled",
                statusCode: null,
                retrievedAt: DateTimeOffset.UtcNow);
        }
        catch (OperationCanceledException)
        {
            return ExternalContentResult.Failure(
                url: request.Url,
                errorMessage: $"Remote content retrieval timed out after {timeout.TotalSeconds} seconds.",
                errorCategory: "Timeout",
                statusCode: null,
                retrievedAt: DateTimeOffset.UtcNow);
        }
        catch (HttpRequestException ex)
        {
            return ExternalContentResult.Failure(
                url: request.Url,
                errorMessage: $"Network or HTTP request failed: {ex.Message}",
                errorCategory: "NetworkError",
                statusCode: (int?)ex.StatusCode,
                retrievedAt: DateTimeOffset.UtcNow);
        }
        catch (Exception ex)
        {
            return ExternalContentResult.Failure(
                url: request.Url,
                errorMessage: $"Unexpected error during remote content retrieval: {ex.Message}",
                errorCategory: "UnexpectedError",
                statusCode: null,
                retrievedAt: DateTimeOffset.UtcNow);
        }
    }
}
