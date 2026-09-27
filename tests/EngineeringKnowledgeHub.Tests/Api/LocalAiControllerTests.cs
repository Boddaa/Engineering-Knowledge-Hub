namespace EngineeringKnowledgeHub.Tests.Api;

using EngineeringKnowledgeHub.Api.Controllers;
using EngineeringKnowledgeHub.Application.Ai;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

public class LocalAiControllerTests
{
    private sealed class FakeLocalAiHealthService(LocalAiHealthDto result) : ILocalAiHealthService
    {
        public Task<LocalAiHealthDto> CheckHealthAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(result);
        }
    }

    [Fact]
    public async Task GetHealth_WhenServiceNull_ShouldReturnOkWithDisabledStatus()
    {
        var controller = new LocalAiController(null);

        var actionResult = await controller.GetHealth();

        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        var dto = Assert.IsType<LocalAiHealthDto>(okResult.Value);
        Assert.False(dto.Enabled);
        Assert.Equal(LocalAiHealthStatus.Disabled, dto.Status);
    }

    [Fact]
    public async Task GetHealth_WhenHealthy_ShouldReturnOkWithHealthDto()
    {
        var healthDto = new LocalAiHealthDto(
            Enabled: true,
            RuntimeReachable: true,
            GenerationModelConfigured: "llama3",
            GenerationModelAvailable: true,
            EmbeddingModelConfigured: "nomic-embed-text",
            EmbeddingModelAvailable: true,
            Status: LocalAiHealthStatus.Healthy,
            DiagnosticMessage: "All systems healthy.");

        var controller = new LocalAiController(new FakeLocalAiHealthService(healthDto));

        var actionResult = await controller.GetHealth();

        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        var returnedDto = Assert.IsType<LocalAiHealthDto>(okResult.Value);
        Assert.True(returnedDto.Enabled);
        Assert.Equal(LocalAiHealthStatus.Healthy, returnedDto.Status);
    }

    [Fact]
    public async Task GetHealth_WhenUnavailable_ShouldReturn503ServiceUnavailableWithDto()
    {
        var healthDto = new LocalAiHealthDto(
            Enabled: true,
            RuntimeReachable: false,
            GenerationModelConfigured: "llama3",
            GenerationModelAvailable: false,
            EmbeddingModelConfigured: "nomic-embed-text",
            EmbeddingModelAvailable: false,
            Status: LocalAiHealthStatus.Unavailable,
            DiagnosticMessage: "Runtime unreachable at 127.0.0.1:11434.");

        var controller = new LocalAiController(new FakeLocalAiHealthService(healthDto));

        var actionResult = await controller.GetHealth();

        var statusResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, statusResult.StatusCode);
        var returnedDto = Assert.IsType<LocalAiHealthDto>(statusResult.Value);
        Assert.Equal(LocalAiHealthStatus.Unavailable, returnedDto.Status);
    }

    [Fact]
    public async Task GetHealth_WhenModelUnavailable_ShouldReturn503ServiceUnavailableWithDto()
    {
        var healthDto = new LocalAiHealthDto(
            Enabled: true,
            RuntimeReachable: true,
            GenerationModelConfigured: "llama3",
            GenerationModelAvailable: false,
            EmbeddingModelConfigured: "nomic-embed-text",
            EmbeddingModelAvailable: true,
            Status: LocalAiHealthStatus.ModelUnavailable,
            DiagnosticMessage: "Generation model 'llama3' is unavailable.");

        var controller = new LocalAiController(new FakeLocalAiHealthService(healthDto));

        var actionResult = await controller.GetHealth();

        var statusResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, statusResult.StatusCode);
        var returnedDto = Assert.IsType<LocalAiHealthDto>(statusResult.Value);
        Assert.Equal(LocalAiHealthStatus.ModelUnavailable, returnedDto.Status);
    }

    [Fact]
    public async Task GetHealth_WhenMisconfigured_ShouldReturn500InternalServerErrorWithDto()
    {
        var healthDto = new LocalAiHealthDto(
            Enabled: true,
            RuntimeReachable: false,
            GenerationModelConfigured: null,
            GenerationModelAvailable: false,
            EmbeddingModelConfigured: null,
            EmbeddingModelAvailable: false,
            Status: LocalAiHealthStatus.Misconfigured,
            DiagnosticMessage: "GenerationModel is required.");

        var controller = new LocalAiController(new FakeLocalAiHealthService(healthDto));

        var actionResult = await controller.GetHealth();

        var statusResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusResult.StatusCode);
        var returnedDto = Assert.IsType<LocalAiHealthDto>(statusResult.Value);
        Assert.Equal(LocalAiHealthStatus.Misconfigured, returnedDto.Status);
    }
}
