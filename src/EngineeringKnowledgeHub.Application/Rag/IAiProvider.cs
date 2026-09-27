namespace EngineeringKnowledgeHub.Application.Rag;

/// <summary>
/// Provider-independent contract for text generation and summarization.
/// Concrete providers (local models, cloud models, deterministic fallbacks) implement this interface.
/// Domain and Application remain decoupled from specific AI vendors or SDKs.
/// </summary>
public interface IAiProvider
{
    Task<AiResponse> GenerateAsync(
        AiPromptRequest request,
        CancellationToken cancellationToken = default);
}
