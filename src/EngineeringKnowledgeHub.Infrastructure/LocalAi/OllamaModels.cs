namespace EngineeringKnowledgeHub.Infrastructure.LocalAi;

using System.Text.Json.Serialization;

internal sealed record OllamaChatRequest(
    [property: JsonPropertyName("model")] string Model,
    [property: JsonPropertyName("messages")] IReadOnlyList<OllamaChatMessage> Messages,
    [property: JsonPropertyName("stream")] bool Stream = false,
    [property: JsonPropertyName("options")] OllamaOptions? Options = null);

internal sealed record OllamaChatMessage(
    [property: JsonPropertyName("role")] string Role,
    [property: JsonPropertyName("content")] string Content);

internal sealed record OllamaOptions(
    [property: JsonPropertyName("temperature")] double? Temperature = null,
    [property: JsonPropertyName("num_predict")] int? NumPredict = null);

internal sealed record OllamaChatResponse(
    [property: JsonPropertyName("model")] string? Model,
    [property: JsonPropertyName("message")] OllamaChatMessage? Message,
    [property: JsonPropertyName("done")] bool Done,
    [property: JsonPropertyName("prompt_eval_count")] int? PromptEvalCount,
    [property: JsonPropertyName("eval_count")] int? EvalCount);

internal sealed record OllamaEmbedRequest(
    [property: JsonPropertyName("model")] string Model,
    [property: JsonPropertyName("input")] object Input);

internal sealed record OllamaEmbedResponse(
    [property: JsonPropertyName("model")] string? Model,
    [property: JsonPropertyName("embeddings")] IReadOnlyList<IReadOnlyList<float>>? Embeddings);

internal sealed record OllamaLegacyEmbedRequest(
    [property: JsonPropertyName("model")] string Model,
    [property: JsonPropertyName("prompt")] string Prompt);

internal sealed record OllamaLegacyEmbedResponse(
    [property: JsonPropertyName("embedding")] IReadOnlyList<float>? Embedding);

internal sealed record OllamaTagsResponse(
    [property: JsonPropertyName("models")] IReadOnlyList<OllamaTagModel>? Models);

internal sealed record OllamaTagModel(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("model")] string? Model);
