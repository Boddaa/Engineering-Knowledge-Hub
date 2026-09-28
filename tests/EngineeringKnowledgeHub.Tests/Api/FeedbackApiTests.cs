namespace EngineeringKnowledgeHub.Tests.Api;

using System.Net;
using System.Net.Http.Json;
using EngineeringKnowledgeHub.Api.Models;
using EngineeringKnowledgeHub.Application.DTOs;
using EngineeringKnowledgeHub.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Xunit;

public class FeedbackApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public FeedbackApiTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<KnowledgeArticleDto> CreateAndPublishArticleAsync()
    {
        var authorId = Guid.NewGuid();
        var reviewerId = Guid.NewGuid();

        var createResponse = await _client.PostAsJsonAsync("/api/knowledge/articles", new CreateArticleRequest(
            Title: $"Feedback Target Article {Guid.NewGuid():N}",
            Summary: "Article to receive helpfulness feedback",
            Content: "Step-by-step instructions for operating services.",
            KnowledgeTypeId: _factory.DefaultKnowledgeTypeId,
            AuthorId: authorId));
        var article = await createResponse.Content.ReadFromJsonAsync<KnowledgeArticleDto>();
        Assert.NotNull(article);

        await _client.PostAsJsonAsync($"/api/knowledge/articles/{article.Id}/submit-review", new SubmitArticleReviewRequest(authorId));
        var approveResponse = await _client.PostAsJsonAsync($"/api/knowledge/articles/{article.Id}/approve", new ApproveArticleRequest(reviewerId, "Approved for feedback test"));
        var published = await approveResponse.Content.ReadFromJsonAsync<KnowledgeArticleDto>();
        Assert.NotNull(published);
        Assert.Equal(KnowledgeStatus.Published, published.Status);

        return published;
    }

    [Fact]
    public async Task SubmitFeedback_ForPublishedArticle_ShouldPersist_AndReturn200OK()
    {
        var article = await CreateAndPublishArticleAsync();
        var userId = Guid.NewGuid();

        var request = new SubmitFeedbackRequest(
            ArticleId: article.Id,
            UserId: userId,
            FeedbackType: FeedbackType.Helpful,
            Comment: "Clear and straightforward explanation!");

        var response = await _client.PostAsJsonAsync("/api/feedback", request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var feedback = await response.Content.ReadFromJsonAsync<KnowledgeFeedbackDto>();
        Assert.NotNull(feedback);
        Assert.NotEqual(Guid.Empty, feedback.Id);
        Assert.Equal(article.Id, feedback.ArticleId);
        Assert.Equal(userId, feedback.UserId);
        Assert.Equal(FeedbackType.Helpful, feedback.FeedbackType);
        Assert.Equal("Clear and straightforward explanation!", feedback.Comment);

        // Verify retrieval via GET /api/feedback/article/{articleId}
        var getResponse = await _client.GetAsync($"/api/feedback/article/{article.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var feedbackList = await getResponse.Content.ReadFromJsonAsync<List<KnowledgeFeedbackDto>>();
        Assert.NotNull(feedbackList);
        Assert.Contains(feedbackList, f => f.Id == feedback.Id && f.Comment == "Clear and straightforward explanation!");
    }

    [Fact]
    public async Task SubmitFeedback_ViaArticleRoute_ShouldSucceed()
    {
        var article = await CreateAndPublishArticleAsync();
        var userId = Guid.NewGuid();

        var request = new SubmitFeedbackRequest(
            ArticleId: Guid.Empty, // Will be bound from route
            UserId: userId,
            FeedbackType: FeedbackType.Inaccurate,
            Comment: "Verified against production setup.");

        var response = await _client.PostAsJsonAsync($"/api/knowledge/articles/{article.Id}/feedback", request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var feedback = await response.Content.ReadFromJsonAsync<KnowledgeFeedbackDto>();
        Assert.NotNull(feedback);
        Assert.Equal(article.Id, feedback.ArticleId);

        var getResponse = await _client.GetAsync($"/api/knowledge/articles/{article.Id}/feedback");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var feedbackList = await getResponse.Content.ReadFromJsonAsync<List<KnowledgeFeedbackDto>>();
        Assert.NotNull(feedbackList);
        Assert.Contains(feedbackList, f => f.Id == feedback.Id);
    }

    [Fact]
    public async Task SubmitFeedback_WhenArticleNotPublished_ShouldReturn400BadRequest()
    {
        // Create draft article without publishing
        var createResponse = await _client.PostAsJsonAsync("/api/knowledge/articles", new CreateArticleRequest(
            Title: "Draft Article Feedback Rejected",
            Summary: "Draft summary",
            Content: "Draft content",
            KnowledgeTypeId: _factory.DefaultKnowledgeTypeId,
            AuthorId: Guid.NewGuid()));
        var draft = await createResponse.Content.ReadFromJsonAsync<KnowledgeArticleDto>();
        Assert.NotNull(draft);
        Assert.Equal(KnowledgeStatus.Draft, draft.Status);

        var request = new SubmitFeedbackRequest(
            ArticleId: draft.Id,
            UserId: Guid.NewGuid(),
            FeedbackType: FeedbackType.Helpful);

        var response = await _client.PostAsJsonAsync("/api/feedback", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
    }

    [Fact]
    public async Task SubmitFeedback_WhenArticleNotFound_ShouldReturn404NotFound()
    {
        var request = new SubmitFeedbackRequest(
            ArticleId: Guid.NewGuid(),
            UserId: Guid.NewGuid(),
            FeedbackType: FeedbackType.Helpful);

        var response = await _client.PostAsJsonAsync("/api/feedback", request);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(404, problem.Status);
    }

    [Fact]
    public async Task SubmitFeedback_WithEmptyUserId_ShouldReturn400BadRequest()
    {
        var request = new SubmitFeedbackRequest(
            ArticleId: Guid.NewGuid(),
            UserId: Guid.Empty,
            FeedbackType: FeedbackType.Helpful);

        var response = await _client.PostAsJsonAsync("/api/feedback", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
        Assert.Contains("UserId cannot be empty", problem.Detail);
    }

    [Fact]
    public async Task GetFeedback_WithEmptyArticleId_ShouldReturn400BadRequest()
    {
        var response = await _client.GetAsync($"/api/feedback/article/{Guid.Empty}");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
        Assert.Contains("Article ID cannot be empty", problem.Detail);
    }
}
