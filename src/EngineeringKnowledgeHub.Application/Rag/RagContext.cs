namespace EngineeringKnowledgeHub.Application.Rag;

/// <summary>
/// Structured context assembled by IContextBuilder for ingestion by IAiProvider.
/// Encapsulates system instructions, user/evidence prompt, selected citations, and sufficiency evaluation.
/// </summary>
public sealed record RagContext(
    string SystemPrompt,
    string UserPrompt,
    IReadOnlyList<RagEvidenceItem> EvidenceItems,
    bool HasSufficientEvidence,
    int EstimatedTokens);
