namespace EngineeringKnowledgeHub.Application.Rag;

/// <summary>
/// Contract for deterministic citation and evidence grounding validation.
/// Verifies that AI-generated text adheres to citation integrity rules before being returned to callers.
/// </summary>
public interface IGroundingValidator
{
    GroundingValidationResult ValidateGrounding(
        string? content,
        IReadOnlyList<RagEvidenceItem> availableEvidence);
}
