namespace EngineeringKnowledgeHub.Infrastructure.LocalAi;

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using EngineeringKnowledgeHub.Application.Common.Exceptions;
using EngineeringKnowledgeHub.Application.SemanticSearch;
using Microsoft.Extensions.Logging;

/// <summary>
/// Concrete local embedding provider connecting to a locally hosted Ollama runtime over HTTP.
/// Converts text into dense vector representations with strict dimension validation and zero cloud egress.
/// </summary>
public sealed class OllamaEmbeddingProvider(
    HttpClient httpClient,
    LocalAiOptions options,
    ILogger<OllamaEmbeddingProvider>? logger = null) : IEmbeddingProvider
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public int Dimensions => options.EmbeddingDimensions;

    public async Task<Vector> GenerateEmbeddingAsync(
        string text,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ValidationException("Text for embedding cannot be null or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(options.EmbeddingModel))
        {
            throw new ValidationException("Local AI EmbeddingModel is not configured.");
        }

        // Modern Ollama endpoint: /api/embed
        var embedRequest = new OllamaEmbedRequest(
            Model: options.EmbeddingModel,
            Input: text.Trim());

        HttpResponseMessage response;
        try
        {
            response = await httpClient.PostAsJsonAsync("/api/embed", embedRequest, JsonOptions, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            logger?.LogWarning("Local AI embedding timed out after {Timeout} seconds for model {Model}", options.TimeoutSeconds, options.EmbeddingModel);
            throw new InvalidOperationException($"Local AI embedding timed out after {options.TimeoutSeconds} seconds for model '{options.EmbeddingModel}'.");
        }
        catch (HttpRequestException ex)
        {
            logger?.LogError(ex, "Failed to connect to Local AI runtime at {BaseUrl}", options.BaseUrl);
            throw new InvalidOperationException($"Local AI runtime is unreachable at '{options.BaseUrl}'. Verify Ollama is running.", ex);
        }

        // Backward compatibility: If /api/embed is not found (404), fall back to legacy /api/embeddings
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return await GenerateEmbeddingLegacyAsync(text, cancellationToken);
        }

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await ReadErrorBodyAsync(response, cancellationToken);
            logger?.LogWarning("Local AI embedding failed with HTTP {StatusCode}: {ErrorBody}", response.StatusCode, errorBody);
            throw new InvalidOperationException(
                $"Local AI embedding failed with status code {(int)response.StatusCode} ({response.ReasonPhrase}): {errorBody}");
        }

        OllamaEmbedResponse? embedResponse;
        try
        {
            embedResponse = await response.Content.ReadFromJsonAsync<OllamaEmbedResponse>(JsonOptions, cancellationToken);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Failed to deserialize Ollama embedding response.");
            throw new InvalidOperationException("Local AI runtime returned an invalid or malformed embedding response.", ex);
        }

        var floatList = embedResponse?.Embeddings?.FirstOrDefault();
        if (floatList is null || floatList.Count == 0)
        {
            throw new InvalidOperationException("Local AI runtime returned an empty embedding vector.");
        }

        return CreateValidatedVector(floatList);
    }

    public async Task<IReadOnlyList<Vector>> GenerateBatchEmbeddingsAsync(
        IReadOnlyList<string> texts,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (texts is null)
        {
            throw new ValidationException("Texts collection cannot be null.");
        }

        if (texts.Count == 0)
        {
            return Array.Empty<Vector>();
        }

        for (int i = 0; i < texts.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(texts[i]))
            {
                throw new ValidationException($"Text at index {i} cannot be null or whitespace.");
            }
        }

        if (string.IsNullOrWhiteSpace(options.EmbeddingModel))
        {
            throw new ValidationException("Local AI EmbeddingModel is not configured.");
        }

        // Batch /api/embed
        var trimmedTexts = texts.Select(t => t.Trim()).ToArray();
        var embedRequest = new OllamaEmbedRequest(
            Model: options.EmbeddingModel,
            Input: trimmedTexts);

        HttpResponseMessage response;
        try
        {
            response = await httpClient.PostAsJsonAsync("/api/embed", embedRequest, JsonOptions, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            logger?.LogWarning("Local AI batch embedding timed out after {Timeout} seconds for model {Model}", options.TimeoutSeconds, options.EmbeddingModel);
            throw new InvalidOperationException($"Local AI batch embedding timed out after {options.TimeoutSeconds} seconds for model '{options.EmbeddingModel}'.");
        }
        catch (HttpRequestException ex)
        {
            logger?.LogError(ex, "Failed to connect to Local AI runtime at {BaseUrl}", options.BaseUrl);
            throw new InvalidOperationException($"Local AI runtime is unreachable at '{options.BaseUrl}'. Verify Ollama is running.", ex);
        }

        // Backward compatibility fallback to sequential legacy embeddings
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            var fallbackResults = new List<Vector>(texts.Count);
            foreach (var text in texts)
            {
                fallbackResults.Add(await GenerateEmbeddingLegacyAsync(text, cancellationToken));
            }
            return fallbackResults;
        }

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await ReadErrorBodyAsync(response, cancellationToken);
            logger?.LogWarning("Local AI batch embedding failed with HTTP {StatusCode}: {ErrorBody}", response.StatusCode, errorBody);
            throw new InvalidOperationException(
                $"Local AI batch embedding failed with status code {(int)response.StatusCode} ({response.ReasonPhrase}): {errorBody}");
        }

        OllamaEmbedResponse? embedResponse;
        try
        {
            embedResponse = await response.Content.ReadFromJsonAsync<OllamaEmbedResponse>(JsonOptions, cancellationToken);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Failed to deserialize Ollama batch embedding response.");
            throw new InvalidOperationException("Local AI runtime returned an invalid or malformed batch embedding response.", ex);
        }

        var embeddings = embedResponse?.Embeddings;
        if (embeddings is null || embeddings.Count != texts.Count)
        {
            throw new InvalidOperationException(
                $"Local AI batch embedding returned {embeddings?.Count ?? 0} vectors, but {texts.Count} were requested.");
        }

        var results = new List<Vector>(embeddings.Count);
        for (int i = 0; i < embeddings.Count; i++)
        {
            results.Add(CreateValidatedVector(embeddings[i]));
        }

        return results;
    }

    private async Task<Vector> GenerateEmbeddingLegacyAsync(string text, CancellationToken cancellationToken)
    {
        var legacyRequest = new OllamaLegacyEmbedRequest(
            Model: options.EmbeddingModel!,
            Prompt: text.Trim());

        HttpResponseMessage response;
        try
        {
            response = await httpClient.PostAsJsonAsync("/api/embeddings", legacyRequest, JsonOptions, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            throw new InvalidOperationException($"Local AI embedding timed out after {options.TimeoutSeconds} seconds for model '{options.EmbeddingModel}'.");
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException($"Local AI runtime is unreachable at '{options.BaseUrl}'. Verify Ollama is running.", ex);
        }

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await ReadErrorBodyAsync(response, cancellationToken);
            throw new InvalidOperationException(
                $"Local AI legacy embedding failed with status code {(int)response.StatusCode} ({response.ReasonPhrase}): {errorBody}");
        }

        var legacyResponse = await response.Content.ReadFromJsonAsync<OllamaLegacyEmbedResponse>(JsonOptions, cancellationToken);
        if (legacyResponse?.Embedding is null || legacyResponse.Embedding.Count == 0)
        {
            throw new InvalidOperationException("Local AI runtime returned an empty legacy embedding vector.");
        }

        return CreateValidatedVector(legacyResponse.Embedding);
    }

    private Vector CreateValidatedVector(IReadOnlyList<float> values)
    {
        // Enforce strict dimension safety: do not silently resize, truncate, or pad
        if (values.Count != Dimensions)
        {
            throw new ValidationException(
                $"Embedding dimension mismatch: configured {Dimensions} dimensions for model '{options.EmbeddingModel}', but runtime returned {values.Count} dimensions.");
        }

        float[] array = values is float[] arr ? arr : values.ToArray();
        return new Vector(array);
    }

    private static async Task<string> ReadErrorBodyAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            return await response.Content.ReadAsStringAsync(cancellationToken);
        }
        catch
        {
            return string.Empty;
        }
    }
}
