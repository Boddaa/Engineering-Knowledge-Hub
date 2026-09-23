namespace EngineeringKnowledgeHub.Tests.Application;

using EngineeringKnowledgeHub.Application.Common.Exceptions;
using EngineeringKnowledgeHub.Application.Common.Interfaces;
using EngineeringKnowledgeHub.Application.Search;
using EngineeringKnowledgeHub.Domain.Entities;
using EngineeringKnowledgeHub.Domain.Enums;
using EngineeringKnowledgeHub.Domain.ValueObjects;
using EngineeringKnowledgeHub.Tests.Common;
using Xunit;

public class SearchServiceTests
{
    private readonly InMemoryKnowledgeArticleRepository _articleRepository = new();
    private readonly SearchService _searchService;

    private readonly Guid _authorId = Guid.NewGuid();
    private readonly Guid _typeTroubleshooting = Guid.NewGuid();
    private readonly Guid _typeConfiguration = Guid.NewGuid();
    private readonly Guid _productCamera = Guid.NewGuid();
    private readonly Guid _productBarrier = Guid.NewGuid();
    private readonly Guid _techNet = Guid.NewGuid();
    private readonly Guid _techSql = Guid.NewGuid();
    private readonly Guid _tagSerial = Guid.NewGuid();
    private readonly Guid _tagTimeout = Guid.NewGuid();

    public SearchServiceTests()
    {
        _searchService = new SearchService(_articleRepository);
    }

    private KnowledgeArticle CreateSampleArticle(
        string title,
        string summary,
        string content,
        Guid typeId,
        DateTimeOffset createdAt,
        DateTimeOffset? updatedAt = null,
        KnowledgeStatus status = KnowledgeStatus.Published,
        VerificationStatus verificationStatus = VerificationStatus.Verified)
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
            createdAt);

        if (status == KnowledgeStatus.PendingReview)
        {
            article.SubmitForReview();
        }
        else if (status == KnowledgeStatus.Published)
        {
            article.SubmitForReview();
            article.Approve(Guid.NewGuid(), createdAt.AddMinutes(10));
        }

        if (updatedAt.HasValue)
        {
            article.UpdateContent(title, slug, summary, content, updatedAt.Value);
            if (status == KnowledgeStatus.Published)
            {
                article.SubmitForReview();
                article.Approve(Guid.NewGuid(), updatedAt.Value.AddMinutes(5));
            }
        }

        return article;
    }

    #region Validation Tests

    [Fact]
    public async Task SearchArticlesAsync_With_Null_Request_Should_Throw_ValidationException()
    {
        await Assert.ThrowsAsync<ValidationException>(() =>
            _searchService.SearchArticlesAsync(null!));
    }

    [Fact]
    public async Task SearchArticlesAsync_With_Page_Less_Than_1_Should_Throw_ValidationException()
    {
        var request = new SearchArticleRequest { Page = 0 };

        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            _searchService.SearchArticlesAsync(request));

        Assert.Contains("Page number", ex.Message);
    }

    [Fact]
    public async Task SearchArticlesAsync_With_PageSize_Zero_Should_Throw_ValidationException()
    {
        var request = new SearchArticleRequest { PageSize = 0 };

        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            _searchService.SearchArticlesAsync(request));

        Assert.Contains("Page size", ex.Message);
    }

    [Fact]
    public async Task SearchArticlesAsync_With_PageSize_Greater_Than_100_Should_Throw_ValidationException()
    {
        var request = new SearchArticleRequest { PageSize = 101 };

        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            _searchService.SearchArticlesAsync(request));

        Assert.Contains("Page size", ex.Message);
    }

    [Fact]
    public async Task SearchArticlesAsync_With_Empty_Guid_Filters_Should_Throw_ValidationException()
    {
        await Assert.ThrowsAsync<ValidationException>(() =>
            _searchService.SearchArticlesAsync(new SearchArticleRequest { ProductId = Guid.Empty }));

        await Assert.ThrowsAsync<ValidationException>(() =>
            _searchService.SearchArticlesAsync(new SearchArticleRequest { TechnologyId = Guid.Empty }));

        await Assert.ThrowsAsync<ValidationException>(() =>
            _searchService.SearchArticlesAsync(new SearchArticleRequest { KnowledgeTypeId = Guid.Empty }));

        await Assert.ThrowsAsync<ValidationException>(() =>
            _searchService.SearchArticlesAsync(new SearchArticleRequest { TagId = Guid.Empty }));
    }

    [Fact]
    public async Task SearchArticlesAsync_With_Invalid_SortOption_Should_Throw_ValidationException()
    {
        var request = new SearchArticleRequest { SortBy = (SearchSortOption)999 };

        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            _searchService.SearchArticlesAsync(request));

        Assert.Contains("sort option", ex.Message);
    }

    [Fact]
    public async Task SearchArticlesAsync_With_Invalid_Status_Filter_Should_Throw_ValidationException()
    {
        var request = new SearchArticleRequest { Status = (KnowledgeStatus)999 };

        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            _searchService.SearchArticlesAsync(request));

        Assert.Contains("status", ex.Message);
    }

    [Fact]
    public async Task SearchArticlesAsync_With_Invalid_VerificationStatus_Filter_Should_Throw_ValidationException()
    {
        var request = new SearchArticleRequest { VerificationStatus = (VerificationStatus)999 };

        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            _searchService.SearchArticlesAsync(request));

        Assert.Contains("verification status", ex.Message);
    }

    #endregion

    #region Search Matching and Zero-Result Tests

    [Fact]
    public async Task SearchArticlesAsync_With_Zero_Results_Should_Return_Empty_PagedResult_Without_Exception()
    {
        var request = new SearchArticleRequest { Query = "NonExistentTermXYZ" };

        var result = await _searchService.SearchArticlesAsync(request);

        Assert.NotNull(result);
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
        Assert.Equal(0, result.TotalPages);
        Assert.False(result.HasPreviousPage);
        Assert.False(result.HasNextPage);
    }

    [Fact]
    public async Task SearchArticlesAsync_Should_Match_By_Title()
    {
        var article = CreateSampleArticle(
            "RTSP Camera Frame Drop Troubleshooting",
            "Resolving frame drop issues in lane cameras.",
            "Detailed content about packet loss and buffer size.",
            _typeTroubleshooting,
            DateTimeOffset.UtcNow);

        await _articleRepository.AddAsync(article);

        var result = await _searchService.SearchArticlesAsync(new SearchArticleRequest
        {
            Query = "Frame Drop"
        });

        Assert.Single(result.Items);
        Assert.Equal(article.Id, result.Items[0].Id);
        Assert.Equal(article.Title, result.Items[0].Title);
    }

    [Fact]
    public async Task SearchArticlesAsync_Should_Match_By_Summary()
    {
        var article = CreateSampleArticle(
            "Barrier Controller Setup",
            "Configuration steps for RS485 loop detector interface.",
            "Connecting pin 3 and pin 4 to controller.",
            _typeConfiguration,
            DateTimeOffset.UtcNow);

        await _articleRepository.AddAsync(article);

        var result = await _searchService.SearchArticlesAsync(new SearchArticleRequest
        {
            Query = "loop detector"
        });

        Assert.Single(result.Items);
        Assert.Equal(article.Id, result.Items[0].Id);
    }

    [Fact]
    public async Task SearchArticlesAsync_Should_Match_By_Content()
    {
        var article = CreateSampleArticle(
            "SQL Server Maintenance",
            "Database index optimization guide.",
            "Rebuild clustered indexes when fragmentation exceeds 30 percent threshold.",
            _typeConfiguration,
            DateTimeOffset.UtcNow);

        await _articleRepository.AddAsync(article);

        var result = await _searchService.SearchArticlesAsync(new SearchArticleRequest
        {
            Query = "fragmentation"
        });

        Assert.Single(result.Items);
        Assert.Equal(article.Id, result.Items[0].Id);
        Assert.Contains("fragmentation", result.Items[0].Excerpt, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SearchArticlesAsync_Should_Be_Case_Insensitive()
    {
        var article = CreateSampleArticle(
            "ANPR Camera Calibration",
            "Optical zoom and focus configuration.",
            "Adjust iris and shutter speed for night lighting.",
            _typeConfiguration,
            DateTimeOffset.UtcNow);

        await _articleRepository.AddAsync(article);

        var resultLower = await _searchService.SearchArticlesAsync(new SearchArticleRequest { Query = "anpr camera" });
        var resultUpper = await _searchService.SearchArticlesAsync(new SearchArticleRequest { Query = "ANPR CAMERA" });

        Assert.Single(resultLower.Items);
        Assert.Single(resultUpper.Items);
        Assert.Equal(resultLower.Items[0].Id, resultUpper.Items[0].Id);
    }

    #endregion

    #region Filtering Tests

    [Fact]
    public async Task SearchArticlesAsync_Should_Filter_By_Product()
    {
        var cameraArticle = CreateSampleArticle("Camera Manual", "Summary", "Content", _typeConfiguration, DateTimeOffset.UtcNow);
        cameraArticle.AddProduct(_productCamera);

        var barrierArticle = CreateSampleArticle("Barrier Manual", "Summary", "Content", _typeConfiguration, DateTimeOffset.UtcNow);
        barrierArticle.AddProduct(_productBarrier);

        await _articleRepository.AddAsync(cameraArticle);
        await _articleRepository.AddAsync(barrierArticle);

        var result = await _searchService.SearchArticlesAsync(new SearchArticleRequest
        {
            ProductId = _productCamera
        });

        Assert.Single(result.Items);
        Assert.Equal(cameraArticle.Id, result.Items[0].Id);
    }

    [Fact]
    public async Task SearchArticlesAsync_Should_Filter_By_Technology()
    {
        var netArticle = CreateSampleArticle("ASP.NET Api Setup", "Summary", "Content", _typeConfiguration, DateTimeOffset.UtcNow);
        netArticle.AddTechnology(_techNet);

        var sqlArticle = CreateSampleArticle("SQL Server Clustering", "Summary", "Content", _typeConfiguration, DateTimeOffset.UtcNow);
        sqlArticle.AddTechnology(_techSql);

        await _articleRepository.AddAsync(netArticle);
        await _articleRepository.AddAsync(sqlArticle);

        var result = await _searchService.SearchArticlesAsync(new SearchArticleRequest
        {
            TechnologyId = _techNet
        });

        Assert.Single(result.Items);
        Assert.Equal(netArticle.Id, result.Items[0].Id);
    }

    [Fact]
    public async Task SearchArticlesAsync_Should_Filter_By_KnowledgeType()
    {
        var troubleshootingArticle = CreateSampleArticle("Issue 101", "Summary", "Content", _typeTroubleshooting, DateTimeOffset.UtcNow);
        var configArticle = CreateSampleArticle("Config 202", "Summary", "Content", _typeConfiguration, DateTimeOffset.UtcNow);

        await _articleRepository.AddAsync(troubleshootingArticle);
        await _articleRepository.AddAsync(configArticle);

        var result = await _searchService.SearchArticlesAsync(new SearchArticleRequest
        {
            KnowledgeTypeId = _typeTroubleshooting
        });

        Assert.Single(result.Items);
        Assert.Equal(troubleshootingArticle.Id, result.Items[0].Id);
    }

    [Fact]
    public async Task SearchArticlesAsync_Should_Filter_By_Tag()
    {
        var taggedArticle = CreateSampleArticle("Serial Port Timeout", "Summary", "Content", _typeTroubleshooting, DateTimeOffset.UtcNow);
        taggedArticle.AddTag(_tagSerial);

        var otherArticle = CreateSampleArticle("Network Latency", "Summary", "Content", _typeTroubleshooting, DateTimeOffset.UtcNow);
        otherArticle.AddTag(_tagTimeout);

        await _articleRepository.AddAsync(taggedArticle);
        await _articleRepository.AddAsync(otherArticle);

        var result = await _searchService.SearchArticlesAsync(new SearchArticleRequest
        {
            TagId = _tagSerial
        });

        Assert.Single(result.Items);
        Assert.Equal(taggedArticle.Id, result.Items[0].Id);
    }

    [Fact]
    public async Task SearchArticlesAsync_Should_Filter_By_Status()
    {
        var published = CreateSampleArticle("Published Article", "Summary", "Content", _typeConfiguration, DateTimeOffset.UtcNow, status: KnowledgeStatus.Published);
        var draft = new KnowledgeArticle(Guid.NewGuid(), "Draft Article", Slug.Create("draft-article"), "Summary", "Content", _typeConfiguration, _authorId, DateTimeOffset.UtcNow);

        await _articleRepository.AddAsync(published);
        await _articleRepository.AddAsync(draft);

        var result = await _searchService.SearchArticlesAsync(new SearchArticleRequest
        {
            Status = KnowledgeStatus.Draft
        });

        Assert.Single(result.Items);
        Assert.Equal(draft.Id, result.Items[0].Id);
    }

    [Fact]
    public async Task SearchArticlesAsync_Should_Filter_By_Combined_Criteria()
    {
        var matching = CreateSampleArticle("Target ANPR Camera Guide", "Summary", "Content", _typeConfiguration, DateTimeOffset.UtcNow);
        matching.AddProduct(_productCamera);
        matching.AddTechnology(_techNet);

        var wrongProduct = CreateSampleArticle("Target ANPR Barrier Guide", "Summary", "Content", _typeConfiguration, DateTimeOffset.UtcNow);
        wrongProduct.AddProduct(_productBarrier);
        wrongProduct.AddTechnology(_techNet);

        var wrongTech = CreateSampleArticle("Target ANPR Camera SQL Guide", "Summary", "Content", _typeConfiguration, DateTimeOffset.UtcNow);
        wrongTech.AddProduct(_productCamera);
        wrongTech.AddTechnology(_techSql);

        await _articleRepository.AddAsync(matching);
        await _articleRepository.AddAsync(wrongProduct);
        await _articleRepository.AddAsync(wrongTech);

        var result = await _searchService.SearchArticlesAsync(new SearchArticleRequest
        {
            Query = "Target ANPR",
            ProductId = _productCamera,
            TechnologyId = _techNet
        });

        Assert.Single(result.Items);
        Assert.Equal(matching.Id, result.Items[0].Id);
    }

    #endregion

    #region Sorting and Relevance Tests

    [Fact]
    public async Task SearchArticlesAsync_With_Relevance_Sort_Should_Rank_Title_Matches_Higher_Than_Content_Matches()
    {
        var contentMatch = CreateSampleArticle(
            "Hardware Component Overview",
            "General system overview",
            "This document discusses barcode scanners in depth.",
            _typeConfiguration,
            DateTimeOffset.UtcNow.AddDays(-5));

        var titleMatch = CreateSampleArticle(
            "Barcode Scanner Installation Guide",
            "Step by step guide",
            "Hardware details and serial connection pins.",
            _typeConfiguration,
            DateTimeOffset.UtcNow.AddDays(-10));

        await _articleRepository.AddAsync(contentMatch);
        await _articleRepository.AddAsync(titleMatch);

        var result = await _searchService.SearchArticlesAsync(new SearchArticleRequest
        {
            Query = "Barcode Scanner",
            SortBy = SearchSortOption.Relevance
        });

        Assert.Equal(2, result.Items.Count);
        Assert.Equal(titleMatch.Id, result.Items[0].Id);
        Assert.Equal(contentMatch.Id, result.Items[1].Id);
    }

    [Fact]
    public async Task SearchArticlesAsync_With_TitleAsc_And_TitleDesc_Should_Sort_Alphabetically()
    {
        var a = CreateSampleArticle("Alpha Configuration", "Summary", "Content", _typeConfiguration, DateTimeOffset.UtcNow);
        var b = CreateSampleArticle("Beta Configuration", "Summary", "Content", _typeConfiguration, DateTimeOffset.UtcNow);
        var c = CreateSampleArticle("Gamma Configuration", "Summary", "Content", _typeConfiguration, DateTimeOffset.UtcNow);

        await _articleRepository.AddAsync(b);
        await _articleRepository.AddAsync(c);
        await _articleRepository.AddAsync(a);

        var ascResult = await _searchService.SearchArticlesAsync(new SearchArticleRequest
        {
            SortBy = SearchSortOption.TitleAsc
        });

        Assert.Equal("Alpha Configuration", ascResult.Items[0].Title);
        Assert.Equal("Beta Configuration", ascResult.Items[1].Title);
        Assert.Equal("Gamma Configuration", ascResult.Items[2].Title);

        var descResult = await _searchService.SearchArticlesAsync(new SearchArticleRequest
        {
            SortBy = SearchSortOption.TitleDesc
        });

        Assert.Equal("Gamma Configuration", descResult.Items[0].Title);
        Assert.Equal("Beta Configuration", descResult.Items[1].Title);
        Assert.Equal("Alpha Configuration", descResult.Items[2].Title);
    }

    [Fact]
    public async Task SearchArticlesAsync_With_RecentlyCreated_Should_Sort_By_CreationDate_Descending()
    {
        var now = DateTimeOffset.UtcNow;
        var older = CreateSampleArticle("Older Article", "Summary", "Content", _typeConfiguration, now.AddDays(-10));
        var newer = CreateSampleArticle("Newer Article", "Summary", "Content", _typeConfiguration, now.AddDays(-1));

        await _articleRepository.AddAsync(older);
        await _articleRepository.AddAsync(newer);

        var result = await _searchService.SearchArticlesAsync(new SearchArticleRequest
        {
            SortBy = SearchSortOption.RecentlyCreated
        });

        Assert.Equal(newer.Id, result.Items[0].Id);
        Assert.Equal(older.Id, result.Items[1].Id);
    }

    #endregion

    #region Pagination Tests

    [Fact]
    public async Task SearchArticlesAsync_Should_Paginate_Correctly()
    {
        var now = DateTimeOffset.UtcNow;
        for (var i = 1; i <= 5; i++)
        {
            var article = CreateSampleArticle($"Article {i:D2}", "Summary", "Content", _typeConfiguration, now.AddMinutes(i));
            await _articleRepository.AddAsync(article);
        }

        // Page 1 of size 2
        var page1 = await _searchService.SearchArticlesAsync(new SearchArticleRequest
        {
            Page = 1,
            PageSize = 2,
            SortBy = SearchSortOption.TitleAsc
        });

        Assert.Equal(2, page1.Items.Count);
        Assert.Equal("Article 01", page1.Items[0].Title);
        Assert.Equal("Article 02", page1.Items[1].Title);
        Assert.Equal(5, page1.TotalCount);
        Assert.Equal(3, page1.TotalPages);
        Assert.False(page1.HasPreviousPage);
        Assert.True(page1.HasNextPage);

        // Page 2 of size 2
        var page2 = await _searchService.SearchArticlesAsync(new SearchArticleRequest
        {
            Page = 2,
            PageSize = 2,
            SortBy = SearchSortOption.TitleAsc
        });

        Assert.Equal(2, page2.Items.Count);
        Assert.Equal("Article 03", page2.Items[0].Title);
        Assert.Equal("Article 04", page2.Items[1].Title);
        Assert.True(page2.HasPreviousPage);
        Assert.True(page2.HasNextPage);

        // Page 3 of size 2
        var page3 = await _searchService.SearchArticlesAsync(new SearchArticleRequest
        {
            Page = 3,
            PageSize = 2,
            SortBy = SearchSortOption.TitleAsc
        });

        Assert.Single(page3.Items);
        Assert.Equal("Article 05", page3.Items[0].Title);
        Assert.True(page3.HasPreviousPage);
        Assert.False(page3.HasNextPage);
    }

    #endregion

    #region Boundary Failure Propagation

    [Fact]
    public async Task SearchArticlesAsync_When_Repository_Throws_Should_Propagate_Exception()
    {
        var failingRepo = new FailingKnowledgeArticleRepository();
        var service = new SearchService(failingRepo);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.SearchArticlesAsync(new SearchArticleRequest()));
    }

    private class FailingKnowledgeArticleRepository : IKnowledgeArticleRepository
    {
        public Task<KnowledgeArticle?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<IReadOnlyList<KnowledgeArticle>> GetAllAsync(CancellationToken cancellationToken = default) => throw new InvalidOperationException("Simulated repository failure.");
        public Task<bool> ExistsBySlugAsync(Slug slug, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task AddAsync(KnowledgeArticle article, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task UpdateAsync(KnowledgeArticle article, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    #endregion
}
