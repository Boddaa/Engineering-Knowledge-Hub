namespace EngineeringKnowledgeHub.Application.Enrichment;

using EngineeringKnowledgeHub.Application.Common.Exceptions;
using EngineeringKnowledgeHub.Application.Common.Interfaces;
using EngineeringKnowledgeHub.Application.DTOs;
using EngineeringKnowledgeHub.Application.SemanticSearch;
using EngineeringKnowledgeHub.Domain.Entities;
using EngineeringKnowledgeHub.Domain.Enums;
using EngineeringKnowledgeHub.Domain.ValueObjects;

public sealed class ExternalEnrichmentService(
    IKnowledgeSourceRepository sourceRepository,
    IDocumentRepository documentRepository,
    IFileHasher fileHasher,
    IExternalKnowledgeProvider externalKnowledgeProvider,
    IDocumentTextExtractor textExtractor,
    IDateTimeProvider dateTimeProvider,
    IDocumentChunkRepository? chunkRepository = null,
    ISemanticIndexService? semanticIndexService = null,
    ITextChunker? textChunker = null,
    IKnowledgeArticleRepository? articleRepository = null,
    ExternalImportOptions? options = null) : IExternalEnrichmentService
{
    private readonly ExternalImportOptions _options = options ?? new ExternalImportOptions();

    public async Task<ExternalImportResultDto> ImportFromSourceAsync(
        Guid sourceId,
        ExternalImportCommand? command = null,
        CancellationToken cancellationToken = default)
    {
        if (sourceId == Guid.Empty)
        {
            throw new ValidationException("SourceId cannot be empty.");
        }

        var source = await sourceRepository.GetByIdAsync(sourceId, cancellationToken)
            ?? throw new NotFoundException("KnowledgeSource", sourceId);

        if (!source.IsApproved)
        {
            throw new ValidationException($"Source '{source.Name}' (ID: {source.Id}) is not approved. Only approved external sources can be imported.");
        }

        if (source.SourceType is not (SourceType.External or SourceType.VendorDocumentation))
        {
            throw new ValidationException($"Source '{source.Name}' has SourceType '{source.SourceType}'. Only External and VendorDocumentation sources are eligible for online enrichment.");
        }

        var targetUrl = !string.IsNullOrWhiteSpace(command?.UrlOverride)
            ? command.UrlOverride.Trim()
            : source.Url;

        if (string.IsNullOrWhiteSpace(targetUrl))
        {
            throw new ValidationException($"Source '{source.Name}' has no URL configured.");
        }

        if (!Uri.TryCreate(targetUrl, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new ValidationException($"Source URL '{targetUrl}' is invalid. Only absolute HTTP and HTTPS URLs are supported.");
        }

        var fetchRequest = new ExternalContentRequest(
            Url: targetUrl,
            Timeout: command?.Timeout ?? TimeSpan.FromSeconds(_options.TimeoutSeconds),
            MaxContentSizeBytes: command?.MaxContentSizeBytes ?? _options.MaxContentSizeBytes);

        var fetchResult = await externalKnowledgeProvider.FetchContentAsync(fetchRequest, cancellationToken);
        if (!fetchResult.IsSuccess)
        {
            return new ExternalImportResultDto(
                IsSuccess: false,
                SourceId: source.Id,
                SourceName: source.Name,
                SourceType: source.SourceType,
                Url: targetUrl,
                DocumentId: null,
                FileName: null,
                ContentType: null,
                FileHash: null,
                ChunkCount: null,
                IsDuplicate: false,
                ErrorCategory: fetchResult.ErrorCategory,
                ErrorMessage: fetchResult.ErrorMessage,
                Timestamp: fetchResult.RetrievedAt);
        }

        var fileHash = fileHasher.ComputeHash(fetchResult.Content!);

        // Check for duplicate import by content hash
        var existingDoc = await documentRepository.GetByHashAsync(fileHash, cancellationToken);
        if (existingDoc != null)
        {
            int existingChunkCount = 0;
            if (chunkRepository != null)
            {
                var existingChunks = await chunkRepository.GetByDocumentIdAsync(existingDoc.Id, cancellationToken);
                existingChunkCount = existingChunks.Count;
            }

            return new ExternalImportResultDto(
                IsSuccess: true,
                SourceId: source.Id,
                SourceName: source.Name,
                SourceType: source.SourceType,
                Url: targetUrl,
                DocumentId: existingDoc.Id,
                FileName: existingDoc.FileName,
                ContentType: existingDoc.ContentType,
                FileHash: fileHash,
                ChunkCount: existingChunkCount,
                IsDuplicate: true,
                ErrorCategory: null,
                ErrorMessage: null,
                Timestamp: dateTimeProvider.UtcNow);
        }

        var fileName = command?.CustomFileName?.Trim();
        if (string.IsNullOrWhiteSpace(fileName))
        {
            var urlPathName = Path.GetFileName(uri.LocalPath);
            if (!string.IsNullOrWhiteSpace(urlPathName) && urlPathName.Contains('.'))
            {
                fileName = urlPathName;
            }
            else
            {
                var ext = GetDefaultExtensionForContentType(fetchResult.ContentType);
                fileName = $"{source.Name.Replace(' ', '_')}_{dateTimeProvider.UtcNow:yyyyMMddHHmmss}{ext}";
            }
        }

        var docId = Guid.NewGuid();
        var storagePath = $"documents/external/{source.Id}/{docId}_{fileName}";
        var uploaderId = command?.ImportedById ?? Guid.NewGuid();
        var now = dateTimeProvider.UtcNow;

        var document = new Document(
            docId,
            fileName,
            storagePath,
            fetchResult.ContentType ?? "text/plain",
            fileHash,
            source.Id,
            uploaderId,
            now);

        await documentRepository.AddAsync(document, cancellationToken);

        document.MarkProcessing();

        string extractedText = string.Empty;
        using (var stream = new MemoryStream(fetchResult.Content!))
        {
            if (textExtractor.CanExtract(document.ContentType, document.FileName))
            {
                extractedText = await textExtractor.ExtractTextAsync(stream, document.ContentType, cancellationToken);
            }
            else if (document.ContentType.StartsWith("text/", StringComparison.OrdinalIgnoreCase) ||
                     document.ContentType.Contains("json", StringComparison.OrdinalIgnoreCase))
            {
                extractedText = System.Text.Encoding.UTF8.GetString(fetchResult.Content!);
            }
        }

        document.MarkProcessed(extractedText, now);
        await documentRepository.UpdateAsync(document, cancellationToken);

        int chunkCount = 0;
        var shouldChunk = command?.ChunkAndIndex ?? _options.ChunkOnImport;
        if (shouldChunk && !string.IsNullOrWhiteSpace(extractedText))
        {
            var chunker = textChunker ?? new TextChunker();
            var chunks = chunker.ChunkDocument(document.Id, extractedText, null, now);
            chunkCount = chunks.Count;

            if (chunkRepository != null && chunks.Count > 0)
            {
                await chunkRepository.AddRangeAsync(chunks, cancellationToken);
            }

            if (semanticIndexService != null && chunks.Count > 0)
            {
                await semanticIndexService.IndexDocumentChunksAsync(document.Id, chunks, cancellationToken);
            }
        }

        return new ExternalImportResultDto(
            IsSuccess: true,
            SourceId: source.Id,
            SourceName: source.Name,
            SourceType: source.SourceType,
            Url: targetUrl,
            DocumentId: document.Id,
            FileName: document.FileName,
            ContentType: document.ContentType,
            FileHash: fileHash,
            ChunkCount: chunkCount,
            IsDuplicate: false,
            ErrorCategory: null,
            ErrorMessage: null,
            Timestamp: now);
    }

    public async Task<KnowledgeArticleDto> ProposeArticleFromDocumentAsync(
        ProposeArticleFromDocumentCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.DocumentId == Guid.Empty)
        {
            throw new ValidationException("DocumentId cannot be empty.");
        }

        if (command.AuthorId == Guid.Empty)
        {
            throw new ValidationException("AuthorId cannot be empty.");
        }

        if (command.KnowledgeTypeId == Guid.Empty)
        {
            throw new ValidationException("KnowledgeTypeId cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(command.Title))
        {
            throw new ValidationException("Title is required when proposing an article.");
        }

        var document = await documentRepository.GetByIdAsync(command.DocumentId, cancellationToken)
            ?? throw new NotFoundException("Document", command.DocumentId);

        var source = await sourceRepository.GetByIdAsync(document.SourceId, cancellationToken)
            ?? throw new NotFoundException("KnowledgeSource", document.SourceId);

        var title = command.Title.Trim();
        var slug = Slug.FromTitle(title);
        if (articleRepository != null && await articleRepository.ExistsBySlugAsync(slug, cancellationToken))
        {
            slug = Slug.FromTitle($"{title}-{Guid.NewGuid():N}"[..Math.Min(100, title.Length + 8)]);
        }

        var now = dateTimeProvider.UtcNow;
        var summary = command.Summary?.Trim() ?? string.Empty;
        var content = !string.IsNullOrWhiteSpace(command.Content)
            ? command.Content.Trim()
            : (document.ExtractedText ?? string.Empty);

        // Proposals are always created in Draft state with Unverified status.
        // They strictly require human review before publication.
        var article = new KnowledgeArticle(
            Guid.NewGuid(),
            title,
            slug,
            summary,
            content,
            command.KnowledgeTypeId,
            command.AuthorId,
            now);

        if (command.ProductIds != null)
        {
            foreach (var pid in command.ProductIds) article.AddProduct(pid);
        }

        if (command.TechnologyIds != null)
        {
            foreach (var tid in command.TechnologyIds) article.AddTechnology(tid);
        }

        if (command.TagIds != null)
        {
            foreach (var tagId in command.TagIds) article.AddTag(tagId);
        }

        var referenceText = !string.IsNullOrWhiteSpace(command.ReferenceText)
            ? command.ReferenceText.Trim()
            : $"Proposed from external source: {source.Name} ({source.Url})";

        article.AddSource(source.Id, referenceText);

        if (articleRepository != null)
        {
            await articleRepository.AddAsync(article, cancellationToken);
        }

        return KnowledgeArticleDto.FromDomain(article);
    }

    private static string GetDefaultExtensionForContentType(string? contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType)) return ".txt";
        var lower = contentType.ToLowerInvariant();
        if (lower.Contains("html")) return ".html";
        if (lower.Contains("markdown") || lower.Contains("md")) return ".md";
        if (lower.Contains("json")) return ".json";
        if (lower.Contains("xml")) return ".xml";
        if (lower.Contains("csv")) return ".csv";
        if (lower.Contains("pdf")) return ".pdf";
        return ".txt";
    }
}
