namespace EngineeringKnowledgeHub.Application.Rag;

/// <summary>
/// Result of deterministic citation grounding validation.
/// </summary>
public sealed record GroundingValidationResult(
    bool IsGrounded,
    string? FailureReason,
    IReadOnlySet<int> CitedNumbers);
