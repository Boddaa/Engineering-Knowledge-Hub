namespace EngineeringKnowledgeHub.Application.Rag;

using EngineeringKnowledgeHub.Domain.Enums;

/// <summary>
/// Domain-level evidence candidate model extracted during knowledge retrieval.
/// Preserves traceability to the origin article, document chunk, or incident.
/// </summary>
public sealed record RagEvidenceItem(
    int CitationNumber,
    string SourceType,
    Guid? ReferenceId,
    Guid? SecondaryId,
    string Title,
    string Content,
    double RelevanceScore,
    VerificationStatus? VerificationStatus,
    IReadOnlyDictionary<string, string> Metadata);
