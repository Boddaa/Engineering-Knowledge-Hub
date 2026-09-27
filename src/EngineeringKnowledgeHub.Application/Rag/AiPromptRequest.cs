namespace EngineeringKnowledgeHub.Application.Rag;

/// <summary>
/// Encapsulates the prompt parameters provided to an AI generation provider.
/// Provider-independent model separating system instructions from user/context content.
/// </summary>
public sealed record AiPromptRequest(
    string SystemPrompt,
    string UserPrompt,
    int? MaxTokens = null,
    double? Temperature = null);
