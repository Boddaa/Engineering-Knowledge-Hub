namespace EngineeringKnowledgeHub.Tests.Application;

using System.Text;
using EngineeringKnowledgeHub.Application.Common.Exceptions;
using EngineeringKnowledgeHub.Application.Common.Interfaces;
using EngineeringKnowledgeHub.Application.Common.Services;
using EngineeringKnowledgeHub.Application.Enrichment;
using EngineeringKnowledgeHub.Application.SemanticSearch;
using EngineeringKnowledgeHub.Domain.Entities;
using EngineeringKnowledgeHub.Domain.Enums;
using EngineeringKnowledgeHub.Tests.Common;
using Xunit;

public class ExternalEnrichmentServiceTests
{
    private readonly InMemoryKnowledgeSourceRepository _sourceRepo = new();
    private readonly InMemoryDocumentRepository _docRepo = new();
    private readonly InMemoryDocumentChunkRepository _chunkRepo = new();
    private readonly InMemoryKnowledgeArticleRepository _articleRepo = new();
    private readonly Sha256FileHasher _fileHasher = new();
    private readonly PlainTextDocumentExtractor _textExtractor = new();
    private readonly TextChunker _textChunker = new();
    private readonly FakeDateTimeProvider _dateTimeProvider = new(DateTimeOffset.UtcNow);
    private readonly DeterministicEmbeddingProvider _embeddingProvider = new();
    private readonly InMemoryVectorStore _vectorStore = new();
    private readonly SemanticIndexService _semanticIndexService;

    public ExternalEnrichmentServiceTests()
    {
        _semanticIndexService = new SemanticIndexService(_embeddingProvider, _vectorStore);
    }

    private ExternalEnrichmentService CreateService(IExternalKnowledgeProvider provider)
    {
        return new ExternalEnrichmentService(
            sourceRepository: _sourceRepo,
            documentRepository: _docRepo,
            fileHasher: _fileHasher,
            externalKnowledgeProvider: provider,
            textExtractor: _textExtractor,
            dateTimeProvider: _dateTimeProvider,
            chunkRepository: _chunkRepo,
            semanticIndexService: _semanticIndexService,
            textChunker: _textChunker,
            articleRepository: _articleRepo,
            options: new ExternalImportOptions { TimeoutSeconds = 5, MaxContentSizeBytes = 1024 * 1024 });
    }

    private sealed class FakeExternalKnowledgeProvider(Func<ExternalContentRequest, ExternalContentResult> handler)
        : IExternalKnowledgeProvider
    {
        public Task<ExternalContentResult> FetchContentAsync(ExternalContentRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(handler(request));
        }
    }

    [Fact]
    public async Task ImportFromSourceAsync_WhenSourceNotApproved_ShouldThrowValidationException()
    {
        var source = new KnowledgeSource(
            Guid.NewGuid(),
            "Unapproved Camera API Docs",
            SourceType.VendorDocumentation,
            _dateTimeProvider.UtcNow,
            url: "https://vendor.example.com/api",
            isApproved: false);
        await _sourceRepo.AddAsync(source);

        var service = CreateService(new FakeExternalKnowledgeProvider(_ =>
            ExternalContentResult.Success("url", [1], "text/plain", 200, DateTimeOffset.UtcNow)));

        await Assert.ThrowsAsync<ValidationException>(() =>
            service.ImportFromSourceAsync(source.Id));
    }

    [Fact]
    public async Task ImportFromSourceAsync_WhenSourceTypeIsInternal_ShouldThrowValidationException()
    {
        var source = new KnowledgeSource(
            Guid.NewGuid(),
            "Internal Wiki",
            SourceType.Internal,
            _dateTimeProvider.UtcNow,
            url: "https://wiki.internal.local",
            isApproved: true);
        await _sourceRepo.AddAsync(source);

        var service = CreateService(new FakeExternalKnowledgeProvider(_ =>
            ExternalContentResult.Success("url", [1], "text/plain", 200, DateTimeOffset.UtcNow)));

        await Assert.ThrowsAsync<ValidationException>(() =>
            service.ImportFromSourceAsync(source.Id));
    }

    [Fact]
    public async Task ImportFromSourceAsync_WhenSourceHasNoUrl_ShouldThrowValidationException()
    {
        var source = new KnowledgeSource(
            Guid.NewGuid(),
            "External Book Reference",
            SourceType.External,
            _dateTimeProvider.UtcNow,
            url: null,
            isApproved: true);
        await _sourceRepo.AddAsync(source);

        var service = CreateService(new FakeExternalKnowledgeProvider(_ =>
            ExternalContentResult.Success("url", [1], "text/plain", 200, DateTimeOffset.UtcNow)));

        await Assert.ThrowsAsync<ValidationException>(() =>
            service.ImportFromSourceAsync(source.Id));
    }

    [Fact]
    public async Task ImportFromSourceAsync_WhenSourceNotFound_ShouldThrowNotFoundException()
    {
        var service = CreateService(new FakeExternalKnowledgeProvider(_ =>
            ExternalContentResult.Success("url", [1], "text/plain", 200, DateTimeOffset.UtcNow)));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.ImportFromSourceAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task ImportFromSourceAsync_WhenExternalRetrievalFails_ShouldReturnFailureResultWithProvenance()
    {
        var source = new KnowledgeSource(
            Guid.NewGuid(),
            "Axis Camera Protocol Specs",
            SourceType.VendorDocumentation,
            _dateTimeProvider.UtcNow,
            url: "https://axis.example.com/specs.txt",
            isApproved: true);
        await _sourceRepo.AddAsync(source);

        var fakeProvider = new FakeExternalKnowledgeProvider(req =>
            ExternalContentResult.Failure(req.Url, "Server returned 404", "HttpError", 404, _dateTimeProvider.UtcNow));

        var service = CreateService(fakeProvider);
        var result = await service.ImportFromSourceAsync(source.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal(source.Id, result.SourceId);
        Assert.Equal(source.Name, result.SourceName);
        Assert.Equal(SourceType.VendorDocumentation, result.SourceType);
        Assert.Equal("HttpError", result.ErrorCategory);
        Assert.Contains("404", result.ErrorMessage);
        Assert.Null(result.DocumentId);
    }

    [Fact]
    public async Task ImportFromSourceAsync_WhenSuccessful_ShouldCreateDocumentAndChunksWithFullProvenance()
    {
        var source = new KnowledgeSource(
            Guid.NewGuid(),
            "Camera Protocol Specs",
            SourceType.VendorDocumentation,
            _dateTimeProvider.UtcNow,
            url: "https://axis.example.com/specs.txt",
            isApproved: true);
        await _sourceRepo.AddAsync(source);

        var contentText = "RTSP communication protocol for cameras. Configure port 554 and auth digest.";
        var fakeProvider = new FakeExternalKnowledgeProvider(req =>
            ExternalContentResult.Success(req.Url, Encoding.UTF8.GetBytes(contentText), "text/plain", 200, _dateTimeProvider.UtcNow));

        var service = CreateService(fakeProvider);
        var result = await service.ImportFromSourceAsync(source.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(source.Id, result.SourceId);
        Assert.Equal(source.Name, result.SourceName);
        Assert.Equal(SourceType.VendorDocumentation, result.SourceType);
        Assert.Equal("https://axis.example.com/specs.txt", result.Url);
        Assert.NotNull(result.DocumentId);
        Assert.False(result.IsDuplicate);
        Assert.NotNull(result.FileHash);
        Assert.True(result.ChunkCount > 0);

        // Verify Document was persisted
        var savedDoc = await _docRepo.GetByIdAsync(result.DocumentId.Value);
        Assert.NotNull(savedDoc);
        Assert.Equal(source.Id, savedDoc.SourceId);
        Assert.Equal(DocumentProcessingStatus.Processed, savedDoc.ProcessingStatus);
        Assert.Equal(contentText, savedDoc.ExtractedText);

        // Verify Chunks were created
        var chunks = await _chunkRepo.GetByDocumentIdAsync(savedDoc.Id);
        Assert.NotEmpty(chunks);

        // Verify Vector Store was indexed
        Assert.NotNull(chunks[0].EmbeddingReference);
        var vectorRecord = await _vectorStore.GetByIdAsync(chunks[0].EmbeddingReference!);
        Assert.NotNull(vectorRecord);
        Assert.Equal(savedDoc.Id, vectorRecord.DocumentId);
    }

    [Fact]
    public async Task ImportFromSourceAsync_WhenDuplicateContentImported_ShouldReturnExistingDocumentWithoutDuplicate()
    {
        var source = new KnowledgeSource(
            Guid.NewGuid(),
            "Camera Protocol Specs",
            SourceType.VendorDocumentation,
            _dateTimeProvider.UtcNow,
            url: "https://axis.example.com/specs.txt",
            isApproved: true);
        await _sourceRepo.AddAsync(source);

        var contentText = "Duplicate content for repeated import test.";
        var fakeProvider = new FakeExternalKnowledgeProvider(req =>
            ExternalContentResult.Success(req.Url, Encoding.UTF8.GetBytes(contentText), "text/plain", 200, _dateTimeProvider.UtcNow));

        var service = CreateService(fakeProvider);

        // First import
        var firstResult = await service.ImportFromSourceAsync(source.Id);
        Assert.True(firstResult.IsSuccess);
        Assert.False(firstResult.IsDuplicate);

        // Second import with identical content
        var secondResult = await service.ImportFromSourceAsync(source.Id);
        Assert.True(secondResult.IsSuccess);
        Assert.True(secondResult.IsDuplicate);
        Assert.Equal(firstResult.DocumentId, secondResult.DocumentId);
        Assert.Equal(firstResult.FileHash, secondResult.FileHash);

        // Verify only 1 document exists in repository
        var allDocsForSource = await _docRepo.GetBySourceIdAsync(source.Id);
        Assert.Single(allDocsForSource);
    }

    [Fact]
    public async Task ProposeArticleFromDocumentAsync_ShouldCreateDraftUnverifiedArticleLinkedToSource()
    {
        var source = new KnowledgeSource(
            Guid.NewGuid(),
            "Vendor Hardware Guide",
            SourceType.VendorDocumentation,
            _dateTimeProvider.UtcNow,
            url: "https://hardware.example.com/guide.html",
            isApproved: true);
        await _sourceRepo.AddAsync(source);

        var doc = new Document(
            Guid.NewGuid(),
            "guide.html",
            "storage/path",
            "text/html",
            "hash123",
            source.Id,
            Guid.NewGuid(),
            _dateTimeProvider.UtcNow);
        doc.MarkProcessing();
        doc.MarkProcessed("Step 1: Check power wiring. Step 2: Configure RS485 baud rate.", _dateTimeProvider.UtcNow);
        await _docRepo.AddAsync(doc);

        var service = CreateService(new FakeExternalKnowledgeProvider(_ =>
            ExternalContentResult.Success("url", [1], "text/plain", 200, DateTimeOffset.UtcNow)));

        var command = new ProposeArticleFromDocumentCommand(
            DocumentId: doc.Id,
            AuthorId: Guid.NewGuid(),
            KnowledgeTypeId: Guid.NewGuid(),
            Title: "RS485 Barrier Controller Wiring",
            Summary: "Wiring guidelines proposed from vendor documentation.",
            ReferenceText: "Proposed from Axis hardware documentation");

        var articleDto = await service.ProposeArticleFromDocumentAsync(command);

        // Verify Draft and Unverified status - strictly preserving the human review invariant
        Assert.Equal(KnowledgeStatus.Draft, articleDto.Status);
        Assert.Equal(VerificationStatus.Unverified, articleDto.VerificationStatus);
        Assert.Equal("RS485 Barrier Controller Wiring", articleDto.Title);
        Assert.Contains(source.Id, articleDto.SourceIds);

        // Verify persisted in article repository
        var persistedArticle = await _articleRepo.GetByIdAsync(articleDto.Id);
        Assert.NotNull(persistedArticle);
        Assert.Equal(KnowledgeStatus.Draft, persistedArticle.Status);
        Assert.Equal(VerificationStatus.Unverified, persistedArticle.VerificationStatus);
    }

    [Fact]
    public async Task ProposeArticleFromDocumentAsync_WithNonExistentDocument_ShouldThrowNotFoundException()
    {
        var service = CreateService(new FakeExternalKnowledgeProvider(_ =>
            ExternalContentResult.Success("url", [1], "text/plain", 200, DateTimeOffset.UtcNow)));

        var command = new ProposeArticleFromDocumentCommand(
            DocumentId: Guid.NewGuid(),
            AuthorId: Guid.NewGuid(),
            KnowledgeTypeId: Guid.NewGuid(),
            Title: "Unknown Document Proposal");

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.ProposeArticleFromDocumentAsync(command));
    }

    [Fact]
    public async Task ImportFromSourceAsync_AlwaysUsesCanonicalSourceUrl_AndPreservesInResult()
    {
        var source = new KnowledgeSource(
            Guid.NewGuid(),
            "Canonical Vendor Specs",
            SourceType.VendorDocumentation,
            _dateTimeProvider.UtcNow,
            url: "https://canonical.vendor.example.com/specs.txt",
            isApproved: true);
        await _sourceRepo.AddAsync(source);

        string? retrievedUrl = null;
        var fakeProvider = new FakeExternalKnowledgeProvider(req =>
        {
            retrievedUrl = req.Url;
            return ExternalContentResult.Success(
                req.Url,
                Encoding.UTF8.GetBytes("Canonical specs content."),
                "text/plain",
                200,
                _dateTimeProvider.UtcNow);
        });

        var service = CreateService(fakeProvider);
        var command = new ExternalImportCommand(
            ImportedById: Guid.NewGuid(),
            CustomFileName: "custom_canonical.txt",
            ChunkAndIndex: true);

        var result = await service.ImportFromSourceAsync(source.Id, command);

        Assert.True(result.IsSuccess);
        Assert.Equal("https://canonical.vendor.example.com/specs.txt", retrievedUrl);
        Assert.Equal("https://canonical.vendor.example.com/specs.txt", result.Url);
        Assert.Equal(source.Id, result.SourceId);
        Assert.Equal(source.Name, result.SourceName);
    }

    [Fact]
    public async Task ImportFromSourceAsync_UsesConfiguredTimeoutAndMaxContentSizeBytes()
    {
        var source = new KnowledgeSource(
            Guid.NewGuid(),
            "Bounded Source Specs",
            SourceType.VendorDocumentation,
            _dateTimeProvider.UtcNow,
            url: "https://bounded.vendor.example.com/docs.txt",
            isApproved: true);
        await _sourceRepo.AddAsync(source);

        TimeSpan? observedTimeout = null;
        long? observedMaxSize = null;

        var fakeProvider = new FakeExternalKnowledgeProvider(req =>
        {
            observedTimeout = req.Timeout;
            observedMaxSize = req.MaxContentSizeBytes;
            return ExternalContentResult.Success(
                req.Url,
                Encoding.UTF8.GetBytes("Bounded content."),
                "text/plain",
                200,
                _dateTimeProvider.UtcNow);
        });

        var configuredOptions = new ExternalImportOptions
        {
            TimeoutSeconds = 12,
            MaxContentSizeBytes = 2048,
        };

        var service = new ExternalEnrichmentService(
            sourceRepository: _sourceRepo,
            documentRepository: _docRepo,
            fileHasher: _fileHasher,
            externalKnowledgeProvider: fakeProvider,
            textExtractor: _textExtractor,
            dateTimeProvider: _dateTimeProvider,
            chunkRepository: _chunkRepo,
            semanticIndexService: _semanticIndexService,
            textChunker: _textChunker,
            articleRepository: _articleRepo,
            options: configuredOptions);

        var result = await service.ImportFromSourceAsync(source.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(TimeSpan.FromSeconds(12), observedTimeout);
        Assert.Equal(2048, observedMaxSize);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void ExternalImportOptions_WhenConfiguredWithNonPositiveTimeout_ShouldThrowArgumentOutOfRangeException(int invalidTimeout)
    {
        var options = new ExternalImportOptions();
        Assert.Throws<ArgumentOutOfRangeException>(() => options.TimeoutSeconds = invalidTimeout);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-5000)]
    public void ExternalImportOptions_WhenConfiguredWithNonPositiveMaxSize_ShouldThrowArgumentOutOfRangeException(long invalidMaxSize)
    {
        var options = new ExternalImportOptions();
        Assert.Throws<ArgumentOutOfRangeException>(() => options.MaxContentSizeBytes = invalidMaxSize);
    }
}
