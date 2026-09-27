namespace EngineeringKnowledgeHub.Application.Rag;

/// <summary>
/// Options configuring RAG context formatting and token budget constraints.
/// </summary>
public sealed class RagContextOptions
{
    public int MaxTokens { get; set; } = 2000;

    public bool IncludeVerificationStatus { get; set; } = true;

    public double InsufficientEvidenceThreshold { get; set; } = 0.2;
}
