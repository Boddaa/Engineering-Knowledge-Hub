namespace EngineeringKnowledgeHub.Tests.Api;

using System.Net;
using System.Net.Http.Json;
using EngineeringKnowledgeHub.Api.Models;
using EngineeringKnowledgeHub.Application.DTOs;
using EngineeringKnowledgeHub.Domain.Enums;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Xunit;

public class SourcesControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public SourcesControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ImportFromSource_WithEmptyId_ShouldReturnBadRequest()
    {
        var response = await _client.PostAsync($"/api/sources/{Guid.Empty}/import", null);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var details = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(details);
        Assert.Equal(400, details.Status);
        Assert.Contains("Source ID cannot be empty", details.Detail);
    }

    [Fact]
    public async Task ImportFromSource_WhenSourceNotFound_ShouldReturnNotFound_ViaGlobalExceptionHandler()
    {
        var missingId = Guid.NewGuid();
        var response = await _client.PostAsync($"/api/sources/{missingId}/import", null);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var details = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(details);
        Assert.Equal(404, details.Status);
        Assert.Equal("Not Found", details.Title);
        Assert.Equal("https://tools.ietf.org/html/rfc7231#section-6.5.4", details.Type);
        Assert.Equal($"/api/sources/{missingId}/import", details.Instance);
    }

    [Fact]
    public async Task ImportFromSource_WhenSourceNotApproved_ShouldReturnBadRequest_ViaGlobalExceptionHandler()
    {
        // 1. Create a source (defaults to unapproved)
        var createResponse = await _client.PostAsJsonAsync("/api/sources", new CreateKnowledgeSourceRequest(
            Name: $"Unapproved External Source {Guid.NewGuid():N}",
            SourceType: SourceType.External,
            Url: "https://docs.external.example.com",
            Description: "Unapproved external docs"));
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var source = await createResponse.Content.ReadFromJsonAsync<KnowledgeSourceDto>();
        Assert.NotNull(source);
        Assert.False(source.IsApproved);

        // 2. Attempt import — should trigger Application ValidationException via GlobalExceptionHandler
        var response = await _client.PostAsync($"/api/sources/{source.Id}/import", null);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var details = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(details);
        Assert.Equal(400, details.Status);
        Assert.Equal("Bad Request", details.Title);
        Assert.Equal("https://tools.ietf.org/html/rfc7231#section-6.5.1", details.Type);
        Assert.Contains("is not approved", details.Detail);
    }

    [Fact]
    public async Task ImportFromSource_WhenSourceTypeIneligible_ShouldReturnBadRequest_ViaGlobalExceptionHandler()
    {
        // 1. Create an internal source
        var createResponse = await _client.PostAsJsonAsync("/api/sources", new CreateKnowledgeSourceRequest(
            Name: $"Internal Source {Guid.NewGuid():N}",
            SourceType: SourceType.Internal,
            Url: "https://internal.example.com",
            Description: "Internal docs"));
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var source = await createResponse.Content.ReadFromJsonAsync<KnowledgeSourceDto>();
        Assert.NotNull(source);

        // 2. Approve it
        var approveResponse = await _client.PostAsync($"/api/sources/{source.Id}/approve", null);
        Assert.Equal(HttpStatusCode.OK, approveResponse.StatusCode);

        // 3. Attempt import — should fail validation because Internal sources are ineligible for external import
        var response = await _client.PostAsync($"/api/sources/{source.Id}/import", null);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var details = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(details);
        Assert.Equal(400, details.Status);
        Assert.Equal("Bad Request", details.Title);
        Assert.Equal("https://tools.ietf.org/html/rfc7231#section-6.5.1", details.Type);
        Assert.Contains("Only External and VendorDocumentation sources are eligible", details.Detail);
    }

    [Fact]
    public async Task ImportFromSource_InProductionEnvironment_ShouldUseGlobalExceptionHandler_WithoutLeakingInternalDetails()
    {
        using var prodFactory = _factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Production");
        });
        var prodClient = prodFactory.CreateClient();

        var missingId = Guid.NewGuid();
        var response = await prodClient.PostAsync($"/api/sources/{missingId}/import", null);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var details = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(details);
        Assert.Equal(404, details.Status);
        Assert.Equal("Not Found", details.Title);
        Assert.Equal("https://tools.ietf.org/html/rfc7231#section-6.5.4", details.Type);
        Assert.DoesNotContain("at ", details.Detail);
        Assert.DoesNotContain("Server=", details.Detail);
    }
}
