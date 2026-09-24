namespace EngineeringKnowledgeHub.Application.Documents;

using EngineeringKnowledgeHub.Application.Common.Exceptions;
using EngineeringKnowledgeHub.Application.Common.Interfaces;
using EngineeringKnowledgeHub.Application.DTOs;
using EngineeringKnowledgeHub.Domain.Entities;

public sealed class DocumentService(
    IDocumentRepository documentRepository,
    IKnowledgeSourceRepository sourceRepository,
    IFileHasher fileHasher,
    IDocumentTextExtractor textExtractor,
    IDateTimeProvider dateTimeProvider) : IDocumentService
{
    public async Task<DocumentDto> UploadDocumentAsync(
        UploadDocumentCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.FileName))
        {
            throw new ValidationException("Document file name is required.");
        }

        if (string.IsNullOrWhiteSpace(command.ContentType))
        {
            throw new ValidationException("Document content type is required.");
        }

        if (command.SourceId == Guid.Empty)
        {
            throw new ValidationException("SourceId is required.");
        }

        if (command.UploadedById == Guid.Empty)
        {
            throw new ValidationException("UploadedById is required.");
        }

        if (command.Content is null || command.Content.Length == 0)
        {
            throw new ValidationException("Document content cannot be empty.");
        }

        var source = await sourceRepository.GetByIdAsync(command.SourceId, cancellationToken)
            ?? throw new NotFoundException("KnowledgeSource", command.SourceId);

        var fileHash = fileHasher.ComputeHash(command.Content);

        if (await documentRepository.ExistsByHashAsync(fileHash, cancellationToken))
        {
            throw new ConflictException($"A document with content hash '{fileHash}' already exists.");
        }

        var docId = Guid.NewGuid();
        var storagePath = string.IsNullOrWhiteSpace(command.StoragePath)
            ? $"documents/{docId}_{command.FileName.Trim()}"
            : command.StoragePath.Trim();

        var now = dateTimeProvider.UtcNow;
        var document = new Document(
            docId,
            command.FileName,
            storagePath,
            command.ContentType,
            fileHash,
            source.Id,
            command.UploadedById,
            now);

        await documentRepository.AddAsync(document, cancellationToken);

        return DocumentDto.FromDomain(document);
    }

    public async Task<DocumentDto> GetDocumentByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            throw new ValidationException("Document Id cannot be empty.");
        }

        var document = await documentRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Document", id);

        return DocumentDto.FromDomain(document);
    }

    public async Task<IReadOnlyList<DocumentDto>> GetDocumentsBySourceIdAsync(
        Guid sourceId,
        CancellationToken cancellationToken = default)
    {
        if (sourceId == Guid.Empty)
        {
            throw new ValidationException("SourceId is required.");
        }

        var source = await sourceRepository.GetByIdAsync(sourceId, cancellationToken)
            ?? throw new NotFoundException("KnowledgeSource", sourceId);

        var docs = await documentRepository.GetBySourceIdAsync(sourceId, cancellationToken);
        return docs.Select(DocumentDto.FromDomain).ToList();
    }

    public async Task<DocumentDto> ProcessDocumentAsync(
        ProcessDocumentCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.DocumentId == Guid.Empty)
        {
            throw new ValidationException("DocumentId is required.");
        }

        var document = await documentRepository.GetByIdAsync(command.DocumentId, cancellationToken)
            ?? throw new NotFoundException("Document", command.DocumentId);

        var now = dateTimeProvider.UtcNow;
        document.MarkProcessing();

        try
        {
            if (command.Content is not null && command.Content.Length > 0)
            {
                if (textExtractor.CanExtract(document.ContentType, document.FileName))
                {
                    using var stream = new MemoryStream(command.Content);
                    var extractedText = await textExtractor.ExtractTextAsync(stream, document.ContentType, cancellationToken);
                    document.MarkProcessed(extractedText, now);
                }
                else
                {
                    document.MarkProcessed(string.Empty, now);
                }
            }
            else
            {
                document.MarkFailed("Document content is not available for processing.", now);
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            document.MarkFailed("Document extraction failed due to an unexpected processing error.", now);
        }

        await documentRepository.UpdateAsync(document, cancellationToken);

        return DocumentDto.FromDomain(document);
    }
}
