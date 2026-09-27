namespace EngineeringKnowledgeHub.Tests.Application;

using EngineeringKnowledgeHub.Application.Common.Exceptions;
using EngineeringKnowledgeHub.Application.Search;
using EngineeringKnowledgeHub.Application.SemanticSearch;
using EngineeringKnowledgeHub.Domain.Entities;
using EngineeringKnowledgeHub.Domain.Enums;
using EngineeringKnowledgeHub.Domain.ValueObjects;
using EngineeringKnowledgeHub.Tests.Common;
using Xunit;

public class HybridSearchTests
{
    private readonly InMemoryKnowledgeArticleRepository _articleRepository = new();
    private readonly InMemoryVectorStore _vectorStore = new();
    private readonly DeterministicEmbeddingProvider _embeddingProvider = new(dimensions: 32);
    private readonly SemanticSearchService _semanticSearchService;
    private readonly SemanticIndexService _semanticIndexService;
    private readonly SearchService _searchService;

    private readonly Guid _authorId = Guid.NewGuid();
    private readonly Guid _typeTroubleshooting = Guid.NewGuid();
    private readonly Guid _typeConfiguration = Guid.NewGuid();
    private readonly Guid _productCamera = Guid.NewGuid();
    private readonly Guid _techNet = Guid.NewGuid();

    public HybridSearchTests()
    {
        _semanticSearchService = new SemanticSearchService(_embeddingProvider, _vectorStore);
        _semanticIndexService = new SemanticIndexService(_embeddingProvider, _vectorStore);
        _searchService = new SearchService(_articleRepository, _semanticSearchService);
    }

    [Fact]
    public async Task HybridSearch_KeywordOnlyRelevance_WhenSemanticWeightIsZero_ShouldRankByKeyword()
    {
        var art1 = CreateAndSaveArticle("Exact Keyword Match", "Summary text", "Content details", _typeTroubleshooting);
        var art2 = CreateAndSaveArticle("Another Unrelated Article", "Summary text", "Keyword occurs in content", _typeTroubleshooting);

        await _semanticIndexService.IndexArticleAsync(art1);
        await _semanticIndexService.IndexArticleAsync(art2);

        var request = new HybridSearchArticleRequest
        {
            Query = "Exact Keyword Match",
            Options = new HybridSearchOptions { KeywordWeight = 1.0, SemanticWeight = 0.0, VerifiedBoost = 1.0 }
        };

        var result = await _searchService.HybridSearchArticlesAsync(request);

        Assert.NotEmpty(result.Items);
        Assert.Equal(art1.Id, result.Items[0].Id);
        Assert.NotNull(result.Items[0].KeywordScore);
        Assert.Equal(0.0, result.Items[0].SemanticScore);
        Assert.Equal(result.Items[0].KeywordScore, result.Items[0].HybridScore);
    }

    [Fact]
    public async Task HybridSearch_SemanticOnlyRelevance_WhenKeywordWeightIsZero_ShouldRankBySemanticSimilarity()
    {
        var art1 = CreateAndSaveArticle("Barrier Gate Operation", "Mechanical maintenance and shear pin guidance", "Ensure arm alignment.", _typeTroubleshooting);
        var art2 = CreateAndSaveArticle("Network Interface Card", "Ethernet port configuration", "IP address settings.", _typeConfiguration);

        await _semanticIndexService.IndexArticleAsync(art1);
        await _semanticIndexService.IndexArticleAsync(art2);

        var request = new HybridSearchArticleRequest
        {
            // Query has no exact keywords in art1's title, but strong conceptual overlap with barrier gate
            Query = "barrier gate arm alignment",
            Options = new HybridSearchOptions { KeywordWeight = 0.0, SemanticWeight = 1.0, VerifiedBoost = 1.0 }
        };

        var result = await _searchService.HybridSearchArticlesAsync(request);

        Assert.NotEmpty(result.Items);
        Assert.Equal(art1.Id, result.Items[0].Id);
        Assert.True(result.Items[0].SemanticScore > 0.0);
        Assert.Equal(result.Items[0].SemanticScore, result.Items[0].HybridScore);
    }

    [Fact]
    public async Task HybridSearch_CombinedRelevance_BothSignalsContributeToRanking()
    {
        // artBoth matches keywords AND has semantic vector similarity
        var artBoth = CreateAndSaveArticle("Camera Sensor Calibration", "Camera sensor calibration guide", "Calibrate optical sensor accurately.", _typeConfiguration);
        // artKeywordOnly matches some keyword but different topic
        var artKeywordOnly = CreateAndSaveArticle("Sensor Overview", "Overview of various sensor types", "General info without calibration.", _typeConfiguration);

        await _semanticIndexService.IndexArticleAsync(artBoth);
        await _semanticIndexService.IndexArticleAsync(artKeywordOnly);

        var request = new HybridSearchArticleRequest
        {
            Query = "Camera Sensor Calibration",
            Options = new HybridSearchOptions { KeywordWeight = 0.5, SemanticWeight = 0.5, VerifiedBoost = 1.0 }
        };

        var result = await _searchService.HybridSearchArticlesAsync(request);

        Assert.True(result.Items.Count >= 2);
        Assert.Equal(artBoth.Id, result.Items[0].Id);
        Assert.True(result.Items[0].HybridScore > result.Items[1].HybridScore);
        Assert.True(result.Items[0].KeywordScore > 0.0);
        Assert.True(result.Items[0].SemanticScore > 0.0);
    }

    [Fact]
    public async Task HybridSearch_VerifiedBoost_ShouldElevateVerifiedArticles()
    {
        var unverifiedArticle = CreateAndSaveArticle("ANPR Capture Tuning Lane 1", "Summary", "ANPR capture tuning steps.", _typeConfiguration, isApproved: false);
        var verifiedArticle = CreateAndSaveArticle("ANPR Capture Tuning Lane 2", "Summary", "ANPR capture tuning steps.", _typeConfiguration, isApproved: true);

        await _semanticIndexService.IndexArticleAsync(unverifiedArticle);
        await _semanticIndexService.IndexArticleAsync(verifiedArticle);

        var request = new HybridSearchArticleRequest
        {
            Query = "ANPR Capture Tuning",
            Options = new HybridSearchOptions { KeywordWeight = 0.5, SemanticWeight = 0.5, VerifiedBoost = 1.3 }
        };

        var result = await _searchService.HybridSearchArticlesAsync(request);

        Assert.NotEmpty(result.Items);
        var verifiedItem = result.Items.FirstOrDefault(i => i.Id == verifiedArticle.Id);
        var unverifiedItem = result.Items.FirstOrDefault(i => i.Id == unverifiedArticle.Id);

        Assert.NotNull(verifiedItem);
        Assert.NotNull(unverifiedItem);
        Assert.True(verifiedItem.HybridScore > unverifiedItem.HybridScore);
    }

    [Fact]
    public async Task HybridSearch_WithStructuredFilters_ShouldExcludeNonMatchingArticles()
    {
        var artCamera = CreateAndSaveArticle("Diagnostics 1", "Summary", "Diagnostics content", _typeTroubleshooting, productId: _productCamera);
        var artOther = CreateAndSaveArticle("Diagnostics 2", "Summary", "Diagnostics content", _typeTroubleshooting, productId: Guid.NewGuid());

        await _semanticIndexService.IndexArticleAsync(artCamera);
        await _semanticIndexService.IndexArticleAsync(artOther);

        var request = new HybridSearchArticleRequest
        {
            Query = "Diagnostics",
            ProductId = _productCamera
        };

        var result = await _searchService.HybridSearchArticlesAsync(request);

        Assert.Single(result.Items);
        Assert.Equal(artCamera.Id, result.Items[0].Id);
    }

    [Fact]
    public async Task SearchArticlesAsync_Phase3KeywordSearch_RemainsCompletelyIntactWithoutRegression()
    {
        var art = CreateAndSaveArticle("Phase 3 Compatibility Article", "Summary text", "Content text", _typeTroubleshooting);

        var keywordResult = await _searchService.SearchArticlesAsync(new SearchArticleRequest
        {
            Query = "Compatibility"
        });

        Assert.Single(keywordResult.Items);
        Assert.Equal(art.Id, keywordResult.Items[0].Id);
        Assert.Null(keywordResult.Items[0].SemanticScore);
    }

    [Theory]
    [InlineData(-0.1, 0.5, 1.0)]
    [InlineData(0.5, -0.1, 1.0)]
    [InlineData(0.0, 0.0, 1.0)]
    [InlineData(0.5, 0.5, 0.9)]
    public void HybridSearchOptions_Validation_ShouldRejectInvalidConfigurations(double kwWeight, double semWeight, double boost)
    {
        Assert.Throws<ValidationException>(() =>
            new HybridSearchOptions
            {
                KeywordWeight = kwWeight,
                SemanticWeight = semWeight,
                VerifiedBoost = boost
            }.Validate());
    }

    private KnowledgeArticle CreateAndSaveArticle(
        string title,
        string summary,
        string content,
        Guid typeId,
        Guid? productId = null,
        bool isApproved = true)
    {
        var slug = Slug.FromTitle(title);
        var article = new KnowledgeArticle(
            Guid.NewGuid(),
            title,
            slug,
            summary,
            content,
            typeId,
            _authorId,
            DateTimeOffset.UtcNow);

        if (productId.HasValue)
        {
            article.AddProduct(productId.Value);
        }

        if (isApproved)
        {
            article.SubmitForReview();
            article.Approve(Guid.NewGuid(), DateTimeOffset.UtcNow);
        }

        _articleRepository.AddAsync(article).GetAwaiter().GetResult();
        return article;
    }
}
