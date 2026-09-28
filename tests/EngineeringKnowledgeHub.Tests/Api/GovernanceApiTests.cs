namespace EngineeringKnowledgeHub.Tests.Api;

using System.Net;
using System.Net.Http.Json;
using EngineeringKnowledgeHub.Api.Models;
using EngineeringKnowledgeHub.Application.DTOs;
using EngineeringKnowledgeHub.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Xunit;

public class GovernanceApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public GovernanceApiTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PublicationFlow_ShouldAtomicallyPublishArticle_AndGenerateKnowledgeVersion_AndAuditTrail()
    {
        var authorId = Guid.NewGuid();
        var reviewerId = Guid.NewGuid();

        // 1. Create Article via API
        var createRequest = new CreateArticleRequest(
            Title: "Governance Atomicity Integration Test",
            Summary: "Testing HTTP publication transaction",
            Content: "Step 1: Write code. Step 2: Test thoroughly.",
            KnowledgeTypeId: _factory.DefaultKnowledgeTypeId,
            AuthorId: authorId);

        var createResponse = await _client.PostAsJsonAsync("/api/knowledge/articles", createRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<KnowledgeArticleDto>();
        Assert.NotNull(created);
        Assert.Equal(KnowledgeStatus.Draft, created.Status);

        // 2. Submit for review via API
        var submitResponse = await _client.PostAsJsonAsync(
            $"/api/knowledge/articles/{created.Id}/submit-review",
            new SubmitArticleReviewRequest(authorId));
        Assert.Equal(HttpStatusCode.OK, submitResponse.StatusCode);

        // 3. Approve via API (executes through mandatory IUnitOfWork transaction!)
        var approveResponse = await _client.PostAsJsonAsync(
            $"/api/knowledge/articles/{created.Id}/approve",
            new ApproveArticleRequest(reviewerId, "Approved for production"));
        Assert.Equal(HttpStatusCode.OK, approveResponse.StatusCode);

        var published = await approveResponse.Content.ReadFromJsonAsync<KnowledgeArticleDto>();
        Assert.NotNull(published);
        Assert.Equal(KnowledgeStatus.Published, published.Status);
        Assert.Equal(VerificationStatus.Verified, published.VerificationStatus);

        // 4. Verify historical KnowledgeVersion snapshot via API
        var versionsResponse = await _client.GetAsync($"/api/knowledge/articles/{created.Id}/versions");
        Assert.Equal(HttpStatusCode.OK, versionsResponse.StatusCode);

        var versions = await versionsResponse.Content.ReadFromJsonAsync<List<KnowledgeVersionDto>>();
        Assert.NotNull(versions);
        Assert.Single(versions);

        var v1 = versions[0];
        Assert.Equal(created.Id, v1.ArticleId);
        Assert.Equal(1, v1.VersionNumber);
        Assert.Equal("Governance Atomicity Integration Test", v1.Title);
        Assert.Equal("Approved for production", v1.ChangeSummary);
        Assert.Equal(reviewerId, v1.ChangedById);

        // 5. Verify lookup of version by number
        var versionByNumberResponse = await _client.GetAsync($"/api/knowledge/articles/{created.Id}/versions/1");
        Assert.Equal(HttpStatusCode.OK, versionByNumberResponse.StatusCode);
        var fetchedByNumber = await versionByNumberResponse.Content.ReadFromJsonAsync<KnowledgeVersionDto>();
        Assert.NotNull(fetchedByNumber);
        Assert.Equal(v1.Id, fetchedByNumber.Id);

        // 6. Verify lookup of version by ID
        var versionByIdResponse = await _client.GetAsync($"/api/knowledge/versions/{v1.Id}");
        Assert.Equal(HttpStatusCode.OK, versionByIdResponse.StatusCode);
        var fetchedById = await versionByIdResponse.Content.ReadFromJsonAsync<KnowledgeVersionDto>();
        Assert.NotNull(fetchedById);
        Assert.Equal(1, fetchedById.VersionNumber);

        // 7. Verify article audit trail via API
        var articleAuditResponse = await _client.GetAsync($"/api/knowledge/articles/{created.Id}/audit");
        Assert.Equal(HttpStatusCode.OK, articleAuditResponse.StatusCode);
        var articleAudits = await articleAuditResponse.Content.ReadFromJsonAsync<List<AuditLogDto>>();
        Assert.NotNull(articleAudits);
        Assert.Contains(articleAudits, a => a.Action == "ArticleSubmittedForReview");
        Assert.Contains(articleAudits, a => a.Action == "ArticleApproved");

        // 8. Verify entity audit trail via general audit endpoint
        var entityAuditResponse = await _client.GetAsync($"/api/audit/entities/KnowledgeArticle/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, entityAuditResponse.StatusCode);
        var entityAudits = await entityAuditResponse.Content.ReadFromJsonAsync<List<AuditLogDto>>();
        Assert.NotNull(entityAudits);
        Assert.True(entityAudits.Count >= 2);

        // 9. Verify user audit trail via general audit endpoint
        var userAuditResponse = await _client.GetAsync($"/api/audit/users/{reviewerId}");
        Assert.Equal(HttpStatusCode.OK, userAuditResponse.StatusCode);
        var userAudits = await userAuditResponse.Content.ReadFromJsonAsync<List<AuditLogDto>>();
        Assert.NotNull(userAudits);
        Assert.Contains(userAudits, a => a.Action == "ArticleApproved");

        // 10. Verify global audit logs endpoint
        var allAuditsResponse = await _client.GetAsync("/api/audit");
        Assert.Equal(HttpStatusCode.OK, allAuditsResponse.StatusCode);
        var allAudits = await allAuditsResponse.Content.ReadFromJsonAsync<List<AuditLogDto>>();
        Assert.NotNull(allAudits);
        Assert.NotEmpty(allAudits);
    }

    [Fact]
    public async Task VersionEndpoints_WithInvalidOrUnknownIdentifiers_ShouldReturnAppropriateErrors()
    {
        var unknownArticleId = Guid.NewGuid();
        var unknownVersionId = Guid.NewGuid();

        // 1. Unknown article versions -> 404
        var response1 = await _client.GetAsync($"/api/knowledge/articles/{unknownArticleId}/versions");
        Assert.Equal(HttpStatusCode.NotFound, response1.StatusCode);

        // 2. Unknown version ID -> 404
        var response2 = await _client.GetAsync($"/api/knowledge/versions/{unknownVersionId}");
        Assert.Equal(HttpStatusCode.NotFound, response2.StatusCode);

        // 3. Invalid version number (<= 0) -> 400 Bad Request
        var response3 = await _client.GetAsync($"/api/knowledge/articles/{unknownArticleId}/versions/0");
        Assert.Equal(HttpStatusCode.BadRequest, response3.StatusCode);

        // 4. Negative version number -> 400 Bad Request
        var response4 = await _client.GetAsync($"/api/knowledge/articles/{unknownArticleId}/versions/-1");
        Assert.Equal(HttpStatusCode.BadRequest, response4.StatusCode);
    }

    [Fact]
    public async Task AuditEndpoints_WithInvalidParameters_ShouldReturn400BadRequest()
    {
        // 1. Empty GUID for entity -> 400
        var response1 = await _client.GetAsync($"/api/audit/entities/KnowledgeArticle/{Guid.Empty}");
        Assert.Equal(HttpStatusCode.BadRequest, response1.StatusCode);

        // 2. Empty GUID for user -> 400
        var response2 = await _client.GetAsync($"/api/audit/users/{Guid.Empty}");
        Assert.Equal(HttpStatusCode.BadRequest, response2.StatusCode);
    }
}
