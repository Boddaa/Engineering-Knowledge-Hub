namespace EngineeringKnowledgeHub.Application.DTOs;

/// <summary>
/// Result of an assisted RAG query.
/// Delivers the generated answer along with traceable citations, evidence sufficiency status, and model metadata.
/// </summary>
public sealed record RagResponseDto(
    string Question,
    string Answer,
    bool IsSufficientEvidence,
    IReadOnlyList<RagEvidenceDto> Evidence,
    string ModelUsed,
    DateTimeOffset AnsweredAt);
