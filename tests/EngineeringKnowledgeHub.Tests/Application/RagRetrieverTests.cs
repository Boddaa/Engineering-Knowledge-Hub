namespace EngineeringKnowledgeHub.Tests.Application;

using EngineeringKnowledgeHub.Application.Common.Exceptions;
using EngineeringKnowledgeHub.Application.Rag;
using EngineeringKnowledgeHub.Application.Search;
using EngineeringKnowledgeHub.Application.SemanticSearch;
using EngineeringKnowledgeHub.Domain.Entities;
using EngineeringKnowledgeHub.Domain.Enums;
using EngineeringKnowledgeHub.Domain.ValueObjects;
using EngineeringKnowledgeHub.Tests.Common;
using Xunit;

public class RagRetrieverTests
{
    private readonly InMemoryKnowledgeArticleRepository _articleRepo = new();
    private readonly InMemoryIncidentRepository _incidentRepo = new();
    private readonly InMemoryDocumentRepository _docRepo = new();
    private readonly InMemoryVectorStore _vectorStore = new();
    private readonly DeterministicEmbeddingProvider _embeddingProvider = new(dimensions: 32);
    private readonly SemanticSearchService _semanticSearchService;
    private readonly SemanticIndexService _semanticIndexService;
    private readonly SearchService _searchService;
    private readonly KnowledgeRetriever _retriever;

    public RagRetrieverTests()
    {
        _semanticSearchService = new SemanticSearchService(_embeddingProvider, _vectorStore);
        _semanticIndexService = new SemanticIndexService(_embeddingProvider, _vectorStore);
        _searchService = new SearchService(_articleRepo, _semanticSearchService);
        _retriever = new KnowledgeRetriever(
            _searchService,
            _semanticSearchService,
            _incidentRepo,
            _docRepo,
            _articleRepo);
    }

    [Fact]
    public async Task RetrieveEvidenceAsync_WithMatchingArticles_ShouldRetrieveAndRankEvidence()
    {
        var art1 = CreateArticle("ANPR Camera Network Configuration", "Camera setup instructions", "Connect camera to LAN port 8080.");
        var art2 = CreateArticle("Barrier Arm Sensor Alignment", "Mechanical barrier guide", "Align optical sensors.");

        await _articleRepo.AddAsync(art1);
        await _articleRepo.AddAsync(art2);
        await _semanticIndexService.IndexArticleAsync(art1);
        await _semanticIndexService.IndexArticleAsync(art2);

        var request = new RagRetrievalRequest
        {
            Query = "ANPR Camera Network",
            MaxEvidenceItems = 5
        };

        var evidence = await _retriever.RetrieveEvidenceAsync(request);

        Assert.NotEmpty(evidence);
        var first = evidence[0];
        Assert.Equal("KnowledgeArticle", first.SourceType);
        Assert.Equal(art1.Id, first.ReferenceId);
        Assert.Equal(1, first.CitationNumber);
        Assert.True(first.RelevanceScore > 0.0);
    }

    [Fact]
    public async Task RetrieveEvidenceAsync_WithDocumentChunks_ShouldRetrieveChunksFromSemanticStore()
    {
        var chunkId = Guid.NewGuid();
        var doc = new Document(Guid.NewGuid(), "TollGateManual.pdf", "/storage/docs/1", "application/pdf", "HASH123", Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow);
        await _docRepo.AddAsync(doc);

        var chunkRecord = VectorRecord.ForDocumentChunk(
            doc.Id,
            chunkId,
            0,
            "Emergency manual barrier release procedure: rotate release valve 90 degrees counterclockwise.",
            await _embeddingProvider.GenerateEmbeddingAsync("Emergency manual barrier release procedure"),
            new Dictionary<string, string>
            {
                ["DocumentFileName"] = doc.FileName
            });

        await _vectorStore.UpsertAsync(chunkRecord);

        var request = new RagRetrievalRequest
        {
            Query = "emergency manual barrier release valve",
            MaxEvidenceItems = 5,
            MinRelevanceScore = 0.1
        };

        var evidence = await _retriever.RetrieveEvidenceAsync(request);

        Assert.Contains(evidence, e => e.SourceType == "DocumentChunk" && e.ReferenceId == doc.Id);
    }

    [Fact]
    public async Task RetrieveEvidenceAsync_WithResolvedIncidents_ShouldIncludeMatchingIncidents()
    {
        var incident = new Incident(
            Guid.NewGuid(),
            "Camera RTSP Stream Lost",
            "Stream dropped on lane 3",
            IncidentSeverity.High,
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            "Black screen in live monitor");

        incident.Resolve(
            Guid.NewGuid(),
            "Replaced faulty POE injector and restarted switch.",
            "Network switch rebooted",
            DateTimeOffset.UtcNow);

        await _incidentRepo.AddAsync(incident);

        var request = new RagRetrievalRequest
        {
            Query = "faulty POE injector restarted switch",
            MaxEvidenceItems = 5,
            MinRelevanceScore = 0.1
        };

        var evidence = await _retriever.RetrieveEvidenceAsync(request);

        Assert.Contains(evidence, e => e.SourceType == "Incident" && e.ReferenceId == incident.Id);
    }

    [Fact]
    public async Task RetrieveEvidenceAsync_WithMinRelevanceScore_ShouldFilterOutLowRelevanceCandidates()
    {
        var art = CreateArticle("Database Backup Procedure", "SQL Server backups", "Run backup database command.");
        await _articleRepo.AddAsync(art);
        await _semanticIndexService.IndexArticleAsync(art);

        var request = new RagRetrievalRequest
        {
            Query = "Optical laser alignment for gate barriers",
            MinRelevanceScore = 0.95 // very high threshold
        };

        var evidence = await _retriever.RetrieveEvidenceAsync(request);

        Assert.Empty(evidence);
    }

    [Fact]
    public async Task RetrieveEvidenceAsync_WithMaxEvidenceItems_ShouldLimitResultCount()
    {
        for (int i = 1; i <= 6; i++)
        {
            var art = CreateArticle($"Troubleshooting Article {i}", $"Summary {i}", $"Procedure step details {i}.");
            await _articleRepo.AddAsync(art);
            await _semanticIndexService.IndexArticleAsync(art);
        }

        var request = new RagRetrievalRequest
        {
            Query = "Troubleshooting",
            MaxEvidenceItems = 3
        };

        var evidence = await _retriever.RetrieveEvidenceAsync(request);

        Assert.True(evidence.Count <= 3);
        // Verify sequential citation numbers 1..N
        for (int i = 0; i < evidence.Count; i++)
        {
            Assert.Equal(i + 1, evidence[i].CitationNumber);
        }
    }

    [Fact]
    public async Task RetrieveEvidenceAsync_WithPreferVerified_ShouldRankVerifiedArticlesHigher()
    {
        var artUnverified = CreateArticle("Firmware Update Procedure", "Firmware upgrade steps", "Flash binary to camera.", isApproved: false);
        var artVerified = CreateArticle("Firmware Update Procedure", "Firmware upgrade steps", "Flash binary to camera.", isApproved: true);

        await _articleRepo.AddAsync(artUnverified);
        await _articleRepo.AddAsync(artVerified);
        await _semanticIndexService.IndexArticleAsync(artUnverified);
        await _semanticIndexService.IndexArticleAsync(artVerified);

        var request = new RagRetrievalRequest
        {
            Query = "Firmware Update Procedure",
            PreferVerified = true,
            MaxEvidenceItems = 10
        };

        var evidence = await _retriever.RetrieveEvidenceAsync(request);

        Assert.NotEmpty(evidence);
        var first = evidence[0];
        Assert.Equal(VerificationStatus.Verified, first.VerificationStatus);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task RetrieveEvidenceAsync_WithEmptyQuery_ShouldThrowValidationException(string query)
    {
        var request = new RagRetrievalRequest { Query = query };
        await Assert.ThrowsAsync<ValidationException>(() => _retriever.RetrieveEvidenceAsync(request));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(51)]
    public async Task RetrieveEvidenceAsync_WithInvalidMaxEvidenceItems_ShouldThrowValidationException(int max)
    {
        var request = new RagRetrievalRequest
        {
            Query = "Test",
            MaxEvidenceItems = max
        };

        await Assert.ThrowsAsync<ValidationException>(() => _retriever.RetrieveEvidenceAsync(request));
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(1.1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public async Task RetrieveEvidenceAsync_WithInvalidMinRelevanceScore_ShouldThrowValidationException(double score)
    {
        var request = new RagRetrievalRequest
        {
            Query = "Test",
            MinRelevanceScore = score
        };

        await Assert.ThrowsAsync<ValidationException>(() => _retriever.RetrieveEvidenceAsync(request));
    }

    [Fact]
    public async Task RetrieveEvidenceAsync_WithProductFilter_WhenIncidentRelatedOnlyToDifferentProduct_ShouldExcludeIncident()
    {
        var productA = Guid.NewGuid();
        var productB = Guid.NewGuid();

        var articleB = CreateArticle("Article B", "Summary B", "Content B", productId: productB);
        await _articleRepo.AddAsync(articleB);

        var incidentB = new Incident(
            Guid.NewGuid(),
            "Camera Switch Loop Incident",
            "Broadcast storm on switch",
            IncidentSeverity.High,
            Guid.NewGuid(),
            DateTimeOffset.UtcNow);

        incidentB.LinkArticle(articleB.Id, ArticleRelationshipType.RelatesTo);
        incidentB.Resolve(Guid.NewGuid(), "Disabled spanning tree port loop", "Loop", DateTimeOffset.UtcNow);
        await _incidentRepo.AddAsync(incidentB);

        var request = new RagRetrievalRequest
        {
            Query = "Broadcast storm spanning tree switch loop",
            ProductId = productA,
            MinRelevanceScore = 0.1
        };

        var evidence = await _retriever.RetrieveEvidenceAsync(request);

        Assert.DoesNotContain(evidence, e => e.SourceType == "Incident" && e.ReferenceId == incidentB.Id);
    }

    [Fact]
    public async Task RetrieveEvidenceAsync_WithTechnologyFilter_WhenIncidentRelatedOnlyToDifferentTechnology_ShouldExcludeIncident()
    {
        var techA = Guid.NewGuid();
        var techB = Guid.NewGuid();

        var articleB = CreateArticle("Article B", "Summary B", "Content B", technologyId: techB);
        await _articleRepo.AddAsync(articleB);

        var incidentB = new Incident(
            Guid.NewGuid(),
            "Database deadlock incident",
            "Deadlock on transaction commit",
            IncidentSeverity.High,
            Guid.NewGuid(),
            DateTimeOffset.UtcNow);

        incidentB.LinkArticle(articleB.Id, ArticleRelationshipType.RelatesTo);
        incidentB.Resolve(Guid.NewGuid(), "Optimized query isolation level", "Lock contention", DateTimeOffset.UtcNow);
        await _incidentRepo.AddAsync(incidentB);

        var request = new RagRetrievalRequest
        {
            Query = "deadlock transaction commit",
            TechnologyId = techA,
            MinRelevanceScore = 0.1
        };

        var evidence = await _retriever.RetrieveEvidenceAsync(request);

        Assert.DoesNotContain(evidence, e => e.SourceType == "Incident" && e.ReferenceId == incidentB.Id);
    }

    [Fact]
    public async Task RetrieveEvidenceAsync_WithTagFilter_WhenIncidentRelatedOnlyToDifferentTag_ShouldExcludeIncident()
    {
        var tagA = Guid.NewGuid();
        var tagB = Guid.NewGuid();

        var articleB = CreateArticle("Article B", "Summary B", "Content B", tagId: tagB);
        await _articleRepo.AddAsync(articleB);

        var incidentB = new Incident(
            Guid.NewGuid(),
            "Power supply voltage fluctuation",
            "Voltage drop on controller board",
            IncidentSeverity.Critical,
            Guid.NewGuid(),
            DateTimeOffset.UtcNow);

        incidentB.LinkArticle(articleB.Id, ArticleRelationshipType.RelatesTo);
        incidentB.Resolve(Guid.NewGuid(), "Replaced power supply unit", "Faulty capacitor", DateTimeOffset.UtcNow);
        await _incidentRepo.AddAsync(incidentB);

        var request = new RagRetrievalRequest
        {
            Query = "voltage drop controller power supply",
            TagId = tagA,
            MinRelevanceScore = 0.1
        };

        var evidence = await _retriever.RetrieveEvidenceAsync(request);

        Assert.DoesNotContain(evidence, e => e.SourceType == "Incident" && e.ReferenceId == incidentB.Id);
    }

    [Fact]
    public async Task RetrieveEvidenceAsync_WithKnowledgeTypeFilter_WhenIncidentRelatedOnlyToDifferentType_ShouldExcludeIncident()
    {
        var typeA = Guid.NewGuid();
        var typeB = Guid.NewGuid();

        var articleB = CreateArticle("Article B", "Summary B", "Content B", knowledgeTypeId: typeB);
        await _articleRepo.AddAsync(articleB);

        var incidentB = new Incident(
            Guid.NewGuid(),
            "Camera lens blur incident",
            "Blurry image capture",
            IncidentSeverity.Medium,
            Guid.NewGuid(),
            DateTimeOffset.UtcNow);

        incidentB.LinkArticle(articleB.Id, ArticleRelationshipType.RelatesTo);
        incidentB.Resolve(Guid.NewGuid(), "Adjusted focus ring", "Out of focus", DateTimeOffset.UtcNow);
        await _incidentRepo.AddAsync(incidentB);

        var request = new RagRetrievalRequest
        {
            Query = "Blurry image focus ring",
            KnowledgeTypeId = typeA,
            MinRelevanceScore = 0.1
        };

        var evidence = await _retriever.RetrieveEvidenceAsync(request);

        Assert.DoesNotContain(evidence, e => e.SourceType == "Incident" && e.ReferenceId == incidentB.Id);
    }

    [Fact]
    public async Task RetrieveEvidenceAsync_WithVerificationStatusFilter_WhenIncidentLinkedOnlyToUnverifiedArticle_ShouldExcludeIncident()
    {
        var articleUnverified = CreateArticle("Unverified Article", "Summary", "Content", isApproved: false);
        await _articleRepo.AddAsync(articleUnverified);

        var incident = new Incident(
            Guid.NewGuid(),
            "Fiber optic cable cut",
            "Fiber signal loss",
            IncidentSeverity.Critical,
            Guid.NewGuid(),
            DateTimeOffset.UtcNow);

        incident.LinkArticle(articleUnverified.Id, ArticleRelationshipType.RelatesTo);
        incident.Resolve(Guid.NewGuid(), "Spliced fiber optic core", "Cut cable", DateTimeOffset.UtcNow);
        await _incidentRepo.AddAsync(incident);

        var request = new RagRetrievalRequest
        {
            Query = "fiber signal loss spliced cable",
            VerificationStatus = VerificationStatus.Verified,
            MinRelevanceScore = 0.1
        };

        var evidence = await _retriever.RetrieveEvidenceAsync(request);

        Assert.DoesNotContain(evidence, e => e.SourceType == "Incident" && e.ReferenceId == incident.Id);
    }

    [Fact]
    public async Task RetrieveEvidenceAsync_WithStructuredFilter_WhenIncidentHasNoLinkedArticles_ShouldExcludeIncident()
    {
        var targetProduct = Guid.NewGuid();

        var incidentUnlinked = new Incident(
            Guid.NewGuid(),
            "Unlinked Hardware Freeze",
            "Board freeze on boot",
            IncidentSeverity.High,
            Guid.NewGuid(),
            DateTimeOffset.UtcNow);

        incidentUnlinked.Resolve(Guid.NewGuid(), "Hard power cycled controller", "Watchdog freeze", DateTimeOffset.UtcNow);
        await _incidentRepo.AddAsync(incidentUnlinked);

        var request = new RagRetrievalRequest
        {
            Query = "Board freeze watchdog power cycle",
            ProductId = targetProduct,
            MinRelevanceScore = 0.1
        };

        var evidence = await _retriever.RetrieveEvidenceAsync(request);

        Assert.DoesNotContain(evidence, e => e.SourceType == "Incident" && e.ReferenceId == incidentUnlinked.Id);
    }

    private static KnowledgeArticle CreateArticle(
        string title,
        string summary,
        string content,
        Guid? knowledgeTypeId = null,
        Guid? productId = null,
        Guid? technologyId = null,
        Guid? tagId = null,
        bool isApproved = true)
    {
        var slug = Slug.FromTitle(title + "-" + Guid.NewGuid().ToString("N")[..6]);
        var article = new KnowledgeArticle(
            Guid.NewGuid(),
            title,
            slug,
            summary,
            content,
            knowledgeTypeId ?? Guid.NewGuid(),
            Guid.NewGuid(),
            DateTimeOffset.UtcNow);

        if (productId.HasValue) article.AddProduct(productId.Value);
        if (technologyId.HasValue) article.AddTechnology(technologyId.Value);
        if (tagId.HasValue) article.AddTag(tagId.Value);

        if (isApproved)
        {
            article.SubmitForReview();
            article.Approve(Guid.NewGuid(), DateTimeOffset.UtcNow);
        }

        return article;
    }
}
