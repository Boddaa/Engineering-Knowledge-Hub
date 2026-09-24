namespace EngineeringKnowledgeHub.Application.DTOs;

using EngineeringKnowledgeHub.Domain.Entities;
using EngineeringKnowledgeHub.Domain.Enums;

public sealed record DocumentDto(
    Guid Id,
    string FileName,
    string StoragePath,
    string ContentType,
    string FileHash,
    Guid SourceId,
    DocumentProcessingStatus ProcessingStatus,
    Guid UploadedById,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ProcessedAt,
    string? FailureReason,
    string? ExtractedText)
{
    public static DocumentDto FromDomain(Document document) =>
        new(
            document.Id,
            document.FileName,
            document.StoragePath,
            document.ContentType,
            document.FileHash,
            document.SourceId,
            document.ProcessingStatus,
            document.UploadedById,
            document.CreatedAt,
            document.ProcessedAt,
            document.FailureReason,
            document.ExtractedText);
}
