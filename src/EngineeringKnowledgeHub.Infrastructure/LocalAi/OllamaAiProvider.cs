namespace EngineeringKnowledgeHub.Infrastructure.LocalAi;

using System.Net.Http.Json;
using System.Text.Json;
using EngineeringKnowledgeHub.Application.Common.Exceptions;
using EngineeringKnowledgeHub.Application.Rag;
using Microsoft.Extensions.Logging;

/// <summary>
/// Concrete local LLM provider connecting to a locally hosted Ollama runtime over HTTP.
/// Operates strictly offline without cloud dependencies, API keys, or internet egress.
/// </summary>
public sealed class OllamaAiProvider(
    HttpClient httpClient,
    LocalAiOptions options,
    ILogger<OllamaAiProvider>? logger = null) : IAiProvider
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<AiResponse> GenerateAsync(
        AiPromptRequest request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (request is null)
        {
            throw new ValidationException("AI prompt request cannot be null.");
        }

        if (string.IsNullOrWhiteSpace(request.UserPrompt))
        {
            throw new ValidationException("User prompt cannot be null or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(options.GenerationModel))
        {
            throw new ValidationException("Local AI GenerationModel is not configured.");
        }

        var messages = new List<OllamaChatMessage>();

        if (!string.IsNullOrWhiteSpace(request.SystemPrompt))
        {
            messages.Add(new OllamaChatMessage("system", request.SystemPrompt.Trim()));
        }

        messages.Add(new OllamaChatMessage("user", request.UserPrompt.Trim()));

        OllamaOptions? modelOptions = null;
        if (request.Temperature.HasValue || request.MaxTokens.HasValue)
        {
            modelOptions = new OllamaOptions(
                Temperature: request.Temperature,
                NumPredict: request.MaxTokens);
        }

        var chatRequest = new OllamaChatRequest(
            Model: options.GenerationModel,
            Messages: messages,
            Stream: false,
            Options: modelOptions);

        HttpResponseMessage response;
        try
        {
            response = await httpClient.PostAsJsonAsync("/api/chat", chatRequest, JsonOptions, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            logger?.LogWarning("Local AI generation timed out after {Timeout} seconds for model {Model}", options.TimeoutSeconds, options.GenerationModel);
            throw new InvalidOperationException($"Local AI generation timed out after {options.TimeoutSeconds} seconds for model '{options.GenerationModel}'.");
        }
        catch (HttpRequestException ex)
        {
            logger?.LogError(ex, "Failed to connect to Local AI runtime at {BaseUrl}", options.BaseUrl);
            throw new InvalidOperationException($"Local AI runtime is unreachable at '{options.BaseUrl}'. Verify Ollama is running.", ex);
        }

        if (!response.IsSuccessStatusCode)
        {
            string errorBody = string.Empty;
            try
            {
                errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            }
            catch
            {
                // Fall back to status code description if body cannot be read
            }

            logger?.LogWarning("Local AI generation failed with HTTP {StatusCode}: {ErrorBody}", response.StatusCode, errorBody);
            throw new InvalidOperationException(
                $"Local AI generation failed with status code {(int)response.StatusCode} ({response.ReasonPhrase}): {errorBody}");
        }

        OllamaChatResponse? chatResponse;
        try
        {
            chatResponse = await response.Content.ReadFromJsonAsync<OllamaChatResponse>(JsonOptions, cancellationToken);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Failed to deserialize Ollama generation response.");
            throw new InvalidOperationException("Local AI runtime returned an invalid or malformed response.", ex);
        }

        var content = chatResponse?.Message?.Content;
        if (content is null)
        {
            throw new InvalidOperationException("Local AI runtime returned an empty response message.");
        }

        var modelName = !string.IsNullOrWhiteSpace(chatResponse?.Model)
            ? chatResponse.Model
            : options.GenerationModel;

        var promptTokens = chatResponse?.PromptEvalCount ?? 0;
        var completionTokens = chatResponse?.EvalCount ?? 0;

        return new AiResponse(
            Content: content,
            ModelName: modelName,
            PromptTokens: promptTokens,
            CompletionTokens: completionTokens);
    }
}
