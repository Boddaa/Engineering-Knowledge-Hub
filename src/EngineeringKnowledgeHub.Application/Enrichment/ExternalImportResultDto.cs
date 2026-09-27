namespace EngineeringKnowledgeHub.Application.Enrichment;

using EngineeringKnowledgeHub.Domain.Enums;

/// <summary>
/// DTO representing the result and provenance of an external knowledge source import operation.
/// </summary>
public sealed record ExternalImportResultDto(
    bool IsSuccess,
    Guid SourceId,
    string SourceName,
    SourceType SourceType,
    string Url,
    Guid? DocumentId,
    string? FileName,
    string? ContentType,
    string? FileHash,
    int? ChunkCount,
    bool IsDuplicate,
    string? ErrorCategory,
    string? ErrorMessage,
    DateTimeOffset Timestamp);
