namespace EngineeringKnowledgeHub.Tests.Api;

using System.Net;
using System.Net.Http.Json;
using EngineeringKnowledgeHub.Api.Models;
using EngineeringKnowledgeHub.Application.Ai;
using EngineeringKnowledgeHub.Application.Rag;
using Microsoft.AspNetCore.Mvc;
using Xunit;

public class LocalAiApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public LocalAiApiTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetHealth_ShouldReturn200Ok_AndLocalAiHealthDto()
    {
        var response = await _client.GetAsync("/api/ai/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var health = await response.Content.ReadFromJsonAsync<LocalAiHealthDto>();
        Assert.NotNull(health);
        Assert.True(Enum.IsDefined(health.Status));
    }

    [Fact]
    public async Task GetStatus_AlternativeRoute_ShouldReturn200Ok()
    {
        var response = await _client.GetAsync("/api/ai/status");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var health = await response.Content.ReadFromJsonAsync<LocalAiHealthDto>();
        Assert.NotNull(health);
    }

    [Fact]
    public async Task GetLocalHealth_LegacyRoute_ShouldReturn200Ok()
    {
        var response = await _client.GetAsync("/api/ai/local/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var health = await response.Content.ReadFromJsonAsync<LocalAiHealthDto>();
        Assert.NotNull(health);
    }

    [Fact]
    public async Task Generate_WithValidPrompt_ShouldReturn200Ok_AndAiResponse()
    {
        var request = new AiGenerateRequest(
            UserPrompt: "Explain ACID properties in relational databases.",
            SystemPrompt: "You are a senior software architect.",
            Temperature: 0.2,
            MaxTokens: 256);

        var response = await _client.PostAsJsonAsync("/api/ai/generate", request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var aiResponse = await response.Content.ReadFromJsonAsync<AiResponse>();
        Assert.NotNull(aiResponse);
        Assert.NotNull(aiResponse.Content);
        Assert.NotEmpty(aiResponse.Content);
        Assert.NotNull(aiResponse.ModelName);
    }

    [Theory]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\r\n")]
    public async Task Generate_WithWhitespacePrompt_ShouldReturn400BadRequest(string whitespace)
    {
        var request = new
        {
            UserPrompt = whitespace
        };

        var response = await _client.PostAsJsonAsync("/api/ai/generate", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
    }

    [Fact]
    public async Task Generate_WithOutOfRangeTemperature_ShouldReturn400BadRequest()
    {
        var request = new
        {
            UserPrompt = "Valid prompt",
            Temperature = 3.5 // Exceeds 2.0
        };

        var response = await _client.PostAsJsonAsync("/api/ai/generate", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
    }

    [Fact]
    public async Task Generate_WithZeroMaxTokens_ShouldReturn400BadRequest()
    {
        var request = new
        {
            UserPrompt = "Valid prompt",
            MaxTokens = 0
        };

        var response = await _client.PostAsJsonAsync("/api/ai/generate", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
    }
}
