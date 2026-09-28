namespace EngineeringKnowledgeHub.Tests.Api;

using System.Net;
using System.Net.Http.Json;
using EngineeringKnowledgeHub.Api.Models;
using EngineeringKnowledgeHub.Application.DTOs;
using EngineeringKnowledgeHub.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Xunit;

public class KnowledgeArticleApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public KnowledgeArticleApiTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetArticle_WhenArticleDoesNotExist_ShouldReturn404NotFound()
    {
        var unknownId = Guid.NewGuid();
        var response = await _client.GetAsync($"/api/knowledge/articles/{unknownId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(404, problem.Status);
    }

    [Fact]
    public async Task CreateArticle_And_GetArticle_ShouldPersist_AndReturn200WithMatchingData()
    {
        var authorId = Guid.NewGuid();
        var request = new CreateArticleRequest(
            Title: "Clean Architecture Principles",
            Summary: "A guide to clean architecture in .NET",
            Content: "Domain, Application, Infrastructure, Presentation layers.",
            KnowledgeTypeId: _factory.DefaultKnowledgeTypeId,
            AuthorId: authorId,
            Slug: $"clean-architecture-{Guid.NewGuid():N}");

        // 1. Create Article via POST
        var createResponse = await _client.PostAsJsonAsync("/api/knowledge/articles", request);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var createdArticle = await createResponse.Content.ReadFromJsonAsync<KnowledgeArticleDto>();
        Assert.NotNull(createdArticle);
        Assert.NotEqual(Guid.Empty, createdArticle.Id);
        Assert.Equal(request.Title, createdArticle.Title);
        Assert.Equal(request.Summary, createdArticle.Summary);
        Assert.Equal(request.Content, createdArticle.Content);
        Assert.Equal(KnowledgeStatus.Draft, createdArticle.Status);
        Assert.Equal(VerificationStatus.Unverified, createdArticle.VerificationStatus);

        // Verify Location header points to GET endpoint
        Assert.NotNull(createResponse.Headers.Location);

        // 2. Query Article via separate GET
        var getResponse = await _client.GetAsync($"/api/knowledge/articles/{createdArticle.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var fetchedArticle = await getResponse.Content.ReadFromJsonAsync<KnowledgeArticleDto>();
        Assert.NotNull(fetchedArticle);
        Assert.Equal(createdArticle.Id, fetchedArticle.Id);
        Assert.Equal(request.Title, fetchedArticle.Title);
        Assert.Equal(request.Summary, fetchedArticle.Summary);
        Assert.Equal(request.Content, fetchedArticle.Content);
        Assert.Equal(KnowledgeStatus.Draft, fetchedArticle.Status);
    }

    [Fact]
    public async Task CreateArticle_WithDuplicateSlug_ShouldReturn409Conflict()
    {
        var slug = $"unique-slug-{Guid.NewGuid():N}";
        var authorId = Guid.NewGuid();
        var request1 = new CreateArticleRequest(
            Title: "Unique Title 1",
            Summary: "Summary 1",
            Content: "Content 1",
            KnowledgeTypeId: _factory.DefaultKnowledgeTypeId,
            AuthorId: authorId,
            Slug: slug);

        var response1 = await _client.PostAsJsonAsync("/api/knowledge/articles", request1);
        Assert.Equal(HttpStatusCode.Created, response1.StatusCode);

        var request2 = new CreateArticleRequest(
            Title: "Unique Title 2",
            Summary: "Summary 2",
            Content: "Content 2",
            KnowledgeTypeId: _factory.DefaultKnowledgeTypeId,
            AuthorId: authorId,
            Slug: slug);

        var response2 = await _client.PostAsJsonAsync("/api/knowledge/articles", request2);
        Assert.Equal(HttpStatusCode.Conflict, response2.StatusCode);

        var problem = await response2.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(409, problem.Status);
    }

    [Fact]
    public async Task UpdateArticle_ShouldUpdateContent_AndPersist()
    {
        // 1. Create
        var createRequest = new CreateArticleRequest(
            Title: "Original Title",
            Summary: "Original Summary",
            Content: "Original Content",
            KnowledgeTypeId: _factory.DefaultKnowledgeTypeId,
            AuthorId: Guid.NewGuid());

        var createResponse = await _client.PostAsJsonAsync("/api/knowledge/articles", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<KnowledgeArticleDto>();
        Assert.NotNull(created);

        // 2. Update via PUT
        var updateRequest = new UpdateArticleRequest(
            Title: "Updated Title",
            Summary: "Updated Summary",
            Content: "Updated Content");

        var updateResponse = await _client.PutAsJsonAsync($"/api/knowledge/articles/{created.Id}", updateRequest);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var updated = await updateResponse.Content.ReadFromJsonAsync<KnowledgeArticleDto>();
        Assert.NotNull(updated);
        Assert.Equal("Updated Title", updated.Title);
        Assert.Equal("Updated Summary", updated.Summary);

        // 3. Verify persistence with fresh GET
        var getResponse = await _client.GetAsync($"/api/knowledge/articles/{created.Id}");
        var persisted = await getResponse.Content.ReadFromJsonAsync<KnowledgeArticleDto>();
        Assert.NotNull(persisted);
        Assert.Equal("Updated Title", persisted.Title);
        Assert.Equal("Updated Summary", persisted.Summary);
    }

    [Fact]
    public async Task UpdateArticle_WithUnknownId_ShouldReturn404NotFound()
    {
        var unknownId = Guid.NewGuid();
        var updateRequest = new UpdateArticleRequest("Title", "Summary", "Content");

        var response = await _client.PutAsJsonAsync($"/api/knowledge/articles/{unknownId}", updateRequest);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task MetadataAssociations_AddAndRemoveTagsProductsTechnologies_ShouldSucceed()
    {
        // 1. Create article
        var createRequest = new CreateArticleRequest(
            Title: "Metadata Associations Test",
            Summary: "Testing tag, product, and technology associations",
            Content: "Association content details.",
            KnowledgeTypeId: _factory.DefaultKnowledgeTypeId,
            AuthorId: Guid.NewGuid());

        var createResponse = await _client.PostAsJsonAsync("/api/knowledge/articles", createRequest);
        var article = await createResponse.Content.ReadFromJsonAsync<KnowledgeArticleDto>();
        Assert.NotNull(article);

        // 2. Add Tag
        var addTagResponse = await _client.PostAsync($"/api/knowledge/articles/{article.Id}/tags/{_factory.DefaultTagId}", null);
        Assert.Equal(HttpStatusCode.OK, addTagResponse.StatusCode);
        var taggedArticle = await addTagResponse.Content.ReadFromJsonAsync<KnowledgeArticleDto>();
        Assert.NotNull(taggedArticle);
        Assert.Contains(_factory.DefaultTagId, taggedArticle.TagIds);

        // 3. Remove Tag
        var removeTagResponse = await _client.DeleteAsync($"/api/knowledge/articles/{article.Id}/tags/{_factory.DefaultTagId}");
        Assert.Equal(HttpStatusCode.OK, removeTagResponse.StatusCode);
        var untaggedArticle = await removeTagResponse.Content.ReadFromJsonAsync<KnowledgeArticleDto>();
        Assert.NotNull(untaggedArticle);
        Assert.DoesNotContain(_factory.DefaultTagId, untaggedArticle.TagIds);

        // 4. Add Product
        var addProdResponse = await _client.PostAsync($"/api/knowledge/articles/{article.Id}/products/{_factory.DefaultProductId}", null);
        Assert.Equal(HttpStatusCode.OK, addProdResponse.StatusCode);
        var prodArticle = await addProdResponse.Content.ReadFromJsonAsync<KnowledgeArticleDto>();
        Assert.NotNull(prodArticle);
        Assert.Contains(_factory.DefaultProductId, prodArticle.ProductIds);

        // 5. Remove Product
        var removeProdResponse = await _client.DeleteAsync($"/api/knowledge/articles/{article.Id}/products/{_factory.DefaultProductId}");
        Assert.Equal(HttpStatusCode.OK, removeProdResponse.StatusCode);
        var unprodArticle = await removeProdResponse.Content.ReadFromJsonAsync<KnowledgeArticleDto>();
        Assert.NotNull(unprodArticle);
        Assert.DoesNotContain(_factory.DefaultProductId, unprodArticle.ProductIds);

        // 6. Add Technology
        var addTechResponse = await _client.PostAsync($"/api/knowledge/articles/{article.Id}/technologies/{_factory.DefaultTechnologyId}", null);
        Assert.Equal(HttpStatusCode.OK, addTechResponse.StatusCode);
        var techArticle = await addTechResponse.Content.ReadFromJsonAsync<KnowledgeArticleDto>();
        Assert.NotNull(techArticle);
        Assert.Contains(_factory.DefaultTechnologyId, techArticle.TechnologyIds);

        // 7. Remove Technology
        var removeTechResponse = await _client.DeleteAsync($"/api/knowledge/articles/{article.Id}/technologies/{_factory.DefaultTechnologyId}");
        Assert.Equal(HttpStatusCode.OK, removeTechResponse.StatusCode);
        var untechArticle = await removeTechResponse.Content.ReadFromJsonAsync<KnowledgeArticleDto>();
        Assert.NotNull(untechArticle);
        Assert.DoesNotContain(_factory.DefaultTechnologyId, untechArticle.TechnologyIds);
    }

    [Fact]
    public async Task RejectArticle_ShouldTransitionToDraft_AndMarkRejected()
    {
        var authorId = Guid.NewGuid();
        var reviewerId = Guid.NewGuid();

        // 1. Create
        var createResponse = await _client.PostAsJsonAsync("/api/knowledge/articles", new CreateArticleRequest(
            Title: "Article For Rejection",
            Summary: "Summary",
            Content: "Content",
            KnowledgeTypeId: _factory.DefaultKnowledgeTypeId,
            AuthorId: authorId));
        var article = await createResponse.Content.ReadFromJsonAsync<KnowledgeArticleDto>();
        Assert.NotNull(article);

        // 2. Submit for review
        var submitResponse = await _client.PostAsJsonAsync($"/api/knowledge/articles/{article.Id}/submit-review", new SubmitArticleReviewRequest(authorId));
        Assert.Equal(HttpStatusCode.OK, submitResponse.StatusCode);

        // 3. Reject
        var rejectResponse = await _client.PostAsJsonAsync($"/api/knowledge/articles/{article.Id}/reject", new RejectArticleRequest(reviewerId));
        Assert.Equal(HttpStatusCode.OK, rejectResponse.StatusCode);

        var rejectedArticle = await rejectResponse.Content.ReadFromJsonAsync<KnowledgeArticleDto>();
        Assert.NotNull(rejectedArticle);
        Assert.Equal(KnowledgeStatus.Draft, rejectedArticle.Status);
        Assert.Equal(VerificationStatus.Rejected, rejectedArticle.VerificationStatus);
    }

    [Fact]
    public async Task DeprecateArticle_And_ArchiveArticle_ShouldTransitionStateCorrectly()
    {
        var authorId = Guid.NewGuid();
        var reviewerId = Guid.NewGuid();

        // 1. Create, submit, approve to publish
        var createResponse = await _client.PostAsJsonAsync("/api/knowledge/articles", new CreateArticleRequest(
            Title: "Article For Deprecation And Archive",
            Summary: "Summary",
            Content: "Content",
            KnowledgeTypeId: _factory.DefaultKnowledgeTypeId,
            AuthorId: authorId));
        var article = await createResponse.Content.ReadFromJsonAsync<KnowledgeArticleDto>();
        Assert.NotNull(article);

        await _client.PostAsJsonAsync($"/api/knowledge/articles/{article.Id}/submit-review", new SubmitArticleReviewRequest(authorId));
        await _client.PostAsJsonAsync($"/api/knowledge/articles/{article.Id}/approve", new ApproveArticleRequest(reviewerId, "Approved"));

        // 2. Deprecate
        var deprecateResponse = await _client.PostAsJsonAsync($"/api/knowledge/articles/{article.Id}/deprecate", new DeprecateArticleRequest(reviewerId));
        Assert.Equal(HttpStatusCode.OK, deprecateResponse.StatusCode);
        var deprecatedArticle = await deprecateResponse.Content.ReadFromJsonAsync<KnowledgeArticleDto>();
        Assert.NotNull(deprecatedArticle);
        Assert.Equal(KnowledgeStatus.Deprecated, deprecatedArticle.Status);

        // 3. Archive
        var archiveResponse = await _client.PostAsJsonAsync($"/api/knowledge/articles/{article.Id}/archive", new ArchiveArticleRequest(reviewerId));
        Assert.Equal(HttpStatusCode.OK, archiveResponse.StatusCode);
        var archivedArticle = await archiveResponse.Content.ReadFromJsonAsync<KnowledgeArticleDto>();
        Assert.NotNull(archivedArticle);
        Assert.Equal(KnowledgeStatus.Archived, archivedArticle.Status);
    }

    [Fact]
    public async Task CreateArticle_WithMissingTitle_ShouldReturn400BadRequest()
    {
        var invalidRequest = new
        {
            Title = "",
            Summary = "Summary",
            Content = "Content",
            KnowledgeTypeId = _factory.DefaultKnowledgeTypeId,
            AuthorId = Guid.NewGuid()
        };

        var response = await _client.PostAsJsonAsync("/api/knowledge/articles", invalidRequest);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
    }

    [Fact]
    public async Task CreateArticle_WithEmptyKnowledgeTypeId_ShouldReturn400BadRequest()
    {
        var invalidRequest = new CreateArticleRequest(
            Title: "Valid Title",
            Summary: "Valid Summary",
            Content: "Valid Content",
            KnowledgeTypeId: Guid.Empty,
            AuthorId: Guid.NewGuid());

        var response = await _client.PostAsJsonAsync("/api/knowledge/articles", invalidRequest);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
        Assert.Contains("KnowledgeTypeId cannot be empty", problem.Detail);
    }

    [Fact]
    public async Task GetArticle_WithEmptyGuid_ShouldReturn400BadRequest()
    {
        var response = await _client.GetAsync($"/api/knowledge/articles/{Guid.Empty}");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
        Assert.Contains("Article ID cannot be empty", problem.Detail);
    }
}
