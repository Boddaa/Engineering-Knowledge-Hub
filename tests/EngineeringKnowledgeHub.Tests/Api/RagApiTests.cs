namespace EngineeringKnowledgeHub.Tests.Api;

using System.Net;
using System.Net.Http.Json;
using EngineeringKnowledgeHub.Api.Models;
using EngineeringKnowledgeHub.Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using Xunit;

public class RagApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public RagApiTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task RagQuery_WithValidQuestion_ShouldReturn200Ok_AndRagResponseDto()
    {
        var request = new RagQueryRequest(
            Question: "How do we handle database deadlocks in payment processing?",
            MaxEvidence: 5,
            MinRelevanceScore: 0.1,
            PreferVerified: true);

        var response = await _client.PostAsJsonAsync("/api/rag/query", request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var ragResponse = await response.Content.ReadFromJsonAsync<RagResponseDto>();
        Assert.NotNull(ragResponse);
        Assert.Equal(request.Question, ragResponse.Question);
        Assert.NotNull(ragResponse.Answer);
        Assert.NotEmpty(ragResponse.Answer);
        Assert.NotNull(ragResponse.ModelUsed);
        Assert.NotEqual(default, ragResponse.AnsweredAt);
        Assert.NotNull(ragResponse.Evidence);
    }

    [Fact]
    public async Task RagAsk_AlternativeRoute_ShouldReturn200Ok()
    {
        var request = new RagQueryRequest(
            Question: "What is the recommended retry pattern for external APIs?");

        var response = await _client.PostAsJsonAsync("/api/rag/ask", request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var ragResponse = await response.Content.ReadFromJsonAsync<RagResponseDto>();
        Assert.NotNull(ragResponse);
        Assert.Equal(request.Question, ragResponse.Question);
        Assert.NotNull(ragResponse.Answer);
    }

    [Fact]
    public async Task RagQuery_WithEvidenceSources_ShouldPropagateCitations()
    {
        var authorId = Guid.NewGuid();
        var reviewerId = Guid.NewGuid();
        var uniqueTopic = $"DistributedLocking_{Guid.NewGuid():N}";

        // 1. Create and publish an article with known content
        var createResponse = await _client.PostAsJsonAsync("/api/knowledge/articles", new CreateArticleRequest(
            Title: $"Guide to {uniqueTopic}",
            Summary: "Authoritative locking guidance",
            Content: $"Always acquire locks in lexicographical order: orders then payments. {uniqueTopic} prevents deadlocks.",
            KnowledgeTypeId: _factory.DefaultKnowledgeTypeId,
            AuthorId: authorId));
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var article = await createResponse.Content.ReadFromJsonAsync<KnowledgeArticleDto>();
        Assert.NotNull(article);

        await _client.PostAsJsonAsync($"/api/knowledge/governance/articles/{article.Id}/submit-for-review", new SubmitArticleReviewRequest(authorId));
        await _client.PostAsJsonAsync($"/api/knowledge/governance/articles/{article.Id}/approve", new ApproveArticleRequest(reviewerId, "LGTM"));

        // 2. Query RAG for this specific topic
        var request = new RagQueryRequest(
            Question: $"What is the recommended practice for {uniqueTopic}?",
            MinRelevanceScore: 0.05);

        var response = await _client.PostAsJsonAsync("/api/rag/query", request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var ragResponse = await response.Content.ReadFromJsonAsync<RagResponseDto>();
        Assert.NotNull(ragResponse);
        Assert.True(ragResponse.IsSufficientEvidence);
        Assert.NotEmpty(ragResponse.Evidence);
        Assert.Contains(ragResponse.Evidence, e => e.Title.Contains(uniqueTopic));
    }

    [Theory]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\r\n")]
    public async Task RagQuery_WithWhitespaceQuestion_ShouldReturn400BadRequest(string whitespace)
    {
        var request = new
        {
            Question = whitespace
        };

        var response = await _client.PostAsJsonAsync("/api/rag/query", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
    }

    [Fact]
    public async Task RagQuery_WithEmptyGuidFilter_ShouldReturn400BadRequest()
    {
        var request = new
        {
            Question = "Valid engineering question",
            ProductId = Guid.Empty
        };

        var response = await _client.PostAsJsonAsync("/api/rag/query", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
    }

    [Fact]
    public async Task RagQuery_WithOutOfRangeMaxEvidence_ShouldReturn400BadRequest()
    {
        var request = new
        {
            Question = "Valid engineering question",
            MaxEvidence = 100 // Exceeds max 50
        };

        var response = await _client.PostAsJsonAsync("/api/rag/query", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
    }
}
