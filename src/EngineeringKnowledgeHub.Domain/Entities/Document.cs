namespace EngineeringKnowledgeHub.Domain.Entities;

using EngineeringKnowledgeHub.Domain.Enums;
using EngineeringKnowledgeHub.Domain.Exceptions;

/// <summary>
/// Imported or uploaded physical file (PDF, Markdown, text, logs, etc.)
/// that serves as supporting evidence or source material for knowledge articles.
/// </summary>
public sealed class Document
{
    public Guid Id { get; private set; }
    public string FileName { get; private set; }
    public string StoragePath { get; private set; }
    public string ContentType { get; private set; }
    public string FileHash { get; private set; }
    public Guid SourceId { get; private set; }
    public DocumentProcessingStatus ProcessingStatus { get; private set; }
    public Guid UploadedById { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? ProcessedAt { get; private set; }
    public string? FailureReason { get; private set; }
    public string? ExtractedText { get; private set; }

    public Document(
        Guid id,
        string fileName,
        string storagePath,
        string contentType,
        string fileHash,
        Guid sourceId,
        Guid uploadedById,
        DateTimeOffset createdAt)
    {
        if (id == Guid.Empty) throw new DomainRuleViolationException("Document Id cannot be empty.");
        if (string.IsNullOrWhiteSpace(fileName)) throw new DomainRuleViolationException("Document file name cannot be null or whitespace.");
        if (string.IsNullOrWhiteSpace(storagePath)) throw new DomainRuleViolationException("Document storage path cannot be null or whitespace.");
        if (string.IsNullOrWhiteSpace(contentType)) throw new DomainRuleViolationException("Document content type cannot be null or whitespace.");
        if (string.IsNullOrWhiteSpace(fileHash)) throw new DomainRuleViolationException("Document file hash cannot be null or whitespace.");
        if (sourceId == Guid.Empty) throw new DomainRuleViolationException("Document SourceId cannot be empty.");
        if (uploadedById == Guid.Empty) throw new DomainRuleViolationException("Document UploadedById cannot be empty.");

        Id = id;
        FileName = fileName.Trim();
        StoragePath = storagePath.Trim();
        ContentType = contentType.Trim();
        FileHash = fileHash.Trim();
        SourceId = sourceId;
        UploadedById = uploadedById;
        CreatedAt = createdAt;
        ProcessingStatus = DocumentProcessingStatus.Pending;
    }

    public void MarkProcessing()
    {
        if (ProcessingStatus is not (DocumentProcessingStatus.Pending or DocumentProcessingStatus.Failed))
        {
            throw new DomainRuleViolationException($"Cannot transition document from '{ProcessingStatus}' to 'Processing'. Must be Pending or Failed.");
        }

        ProcessingStatus = DocumentProcessingStatus.Processing;
    }

    public void MarkProcessed(string? extractedText, DateTimeOffset processedAt)
    {
        if (ProcessingStatus != DocumentProcessingStatus.Processing)
        {
            throw new DomainRuleViolationException($"Cannot transition document from '{ProcessingStatus}' to 'Processed'. Document must be in Processing state.");
        }

        ProcessingStatus = DocumentProcessingStatus.Processed;
        ExtractedText = extractedText?.Trim() ?? string.Empty;
        ProcessedAt = processedAt;
        FailureReason = null;
    }

    public void MarkFailed(string failureReason, DateTimeOffset failedAt)
    {
        if (string.IsNullOrWhiteSpace(failureReason))
        {
            throw new DomainRuleViolationException("Document failure reason cannot be null or whitespace.");
        }

        ProcessingStatus = DocumentProcessingStatus.Failed;
        FailureReason = failureReason.Trim();
        ProcessedAt = failedAt;
    }
}
