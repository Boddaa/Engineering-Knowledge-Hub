namespace EngineeringKnowledgeHub.Tests.Api;

using System.Net;
using System.Net.Http.Json;
using EngineeringKnowledgeHub.Api.Models;
using EngineeringKnowledgeHub.Application.DTOs;
using EngineeringKnowledgeHub.Application.Search;
using Microsoft.AspNetCore.Mvc;
using Xunit;

public class SearchApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public SearchApiTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task SearchArticles_ViaGet_ShouldReturn200_WithPagedResult()
    {
        var authorId = Guid.NewGuid();
        var reviewerId = Guid.NewGuid();

        // 1. Create and publish an article with unique searchable term
        var uniqueTerm = $"KubernetesIngress_{Guid.NewGuid():N}";
        var createResponse = await _client.PostAsJsonAsync("/api/knowledge/articles", new CreateArticleRequest(
            Title: $"Deploying with {uniqueTerm}",
            Summary: "Configuring ingress controller in production",
            Content: $"Production guide detailing {uniqueTerm} routing and TLS certificates.",
            KnowledgeTypeId: _factory.DefaultKnowledgeTypeId,
            AuthorId: authorId));

        var article = await createResponse.Content.ReadFromJsonAsync<KnowledgeArticleDto>();
        Assert.NotNull(article);

        await _client.PostAsJsonAsync($"/api/knowledge/articles/{article.Id}/submit-review", new SubmitArticleReviewRequest(authorId));
        await _client.PostAsJsonAsync($"/api/knowledge/articles/{article.Id}/approve", new ApproveArticleRequest(reviewerId, "Approved"));

        // 2. Query Search via GET endpoint
        var searchResponse = await _client.GetAsync($"/api/search/articles?query={uniqueTerm}");
        Assert.Equal(HttpStatusCode.OK, searchResponse.StatusCode);

        var result = await searchResponse.Content.ReadFromJsonAsync<PagedResult<SearchArticleResultDto>>();
        Assert.NotNull(result);
        Assert.True(result.TotalCount >= 1);
        Assert.Contains(result.Items, item => item.Id == article.Id);
        Assert.Equal(1, result.Page);
        Assert.True(result.PageSize > 0);
    }

    [Fact]
    public async Task SearchArticles_ViaPost_ShouldReturn200_WithMatchingResults()
    {
        var authorId = Guid.NewGuid();
        var reviewerId = Guid.NewGuid();

        var uniqueTerm = $"PostgresPartitioning_{Guid.NewGuid():N}";
        var createResponse = await _client.PostAsJsonAsync("/api/knowledge/articles", new CreateArticleRequest(
            Title: $"Database Tuning {uniqueTerm}",
            Summary: "Table partitioning strategies",
            Content: $"Best practices for handling large tables with {uniqueTerm}.",
            KnowledgeTypeId: _factory.DefaultKnowledgeTypeId,
            AuthorId: authorId));

        var article = await createResponse.Content.ReadFromJsonAsync<KnowledgeArticleDto>();
        Assert.NotNull(article);

        await _client.PostAsJsonAsync($"/api/knowledge/articles/{article.Id}/submit-review", new SubmitArticleReviewRequest(authorId));
        await _client.PostAsJsonAsync($"/api/knowledge/articles/{article.Id}/approve", new ApproveArticleRequest(reviewerId, "Approved"));

        // 2. Search via POST body
        var request = new SearchArticleRequest
        {
            Query = uniqueTerm,
            Page = 1,
            PageSize = 10
        };

        var searchResponse = await _client.PostAsJsonAsync("/api/search/articles", request);
        Assert.Equal(HttpStatusCode.OK, searchResponse.StatusCode);

        var result = await searchResponse.Content.ReadFromJsonAsync<PagedResult<SearchArticleResultDto>>();
        Assert.NotNull(result);
        Assert.True(result.TotalCount >= 1);
        Assert.Contains(result.Items, item => item.Id == article.Id);
    }

    [Fact]
    public async Task HybridSearchArticles_ViaPost_ShouldReturn200_WithPagedResult()
    {
        var authorId = Guid.NewGuid();
        var reviewerId = Guid.NewGuid();

        var uniqueTerm = $"EventSourcing_{Guid.NewGuid():N}";
        var createResponse = await _client.PostAsJsonAsync("/api/knowledge/articles", new CreateArticleRequest(
            Title: $"Architectural Patterns {uniqueTerm}",
            Summary: "Event sourcing and CQRS overview",
            Content: $"Deep dive into {uniqueTerm} event streams and projectors.",
            KnowledgeTypeId: _factory.DefaultKnowledgeTypeId,
            AuthorId: authorId));

        var article = await createResponse.Content.ReadFromJsonAsync<KnowledgeArticleDto>();
        Assert.NotNull(article);

        await _client.PostAsJsonAsync($"/api/knowledge/articles/{article.Id}/submit-review", new SubmitArticleReviewRequest(authorId));
        await _client.PostAsJsonAsync($"/api/knowledge/articles/{article.Id}/approve", new ApproveArticleRequest(reviewerId, "Approved"));

        // 2. Execute Hybrid Search
        var hybridRequest = new HybridSearchArticleRequest
        {
            Query = uniqueTerm,
            Page = 1,
            PageSize = 5,
            Options = new HybridSearchOptions
            {
                KeywordWeight = 0.5,
                SemanticWeight = 0.5
            }
        };

        var searchResponse = await _client.PostAsJsonAsync("/api/search/articles/hybrid", hybridRequest);
        Assert.Equal(HttpStatusCode.OK, searchResponse.StatusCode);

        var result = await searchResponse.Content.ReadFromJsonAsync<PagedResult<SearchArticleResultDto>>();
        Assert.NotNull(result);
        Assert.True(result.TotalCount >= 1);
        Assert.Contains(result.Items, item => item.Id == article.Id);
    }
}
