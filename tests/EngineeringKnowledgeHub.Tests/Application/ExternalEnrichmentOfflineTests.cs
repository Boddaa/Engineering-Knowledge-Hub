namespace EngineeringKnowledgeHub.Tests.Application;

using System.Text;
using EngineeringKnowledgeHub.Application.Articles;
using EngineeringKnowledgeHub.Application.Common.Interfaces;
using EngineeringKnowledgeHub.Application.Common.Services;
using EngineeringKnowledgeHub.Application.Enrichment;
using EngineeringKnowledgeHub.Application.Rag;
using EngineeringKnowledgeHub.Application.Search;
using EngineeringKnowledgeHub.Application.SemanticSearch;
using EngineeringKnowledgeHub.Domain.Entities;
using EngineeringKnowledgeHub.Domain.Enums;
using EngineeringKnowledgeHub.Domain.ValueObjects;
using EngineeringKnowledgeHub.Tests.Common;
using Xunit;

public class ExternalEnrichmentOfflineTests
{
    private readonly InMemoryKnowledgeSourceRepository _sourceRepo = new();
    private readonly InMemoryDocumentRepository _docRepo = new();
    private readonly InMemoryDocumentChunkRepository _chunkRepo = new();
    private readonly InMemoryKnowledgeArticleRepository _articleRepo = new();
    private readonly InMemoryIncidentRepository _incidentRepo = new();
    private readonly Sha256FileHasher _fileHasher = new();
    private readonly PlainTextDocumentExtractor _textExtractor = new();
    private readonly TextChunker _textChunker = new();
    private readonly FakeDateTimeProvider _dateTimeProvider = new(DateTimeOffset.UtcNow);
    private readonly DeterministicEmbeddingProvider _embeddingProvider = new();
    private readonly InMemoryVectorStore _vectorStore = new();
    private readonly SemanticIndexService _semanticIndexService;
    private readonly SemanticSearchService _semanticSearchService;
    private readonly SearchService _searchService;
    private readonly KnowledgeRetriever _retriever;
    private readonly ContextBuilder _contextBuilder = new();
    private readonly DeterministicAiProvider _aiProvider = new();
    private readonly RagService _ragService;

    public ExternalEnrichmentOfflineTests()
    {
        _semanticIndexService = new SemanticIndexService(_embeddingProvider, _vectorStore);
        _semanticSearchService = new SemanticSearchService(_embeddingProvider, _vectorStore);
        _searchService = new SearchService(_articleRepo, _semanticSearchService);
        _retriever = new KnowledgeRetriever(
            _searchService,
            _semanticSearchService,
            _incidentRepo,
            _docRepo,
            _articleRepo,
            _sourceRepo);
        _ragService = new RagService(_retriever, _contextBuilder, _aiProvider, _dateTimeProvider);
    }

    private ExternalEnrichmentService CreateEnrichmentService(IExternalKnowledgeProvider provider)
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
            articleRepository: _articleRepo);
    }

    [Fact]
    public async Task WhenInternetFails_LocalSearchAndRagRemainFullyFunctional()
    {
        // 1. Add internal verified article
        var article = new KnowledgeArticle(
            Guid.NewGuid(),
            "Barrier Control Board Setup",
            Slug.FromTitle("barrier-control-board-setup"),
            "Summary of barrier setup",
            "Configure dip switch 3 to ON for automatic closing.",
            Guid.NewGuid(),
            Guid.NewGuid(),
            _dateTimeProvider.UtcNow);
        article.SubmitForReview();
        article.Approve(Guid.NewGuid(), _dateTimeProvider.UtcNow);
        await _articleRepo.AddAsync(article);
        await _semanticIndexService.IndexArticleAsync(article);

        // 2. Simulate complete network blackout for external provider
        var offlineProvider = new OfflineThrowingProvider();
        var enrichmentService = CreateEnrichmentService(offlineProvider);

        var source = new KnowledgeSource(
            Guid.NewGuid(),
            "Vendor Specs",
            SourceType.VendorDocumentation,
            _dateTimeProvider.UtcNow,
            url: "https://vendor.example.com/specs.txt",
            isApproved: true);
        await _sourceRepo.AddAsync(source);

        // Attempt external enrichment during network failure
        var importResult = await enrichmentService.ImportFromSourceAsync(source.Id);
        Assert.False(importResult.IsSuccess);
        Assert.Equal("NetworkError", importResult.ErrorCategory);

        // 3. Verify local search works completely offline
        var searchResults = await _searchService.SearchArticlesAsync(new SearchArticleRequest { Query = "barrier closing dip switch" });
        Assert.Single(searchResults.Items);
        Assert.Equal(article.Id, searchResults.Items[0].Id);

        // 4. Verify local RAG works completely offline without making external calls
        var ragResponse = await _ragService.AskAsync(new RagQuestionRequest
        {
            Question = "How to configure barrier automatic closing?"
        });

        Assert.True(ragResponse.IsSufficientEvidence);
        Assert.Contains(article.Id, ragResponse.Evidence.Select(e => e.ReferenceId));
    }

    [Fact]
    public async Task AfterImport_DocumentChunksRetrievedLocallyWithExternalProvenancePreserved()
    {
        // 1. Setup external source
        var source = new KnowledgeSource(
            Guid.NewGuid(),
            "Optics Laser Manual",
            SourceType.External,
            _dateTimeProvider.UtcNow,
            url: "https://optics.example.com/manual.txt",
            isApproved: true);
        await _sourceRepo.AddAsync(source);

        var docContent = "Optics sensor wavelength is 850nm. Calibrate sensor gain using potentiometer R12.";
        var fakeProvider = new SingleResultProvider(ExternalContentResult.Success(
            source.Url,
            Encoding.UTF8.GetBytes(docContent),
            "text/plain",
            200,
            _dateTimeProvider.UtcNow));

        var enrichmentService = CreateEnrichmentService(fakeProvider);

        // 2. Import into local storage
        var importResult = await enrichmentService.ImportFromSourceAsync(source.Id);
        Assert.True(importResult.IsSuccess);
        Assert.NotNull(importResult.DocumentId);

        // 3. Now simulate internet is cut off
        // Retrieval from RAG and search must succeed offline and preserve external provenance
        var evidence = await _retriever.RetrieveEvidenceAsync(new RagRetrievalRequest
        {
            Query = "Optics sensor wavelength potentiometer calibration"
        });

        var chunkEvidence = evidence.FirstOrDefault(e => e.SourceType == "DocumentChunk" && e.ReferenceId == importResult.DocumentId);
        Assert.NotNull(chunkEvidence);
        Assert.Null(chunkEvidence.VerificationStatus); // Must NOT be represented as verified internal knowledge
        Assert.Equal("true", chunkEvidence.Metadata["IsExternal"]);
        Assert.Equal(source.Id.ToString(), chunkEvidence.Metadata["SourceId"]);
        Assert.Equal("Optics Laser Manual", chunkEvidence.Metadata["SourceName"]);
        Assert.Equal("External", chunkEvidence.Metadata["SourceType"]);
        Assert.Equal(source.Url, chunkEvidence.Metadata["SourceUrl"]);
    }

    private sealed class OfflineThrowingProvider : IExternalKnowledgeProvider
    {
        public Task<ExternalContentResult> FetchContentAsync(ExternalContentRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(ExternalContentResult.Failure(
                request.Url,
                "DNS lookup failed: Name resolution error",
                "NetworkError",
                null,
                DateTimeOffset.UtcNow));
        }
    }

    private sealed class SingleResultProvider(ExternalContentResult result) : IExternalKnowledgeProvider
    {
        public Task<ExternalContentResult> FetchContentAsync(ExternalContentRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(result);
        }
    }
}
