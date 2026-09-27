namespace EngineeringKnowledgeHub.Application.DTOs;

using EngineeringKnowledgeHub.Domain.Enums;

/// <summary>
/// Citation evidence item returned with a RAG response.
/// Provides provenance, verification status, and relevant excerpt for an answer assertion.
/// </summary>
public sealed record RagEvidenceDto(
    int CitationNumber,
    string SourceType,
    Guid? ReferenceId,
    Guid? SecondaryId,
    string Title,
    string Excerpt,
    double RelevanceScore,
    VerificationStatus? VerificationStatus,
    IReadOnlyDictionary<string, string> Metadata);
