namespace EngineeringKnowledgeHub.Tests.Api;

using System.Net;
using System.Net.Http.Json;
using EngineeringKnowledgeHub.Api.Models;
using EngineeringKnowledgeHub.Application.DTOs;
using EngineeringKnowledgeHub.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Xunit;

public class KnowledgeSourceApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public KnowledgeSourceApiTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateSource_And_GetSource_ShouldPersist_AndReturn201Created()
    {
        var request = new CreateKnowledgeSourceRequest(
            Name: $"Integration Test Source {Guid.NewGuid():N}",
            SourceType: SourceType.Internal,
            Url: "https://docs.engineering.internal",
            Description: "Internal engineering architecture and API documentation",
            TrustLevel: TrustLevel.Verified);

        // 1. Create via POST /api/sources
        var response = await _client.PostAsJsonAsync("/api/sources", request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var created = await response.Content.ReadFromJsonAsync<KnowledgeSourceDto>();
        Assert.NotNull(created);
        Assert.NotEqual(Guid.Empty, created.Id);
        Assert.Equal(request.Name, created.Name);
        Assert.Equal(request.SourceType, created.SourceType);
        Assert.Equal(request.Url, created.Url);
        Assert.Equal(TrustLevel.Verified, created.TrustLevel);
        Assert.False(created.IsApproved);

        // 2. Query via GET /api/sources/{id}
        var getResponse = await _client.GetAsync($"/api/sources/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var fetched = await getResponse.Content.ReadFromJsonAsync<KnowledgeSourceDto>();
        Assert.NotNull(fetched);
        Assert.Equal(created.Id, fetched.Id);
        Assert.Equal(created.Name, fetched.Name);
    }

    [Fact]
    public async Task GetSources_WithFiltering_ShouldReturnMatchingSources()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/sources", new CreateKnowledgeSourceRequest(
            Name: $"Vendor Source {Guid.NewGuid():N}",
            SourceType: SourceType.VendorDocumentation,
            Url: "https://vendor.docs.com",
            TrustLevel: TrustLevel.Untrusted));
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var response = await _client.GetAsync("/api/sources?sourceType=VendorDocumentation");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var sources = await response.Content.ReadFromJsonAsync<List<KnowledgeSourceDto>>();
        Assert.NotNull(sources);
        Assert.NotEmpty(sources);
        Assert.All(sources, s => Assert.Equal(SourceType.VendorDocumentation, s.SourceType));
    }

    [Fact]
    public async Task UpdateSource_ShouldUpdateMetadata()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/sources", new CreateKnowledgeSourceRequest(
            Name: $"Source To Update {Guid.NewGuid():N}",
            SourceType: SourceType.Internal,
            Description: "Initial description"));
        var source = await createResponse.Content.ReadFromJsonAsync<KnowledgeSourceDto>();
        Assert.NotNull(source);

        var updateRequest = new UpdateKnowledgeSourceRequest(
            Name: $"Source Updated {Guid.NewGuid():N}",
            Url: "https://updated.docs.com",
            Description: "Updated description");

        var updateResponse = await _client.PutAsJsonAsync($"/api/sources/{source.Id}", updateRequest);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var updated = await updateResponse.Content.ReadFromJsonAsync<KnowledgeSourceDto>();
        Assert.NotNull(updated);
        Assert.Equal(updateRequest.Name, updated.Name);
        Assert.Equal(updateRequest.Url, updated.Url);
        Assert.Equal(updateRequest.Description, updated.Description);
    }

    [Fact]
    public async Task SourceApproval_Approve_And_Revoke_ShouldTransitionApprovalStatus()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/sources", new CreateKnowledgeSourceRequest(
            Name: $"Approval Lifecycle Source {Guid.NewGuid():N}",
            SourceType: SourceType.External));
        var source = await createResponse.Content.ReadFromJsonAsync<KnowledgeSourceDto>();
        Assert.NotNull(source);
        Assert.False(source.IsApproved);

        // 1. Approve
        var approveResponse = await _client.PostAsync($"/api/sources/{source.Id}/approve", null);
        Assert.Equal(HttpStatusCode.OK, approveResponse.StatusCode);
        var approved = await approveResponse.Content.ReadFromJsonAsync<KnowledgeSourceDto>();
        Assert.NotNull(approved);
        Assert.True(approved.IsApproved);

        // 2. Revoke Approval
        var revokeResponse = await _client.PostAsync($"/api/sources/{source.Id}/revoke-approval", null);
        Assert.Equal(HttpStatusCode.OK, revokeResponse.StatusCode);
        var revoked = await revokeResponse.Content.ReadFromJsonAsync<KnowledgeSourceDto>();
        Assert.NotNull(revoked);
        Assert.False(revoked.IsApproved);
    }

    [Fact]
    public async Task UpdateTrustLevel_ShouldUpdateTrust()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/sources", new CreateKnowledgeSourceRequest(
            Name: $"Trust Level Source {Guid.NewGuid():N}",
            SourceType: SourceType.Internal,
            TrustLevel: TrustLevel.Untrusted));
        var source = await createResponse.Content.ReadFromJsonAsync<KnowledgeSourceDto>();
        Assert.NotNull(source);
        Assert.Equal(TrustLevel.Untrusted, source.TrustLevel);

        var updateResponse = await _client.PutAsJsonAsync($"/api/sources/{source.Id}/trust-level", new UpdateTrustLevelRequest(TrustLevel.Verified));
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var updated = await updateResponse.Content.ReadFromJsonAsync<KnowledgeSourceDto>();
        Assert.NotNull(updated);
        Assert.Equal(TrustLevel.Verified, updated.TrustLevel);
    }

    [Fact]
    public async Task ArticleSourceAssociation_AttachAndDetach_ShouldUpdateArticle()
    {
        // 1. Create Article
        var articleResponse = await _client.PostAsJsonAsync("/api/knowledge/articles", new CreateArticleRequest(
            Title: $"Source Association Article {Guid.NewGuid():N}",
            Summary: "Article for testing source attachment",
            Content: "Content referencing external standard.",
            KnowledgeTypeId: _factory.DefaultKnowledgeTypeId,
            AuthorId: Guid.NewGuid()));
        var article = await articleResponse.Content.ReadFromJsonAsync<KnowledgeArticleDto>();
        Assert.NotNull(article);

        // 2. Create Source
        var sourceResponse = await _client.PostAsJsonAsync("/api/sources", new CreateKnowledgeSourceRequest(
            Name: $"RFC Standard {Guid.NewGuid():N}",
            SourceType: SourceType.External,
            Url: "https://rfc-editor.org/rfc/rfc7807"));
        var source = await sourceResponse.Content.ReadFromJsonAsync<KnowledgeSourceDto>();
        Assert.NotNull(source);

        // 3. Attach Source
        var attachResponse = await _client.PostAsJsonAsync(
            $"/api/sources/{source.Id}/articles/{article.Id}",
            new AttachSourceRequest("Section 3: Problem Details Format"));
        Assert.Equal(HttpStatusCode.OK, attachResponse.StatusCode);

        var updatedArticle = await attachResponse.Content.ReadFromJsonAsync<KnowledgeArticleDto>();
        Assert.NotNull(updatedArticle);
        Assert.Contains(source.Id, updatedArticle.SourceIds);

        // 4. Query Sources by Article
        var getSourcesResponse = await _client.GetAsync($"/api/sources/article/{article.Id}");
        Assert.Equal(HttpStatusCode.OK, getSourcesResponse.StatusCode);
        var sourcesForArticle = await getSourcesResponse.Content.ReadFromJsonAsync<List<KnowledgeSourceDto>>();
        Assert.NotNull(sourcesForArticle);
        Assert.Contains(sourcesForArticle, s => s.Id == source.Id);

        // 5. Detach Source
        var detachResponse = await _client.DeleteAsync($"/api/sources/{source.Id}/articles/{article.Id}");
        Assert.Equal(HttpStatusCode.OK, detachResponse.StatusCode);

        var detachedArticle = await detachResponse.Content.ReadFromJsonAsync<KnowledgeArticleDto>();
        Assert.NotNull(detachedArticle);
        Assert.DoesNotContain(source.Id, detachedArticle.SourceIds);
    }

    [Fact]
    public async Task GetSource_WhenNotFound_ShouldReturn404NotFound()
    {
        var response = await _client.GetAsync($"/api/sources/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(404, problem.Status);
    }

    [Fact]
    public async Task CreateSource_WithDuplicateName_ShouldReturn409Conflict()
    {
        var name = $"Unique Source Name {Guid.NewGuid():N}";
        var request = new CreateKnowledgeSourceRequest(name, SourceType.Internal);

        var response1 = await _client.PostAsJsonAsync("/api/sources", request);
        Assert.Equal(HttpStatusCode.Created, response1.StatusCode);

        var response2 = await _client.PostAsJsonAsync("/api/sources", request);
        Assert.Equal(HttpStatusCode.Conflict, response2.StatusCode);

        var problem = await response2.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(409, problem.Status);
    }

    [Fact]
    public async Task UpdateSource_WhenNotFound_ShouldReturn404NotFound()
    {
        var request = new UpdateKnowledgeSourceRequest("Updated Name");
        var response = await _client.PutAsJsonAsync($"/api/sources/{Guid.NewGuid()}", request);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(404, problem.Status);
    }

    [Theory]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\r\n")]
    public async Task CreateSource_WithWhitespaceName_ShouldReturn400BadRequest(string whitespace)
    {
        var invalidRequest = new
        {
            Name = whitespace,
            SourceType = SourceType.Internal
        };

        var response = await _client.PostAsJsonAsync("/api/sources", invalidRequest);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
    }

    [Theory]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\r\n")]
    public async Task UpdateSource_WithWhitespaceName_ShouldReturn400BadRequest(string whitespace)
    {
        var createResponse = await _client.PostAsJsonAsync("/api/sources", new CreateKnowledgeSourceRequest(
            Name: $"Source For Whitespace Update {Guid.NewGuid():N}",
            SourceType: SourceType.Internal));
        var source = await createResponse.Content.ReadFromJsonAsync<KnowledgeSourceDto>();
        Assert.NotNull(source);

        var invalidRequest = new
        {
            Name = whitespace
        };

        var response = await _client.PutAsJsonAsync($"/api/sources/{source.Id}", invalidRequest);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
    }
}
