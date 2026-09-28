namespace EngineeringKnowledgeHub.Tests.Api;

using System.Net;
using System.Net.Http.Json;
using EngineeringKnowledgeHub.Api.Models;
using EngineeringKnowledgeHub.Application.DTOs;
using EngineeringKnowledgeHub.Application.SemanticSearch;
using EngineeringKnowledgeHub.Domain.Entities;
using EngineeringKnowledgeHub.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public class SemanticSearchApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public SemanticSearchApiTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task SemanticSearch_WithValidQuery_ShouldReturn200_WithExpectedResultStructure()
    {
        var uniqueTerm = $"OpticalSensor_{Guid.NewGuid():N}";
        var article = new KnowledgeArticle(
            Guid.NewGuid(),
            $"Troubleshooting {uniqueTerm} Laser Sensor",
            Slug.FromTitle($"Troubleshooting {uniqueTerm} Laser Sensor"),
            "Procedure for calibrating optical distance sensor",
            $"Step-by-step diagnostic guide for {uniqueTerm} signal alignment.",
            _factory.DefaultKnowledgeTypeId,
            Guid.NewGuid(),
            DateTimeOffset.UtcNow);

        using (var scope = _factory.Services.CreateScope())
        {
            var indexService = scope.ServiceProvider.GetRequiredService<ISemanticIndexService>();
            await indexService.IndexArticleAsync(article);
        }

        var request = new SemanticSearchApiRequest(
            Query: $"calibrating {uniqueTerm}",
            TopK: 5);

        var response = await _client.PostAsJsonAsync("/api/search/semantic", request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var results = await response.Content.ReadFromJsonAsync<List<SemanticSearchResultDto>>();
        Assert.NotNull(results);
        Assert.NotEmpty(results);

        var match = results.FirstOrDefault(r => r.ArticleId == article.Id);
        Assert.NotNull(match);
        Assert.False(string.IsNullOrWhiteSpace(match.Id));
        Assert.Equal(article.Id, match.ArticleId);
        Assert.False(string.IsNullOrWhiteSpace(match.Text));
        Assert.True(match.SimilarityScore > 0.0);
        Assert.NotNull(match.Metadata);
    }

    [Fact]
    public async Task SemanticSearch_ShouldNotExposeEmbeddingVectorsOrInternals()
    {
        var uniqueTerm = $"DenseVectorGuard_{Guid.NewGuid():N}";
        var article = new KnowledgeArticle(
            Guid.NewGuid(),
            $"Security Guide {uniqueTerm}",
            Slug.FromTitle($"Security Guide {uniqueTerm}"),
            "Summary",
            $"Content discussing {uniqueTerm}",
            _factory.DefaultKnowledgeTypeId,
            Guid.NewGuid(),
            DateTimeOffset.UtcNow);

        using (var scope = _factory.Services.CreateScope())
        {
            var indexService = scope.ServiceProvider.GetRequiredService<ISemanticIndexService>();
            await indexService.IndexArticleAsync(article);
        }

        var response = await _client.PostAsJsonAsync("/api/search/semantic", new SemanticSearchApiRequest(
            Query: uniqueTerm,
            TopK: 1));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var rawJson = await response.Content.ReadAsStringAsync();

        // Verify provider-specific embedding vectors and internals are NOT exposed
        Assert.DoesNotContain("\"vector\"", rawJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("\"embedding\"", rawJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("\"denseVector\"", rawJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("\"embeddingReference\"", rawJson, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\r\n")]
    public async Task SemanticSearch_WithNullOrWhitespaceQuery_ShouldReturn400BadRequest(string? whitespaceQuery)
    {
        var request = new { Query = whitespaceQuery, TopK = 5 };
        var response = await _client.PostAsJsonAsync("/api/search/semantic", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task SemanticSearch_WithInvalidTopK_ShouldReturn400BadRequest(int invalidTopK)
    {
        var request = new SemanticSearchApiRequest(
            Query: "Valid Search Query",
            TopK: invalidTopK);

        var response = await _client.PostAsJsonAsync("/api/search/semantic", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(1.5)]
    public async Task SemanticSearch_WithInvalidMinSimilarity_ShouldReturn400BadRequest(double invalidMinSimilarity)
    {
        var request = new SemanticSearchApiRequest(
            Query: "Valid Search Query",
            MinSimilarity: invalidMinSimilarity);

        var response = await _client.PostAsJsonAsync("/api/search/semantic", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
    }

    [Theory]
    [InlineData("ArticleId")]
    [InlineData("DocumentId")]
    [InlineData("ProductId")]
    [InlineData("TechnologyId")]
    [InlineData("KnowledgeTypeId")]
    [InlineData("TagId")]
    public async Task SemanticSearch_WithEmptyGuidFilter_ShouldReturn400BadRequest(string filterField)
    {
        var payload = new Dictionary<string, object>
        {
            ["query"] = "Valid query",
            [filterField] = Guid.Empty
        };

        var response = await _client.PostAsJsonAsync("/api/search/semantic", payload);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
    }

    [Theory]
    [InlineData("Status", 999)]
    [InlineData("VerificationStatus", 999)]
    public async Task SemanticSearch_WithInvalidEnumFilter_ShouldReturn400BadRequest(string enumField, int invalidValue)
    {
        var payload = new Dictionary<string, object>
        {
            ["query"] = "Valid query",
            [enumField] = invalidValue
        };

        var response = await _client.PostAsJsonAsync("/api/search/semantic", payload);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
    }

    [Fact]
    public async Task SemanticSearch_WhenNoCandidatesMatch_ShouldReturn200Ok_WithEmptyArray()
    {
        var request = new SemanticSearchApiRequest(
            Query: $"UnmatchedQuery_{Guid.NewGuid():N}",
            MinSimilarity: 0.999);

        var response = await _client.PostAsJsonAsync("/api/search/semantic", request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var results = await response.Content.ReadFromJsonAsync<List<SemanticSearchResultDto>>();
        Assert.NotNull(results);
        Assert.Empty(results);
    }

    [Fact]
    public async Task SemanticSearch_WithArticleIdFilter_ShouldFilterResults()
    {
        var targetArticleId = Guid.NewGuid();
        var otherArticleId = Guid.NewGuid();

        var targetArticle = new KnowledgeArticle(
            targetArticleId,
            "Target Sensor Protocol",
            Slug.FromTitle("Target Sensor Protocol"),
            "Target description",
            "Target sensor protocol content",
            _factory.DefaultKnowledgeTypeId,
            Guid.NewGuid(),
            DateTimeOffset.UtcNow);

        var otherArticle = new KnowledgeArticle(
            otherArticleId,
            "Other Sensor Protocol",
            Slug.FromTitle("Other Sensor Protocol"),
            "Other description",
            "Other sensor protocol content",
            _factory.DefaultKnowledgeTypeId,
            Guid.NewGuid(),
            DateTimeOffset.UtcNow);

        using (var scope = _factory.Services.CreateScope())
        {
            var indexService = scope.ServiceProvider.GetRequiredService<ISemanticIndexService>();
            await indexService.IndexArticleAsync(targetArticle);
            await indexService.IndexArticleAsync(otherArticle);
        }

        var request = new SemanticSearchApiRequest(
            Query: "Sensor Protocol",
            ArticleId: targetArticleId);

        var response = await _client.PostAsJsonAsync("/api/search/semantic", request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var results = await response.Content.ReadFromJsonAsync<List<SemanticSearchResultDto>>();
        Assert.NotNull(results);
        Assert.All(results, r => Assert.Equal(targetArticleId, r.ArticleId));
    }
}
