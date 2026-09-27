namespace EngineeringKnowledgeHub.Tests.Application;

using EngineeringKnowledgeHub.Application.Common.Exceptions;
using EngineeringKnowledgeHub.Application.SemanticSearch;
using EngineeringKnowledgeHub.Domain.Entities;
using EngineeringKnowledgeHub.Domain.Enums;
using EngineeringKnowledgeHub.Domain.ValueObjects;
using Xunit;

public class SemanticSearchServiceTests
{
    private readonly InMemoryVectorStore _vectorStore = new();
    private readonly DeterministicEmbeddingProvider _embeddingProvider = new(dimensions: 32);
    private readonly SemanticSearchService _searchService;
    private readonly SemanticIndexService _indexService;

    public SemanticSearchServiceTests()
    {
        _searchService = new SemanticSearchService(_embeddingProvider, _vectorStore);
        _indexService = new SemanticIndexService(_embeddingProvider, _vectorStore);
    }

    [Fact]
    public async Task SearchAsync_WithValidQuery_ShouldRetrieveAndRankCandidates()
    {
        var art1 = CreateArticle("ANPR Camera Connection Troubleshooting", "Guide for camera connectivity", "Check cable and network settings.");
        var art2 = CreateArticle("Barrier Arm Mechanical Replacement", "Maintenance of mechanical barrier arm", "Replace broken shear pins.");

        await _indexService.IndexArticleAsync(art1);
        await _indexService.IndexArticleAsync(art2);

        var request = new SemanticSearchRequest
        {
            Query = "camera network connectivity issue",
            TopK = 5
        };

        var results = await _searchService.SearchAsync(request);

        Assert.NotEmpty(results);
        Assert.Equal(art1.Id, results[0].ArticleId);
        Assert.True(results[0].SimilarityScore > 0.0);
    }

    [Fact]
    public async Task SearchAsync_WithTopK_ShouldLimitResultCount()
    {
        for (int i = 1; i <= 5; i++)
        {
            var art = CreateArticle($"Troubleshooting Guide {i}", $"Summary {i}", $"Diagnostics content {i}.");
            await _indexService.IndexArticleAsync(art);
        }

        var results = await _searchService.SearchAsync(new SemanticSearchRequest
        {
            Query = "Troubleshooting",
            TopK = 2
        });

        Assert.Equal(2, results.Count);
    }

    [Fact]
    public async Task SearchAsync_WithMetadataFilters_ShouldOnlyReturnMatchingCandidates()
    {
        var targetType = Guid.NewGuid();
        var otherType = Guid.NewGuid();

        var artMatch = CreateArticle("Camera Config", "Config summary", "Config details", knowledgeTypeId: targetType);
        var artOther = CreateArticle("Camera Config 2", "Config summary", "Config details", knowledgeTypeId: otherType);

        await _indexService.IndexArticleAsync(artMatch);
        await _indexService.IndexArticleAsync(artOther);

        var results = await _searchService.SearchAsync(new SemanticSearchRequest
        {
            Query = "Camera Config",
            KnowledgeTypeId = targetType
        });

        Assert.Single(results);
        Assert.Equal(artMatch.Id, results[0].ArticleId);
    }

    [Fact]
    public async Task SearchAsync_WithMinSimilarity_ShouldFilterOutLowRelevanceCandidates()
    {
        var art1 = CreateArticle("Camera ANPR plate capture", "ANPR capture procedure", "Detailed lane settings.");
        await _indexService.IndexArticleAsync(art1);

        var results = await _searchService.SearchAsync(new SemanticSearchRequest
        {
            Query = "Database backup and disaster recovery",
            MinSimilarity = 0.95 // Very high threshold
        });

        Assert.Empty(results);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SearchAsync_WithNullOrWhitespaceQuery_ShouldThrowValidationException(string? invalidQuery)
    {
        await Assert.ThrowsAsync<ValidationException>(() =>
            _searchService.SearchAsync(new SemanticSearchRequest { Query = invalidQuery! }));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task SearchAsync_WithInvalidTopK_ShouldThrowValidationException(int invalidTopK)
    {
        await Assert.ThrowsAsync<ValidationException>(() =>
            _searchService.SearchAsync(new SemanticSearchRequest { Query = "Valid query", TopK = invalidTopK }));
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(0.5)]
    [InlineData(1.0)]
    public async Task SearchAsync_WithValidMinSimilarityBoundaries_ShouldSucceed(double validMinSimilarity)
    {
        var art = CreateArticle("Boundary Test Article", "Summary", "Content");
        await _indexService.IndexArticleAsync(art);

        var results = await _searchService.SearchAsync(new SemanticSearchRequest
        {
            Query = "Boundary Test",
            MinSimilarity = validMinSimilarity
        });

        Assert.NotNull(results);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(-1.0)]
    public async Task SearchAsync_WithMinSimilarityBelowZero_ShouldThrowValidationException(double invalidMinSimilarity)
    {
        await Assert.ThrowsAsync<ValidationException>(() =>
            _searchService.SearchAsync(new SemanticSearchRequest
            {
                Query = "Valid query",
                MinSimilarity = invalidMinSimilarity
            }));
    }

    [Theory]
    [InlineData(1.01)]
    [InlineData(2.0)]
    public async Task SearchAsync_WithMinSimilarityAboveOne_ShouldThrowValidationException(double invalidMinSimilarity)
    {
        await Assert.ThrowsAsync<ValidationException>(() =>
            _searchService.SearchAsync(new SemanticSearchRequest
            {
                Query = "Valid query",
                MinSimilarity = invalidMinSimilarity
            }));
    }

    [Fact]
    public async Task SearchAsync_WithMinSimilarityNaNOrInfinity_ShouldThrowValidationException()
    {
        await Assert.ThrowsAsync<ValidationException>(() =>
            _searchService.SearchAsync(new SemanticSearchRequest
            {
                Query = "Valid query",
                MinSimilarity = double.NaN
            }));

        await Assert.ThrowsAsync<ValidationException>(() =>
            _searchService.SearchAsync(new SemanticSearchRequest
            {
                Query = "Valid query",
                MinSimilarity = double.PositiveInfinity
            }));
    }

    [Fact]
    public async Task IndexDocumentChunksAsync_ShouldIndexChunksAndSetEmbeddingReferences()
    {
        var docId = Guid.NewGuid();
        var chunks = new List<DocumentChunk>
        {
            new(Guid.NewGuid(), docId, 0, "First chunk about sensor calibration.", 5, null, DateTimeOffset.UtcNow),
            new(Guid.NewGuid(), docId, 1, "Second chunk about optical zoom settings.", 6, null, DateTimeOffset.UtcNow)
        };

        var records = await _indexService.IndexDocumentChunksAsync(docId, chunks);

        Assert.Equal(2, records.Count);
        Assert.NotNull(chunks[0].EmbeddingReference);
        Assert.NotNull(chunks[1].EmbeddingReference);

        var results = await _searchService.SearchAsync(new SemanticSearchRequest
        {
            Query = "optical zoom settings",
            DocumentId = docId
        });

        Assert.NotEmpty(results);
        Assert.Equal(chunks[1].Id, results[0].ChunkId);
    }

    [Fact]
    public async Task RemoveArticleIndexAsync_ShouldRemoveVectorFromStore()
    {
        var art = CreateArticle("Article to delete", "Summary", "Content");
        await _indexService.IndexArticleAsync(art);

        Assert.Equal(1, await _vectorStore.CountAsync());

        var removed = await _indexService.RemoveArticleIndexAsync(art.Id);

        Assert.True(removed);
        Assert.Equal(0, await _vectorStore.CountAsync());
    }

    private static KnowledgeArticle CreateArticle(
        string title,
        string summary,
        string content,
        Guid? knowledgeTypeId = null)
    {
        return new KnowledgeArticle(
            Guid.NewGuid(),
            title,
            Slug.FromTitle(title),
            summary,
            content,
            knowledgeTypeId ?? Guid.NewGuid(),
            Guid.NewGuid(),
            DateTimeOffset.UtcNow);
    }
}
