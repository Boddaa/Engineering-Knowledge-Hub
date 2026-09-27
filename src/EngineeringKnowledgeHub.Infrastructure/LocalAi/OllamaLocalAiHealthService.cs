namespace EngineeringKnowledgeHub.Infrastructure.LocalAi;

using System.Net.Http.Json;
using System.Text.Json;
using EngineeringKnowledgeHub.Application.Ai;
using Microsoft.Extensions.Logging;

/// <summary>
/// Health check and diagnostic service for the local AI subsystem.
/// Determines whether the local runtime (Ollama) is reachable and verifies that the configured models are loaded.
/// </summary>
public sealed class OllamaLocalAiHealthService(
    HttpClient httpClient,
    LocalAiOptions options,
    ILogger<OllamaLocalAiHealthService>? logger = null) : ILocalAiHealthService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<LocalAiHealthDto> CheckHealthAsync(CancellationToken cancellationToken = default)
    {
        if (!options.Enabled)
        {
            return new LocalAiHealthDto(
                Enabled: false,
                RuntimeReachable: false,
                GenerationModelConfigured: options.GenerationModel,
                GenerationModelAvailable: false,
                EmbeddingModelConfigured: options.EmbeddingModel,
                EmbeddingModelAvailable: false,
                Status: LocalAiHealthStatus.Disabled,
                DiagnosticMessage: "Local AI is currently disabled in configuration.");
        }

        // Validate configuration settings
        try
        {
            options.Validate();
        }
        catch (Exception ex)
        {
            logger?.LogWarning(ex, "Local AI configuration is invalid: {Message}", ex.Message);
            return new LocalAiHealthDto(
                Enabled: true,
                RuntimeReachable: false,
                GenerationModelConfigured: options.GenerationModel,
                GenerationModelAvailable: false,
                EmbeddingModelConfigured: options.EmbeddingModel,
                EmbeddingModelAvailable: false,
                Status: LocalAiHealthStatus.Misconfigured,
                DiagnosticMessage: $"Local AI configuration error: {ex.Message}");
        }

        // Query Ollama /api/tags to list available models
        HttpResponseMessage response;
        try
        {
            response = await httpClient.GetAsync("/api/tags", cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger?.LogWarning(ex, "Failed to reach Local AI runtime at {BaseUrl}", options.BaseUrl);
            return new LocalAiHealthDto(
                Enabled: true,
                RuntimeReachable: false,
                GenerationModelConfigured: options.GenerationModel,
                GenerationModelAvailable: false,
                EmbeddingModelConfigured: options.EmbeddingModel,
                EmbeddingModelAvailable: false,
                Status: LocalAiHealthStatus.Unavailable,
                DiagnosticMessage: $"Local AI runtime is unreachable at '{options.BaseUrl}'. Verify Ollama is running.");
        }

        if (!response.IsSuccessStatusCode)
        {
            logger?.LogWarning("Local AI /api/tags endpoint returned HTTP {StatusCode}", response.StatusCode);
            return new LocalAiHealthDto(
                Enabled: true,
                RuntimeReachable: false,
                GenerationModelConfigured: options.GenerationModel,
                GenerationModelAvailable: false,
                EmbeddingModelConfigured: options.EmbeddingModel,
                EmbeddingModelAvailable: false,
                Status: LocalAiHealthStatus.Unavailable,
                DiagnosticMessage: $"Local AI runtime returned HTTP {(int)response.StatusCode} ({response.ReasonPhrase}).");
        }

        OllamaTagsResponse? tagsResponse = null;
        try
        {
            tagsResponse = await response.Content.ReadFromJsonAsync<OllamaTagsResponse>(JsonOptions, cancellationToken);
        }
        catch (Exception ex)
        {
            logger?.LogWarning(ex, "Failed to deserialize Ollama tags response.");
        }

        var availableModels = tagsResponse?.Models?
            .Select(m => m.Name)
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .ToList() ?? [];

        bool isGenAvailable = IsModelAvailable(options.GenerationModel!, availableModels);
        bool isEmbedAvailable = IsModelAvailable(options.EmbeddingModel!, availableModels);

        if (!isGenAvailable && !isEmbedAvailable)
        {
            return new LocalAiHealthDto(
                Enabled: true,
                RuntimeReachable: true,
                GenerationModelConfigured: options.GenerationModel,
                GenerationModelAvailable: false,
                EmbeddingModelConfigured: options.EmbeddingModel,
                EmbeddingModelAvailable: false,
                Status: LocalAiHealthStatus.ModelUnavailable,
                DiagnosticMessage: $"Runtime is reachable, but neither generation model '{options.GenerationModel}' nor embedding model '{options.EmbeddingModel}' is available.");
        }

        if (!isGenAvailable)
        {
            return new LocalAiHealthDto(
                Enabled: true,
                RuntimeReachable: true,
                GenerationModelConfigured: options.GenerationModel,
                GenerationModelAvailable: false,
                EmbeddingModelConfigured: options.EmbeddingModel,
                EmbeddingModelAvailable: isEmbedAvailable,
                Status: LocalAiHealthStatus.ModelUnavailable,
                DiagnosticMessage: $"Runtime is reachable, but generation model '{options.GenerationModel}' is unavailable.");
        }

        if (!isEmbedAvailable)
        {
            return new LocalAiHealthDto(
                Enabled: true,
                RuntimeReachable: true,
                GenerationModelConfigured: options.GenerationModel,
                GenerationModelAvailable: isGenAvailable,
                EmbeddingModelConfigured: options.EmbeddingModel,
                EmbeddingModelAvailable: false,
                Status: LocalAiHealthStatus.ModelUnavailable,
                DiagnosticMessage: $"Runtime is reachable, but embedding model '{options.EmbeddingModel}' is unavailable.");
        }

        return new LocalAiHealthDto(
            Enabled: true,
            RuntimeReachable: true,
            GenerationModelConfigured: options.GenerationModel,
            GenerationModelAvailable: true,
            EmbeddingModelConfigured: options.EmbeddingModel,
            EmbeddingModelAvailable: true,
            Status: LocalAiHealthStatus.Healthy,
            DiagnosticMessage: "Local AI runtime is reachable and all configured models are available.");
    }

    private static bool IsModelAvailable(string requestedModel, List<string> availableModels)
    {
        var normalizedRequested = requestedModel.Trim();
        foreach (var available in availableModels)
        {
            var normalizedAvailable = available.Trim();
            if (string.Equals(normalizedAvailable, normalizedRequested, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // e.g. "llama3" matches "llama3:latest"
            if (string.Equals(normalizedAvailable, $"{normalizedRequested}:latest", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // e.g. "llama3:latest" matches "llama3"
            if (normalizedRequested.EndsWith(":latest", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(normalizedAvailable, normalizedRequested[..^7], StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
