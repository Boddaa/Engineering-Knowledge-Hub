namespace EngineeringKnowledgeHub.Tests.Api;

using System.Net;
using System.Net.Http.Json;
using EngineeringKnowledgeHub.Api.Models;
using EngineeringKnowledgeHub.Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using Xunit;

public class TechnologyApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public TechnologyApiTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateTechnology_And_GetTechnology_ShouldPersist_AndReturn201Created()
    {
        var request = new CreateTechnologyRequest(
            Name: $"Integration Test Tech {Guid.NewGuid():N}",
            Description: "Distributed stream-processing framework");

        // 1. Create via POST /api/technologies
        var response = await _client.PostAsJsonAsync("/api/technologies", request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var created = await response.Content.ReadFromJsonAsync<TechnologyDto>();
        Assert.NotNull(created);
        Assert.NotEqual(Guid.Empty, created.Id);
        Assert.Equal(request.Name, created.Name);
        Assert.Equal(request.Description, created.Description);

        // 2. Query via GET /api/technologies/{id}
        var getResponse = await _client.GetAsync($"/api/technologies/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var fetched = await getResponse.Content.ReadFromJsonAsync<TechnologyDto>();
        Assert.NotNull(fetched);
        Assert.Equal(created.Id, fetched.Id);
        Assert.Equal(created.Name, fetched.Name);
        Assert.Equal(created.Description, fetched.Description);
    }

    [Fact]
    public async Task UpdateTechnology_ShouldUpdateMetadata_AndReturn200Ok()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/technologies", new CreateTechnologyRequest(
            Name: $"Initial Tech {Guid.NewGuid():N}",
            Description: "Initial description"));
        var created = await createResponse.Content.ReadFromJsonAsync<TechnologyDto>();
        Assert.NotNull(created);

        var updateRequest = new UpdateTechnologyRequest(
            Name: "Updated Tech Name",
            Description: "Updated technology description");

        var updateResponse = await _client.PutAsJsonAsync($"/api/technologies/{created.Id}", updateRequest);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var updated = await updateResponse.Content.ReadFromJsonAsync<TechnologyDto>();
        Assert.NotNull(updated);
        Assert.Equal(updateRequest.Name, updated.Name);
        Assert.Equal(updateRequest.Description, updated.Description);
    }

    [Fact]
    public async Task GetTechnology_WhenNotFound_ShouldReturn404NotFound()
    {
        var response = await _client.GetAsync($"/api/technologies/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(404, problem.Status);
    }

    [Fact]
    public async Task UpdateTechnology_WhenNotFound_ShouldReturn404NotFound()
    {
        var updateRequest = new UpdateTechnologyRequest(
            Name: "Ghost Tech",
            Description: "Does not exist");

        var response = await _client.PutAsJsonAsync($"/api/technologies/{Guid.NewGuid()}", updateRequest);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(404, problem.Status);
    }

    [Theory]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\r\n")]
    public async Task CreateTechnology_WithWhitespaceName_ShouldReturn400BadRequest(string whitespace)
    {
        var request = new
        {
            Name = whitespace,
            Description = "Whitespace name test"
        };

        var response = await _client.PostAsJsonAsync("/api/technologies", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
    }

    [Theory]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\r\n")]
    public async Task UpdateTechnology_WithWhitespaceName_ShouldReturn400BadRequest(string whitespace)
    {
        var createResponse = await _client.PostAsJsonAsync("/api/technologies", new CreateTechnologyRequest(
            Name: $"Tech For Whitespace Update {Guid.NewGuid():N}"));
        var created = await createResponse.Content.ReadFromJsonAsync<TechnologyDto>();
        Assert.NotNull(created);

        var invalidRequest = new
        {
            Name = whitespace
        };

        var response = await _client.PutAsJsonAsync($"/api/technologies/{created.Id}", invalidRequest);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
    }
}
