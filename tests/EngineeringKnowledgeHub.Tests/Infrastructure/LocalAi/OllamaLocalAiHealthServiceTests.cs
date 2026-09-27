namespace EngineeringKnowledgeHub.Tests.Infrastructure.LocalAi;

using System.Net;
using EngineeringKnowledgeHub.Application.Ai;
using EngineeringKnowledgeHub.Infrastructure.LocalAi;
using Xunit;

public class OllamaLocalAiHealthServiceTests
{
    private sealed class MockHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handlerFunc)
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return handlerFunc(request, cancellationToken);
        }
    }

    [Fact]
    public async Task CheckHealthAsync_WhenDisabled_ShouldReturnDisabledStatusWithoutNetworkCall()
    {
        int callCount = 0;
        var handler = new MockHttpMessageHandler((_, _) =>
        {
            callCount++;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        });

        using var client = new HttpClient(handler) { BaseAddress = new Uri("http://127.0.0.1:11434") };
        var options = new LocalAiOptions { Enabled = false };
        var service = new OllamaLocalAiHealthService(client, options);

        var health = await service.CheckHealthAsync();

        Assert.False(health.Enabled);
        Assert.False(health.RuntimeReachable);
        Assert.Equal(LocalAiHealthStatus.Disabled, health.Status);
        Assert.Contains("disabled", health.DiagnosticMessage, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(0, callCount);
    }

    [Fact]
    public async Task CheckHealthAsync_WhenMisconfigured_ShouldReturnMisconfiguredStatusWithoutNetworkCall()
    {
        int callCount = 0;
        var handler = new MockHttpMessageHandler((_, _) =>
        {
            callCount++;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        });

        using var client = new HttpClient(handler) { BaseAddress = new Uri("http://127.0.0.1:11434") };
        var options = new LocalAiOptions
        {
            Enabled = true,
            BaseUrl = "invalid-url",
            GenerationModel = "llama3",
            EmbeddingModel = "nomic-embed-text"
        };
        var service = new OllamaLocalAiHealthService(client, options);

        var health = await service.CheckHealthAsync();

        Assert.True(health.Enabled);
        Assert.False(health.RuntimeReachable);
        Assert.Equal(LocalAiHealthStatus.Misconfigured, health.Status);
        Assert.Contains("configuration error", health.DiagnosticMessage, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(0, callCount);
    }

    [Fact]
    public async Task CheckHealthAsync_WhenRuntimeUnreachable_ShouldReturnUnavailableStatus()
    {
        var handler = new MockHttpMessageHandler((req, ct) =>
            throw new HttpRequestException("Connection refused"));

        using var client = new HttpClient(handler) { BaseAddress = new Uri("http://127.0.0.1:11434") };
        var options = new LocalAiOptions
        {
            Enabled = true,
            BaseUrl = "http://127.0.0.1:11434",
            GenerationModel = "llama3",
            EmbeddingModel = "nomic-embed-text"
        };
        var service = new OllamaLocalAiHealthService(client, options);

        var health = await service.CheckHealthAsync();

        Assert.True(health.Enabled);
        Assert.False(health.RuntimeReachable);
        Assert.Equal(LocalAiHealthStatus.Unavailable, health.Status);
        Assert.Contains("unreachable", health.DiagnosticMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CheckHealthAsync_WhenBothModelsAvailable_ShouldReturnHealthyStatus()
    {
        var handler = new MockHttpMessageHandler((req, ct) =>
        {
            Assert.Equal("/api/tags", req.RequestUri!.AbsolutePath);

            var jsonResponse = """
            {
                "models": [
                    { "name": "llama3:latest", "model": "llama3:latest" },
                    { "name": "nomic-embed-text:latest", "model": "nomic-embed-text:latest" }
                ]
            }
            """;

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(jsonResponse, System.Text.Encoding.UTF8, "application/json")
            });
        });

        using var client = new HttpClient(handler) { BaseAddress = new Uri("http://127.0.0.1:11434") };
        var options = new LocalAiOptions
        {
            Enabled = true,
            BaseUrl = "http://127.0.0.1:11434",
            GenerationModel = "llama3",
            EmbeddingModel = "nomic-embed-text"
        };
        var service = new OllamaLocalAiHealthService(client, options);

        var health = await service.CheckHealthAsync();

        Assert.True(health.Enabled);
        Assert.True(health.RuntimeReachable);
        Assert.True(health.GenerationModelAvailable);
        Assert.True(health.EmbeddingModelAvailable);
        Assert.Equal("llama3", health.GenerationModelConfigured);
        Assert.Equal("nomic-embed-text", health.EmbeddingModelConfigured);
        Assert.Equal(LocalAiHealthStatus.Healthy, health.Status);
        Assert.Contains("reachable and all configured models are available", health.DiagnosticMessage);
    }

    [Fact]
    public async Task CheckHealthAsync_WhenGenerationModelMissing_ShouldReturnModelUnavailableStatus()
    {
        var handler = new MockHttpMessageHandler((req, ct) =>
        {
            var jsonResponse = """
            {
                "models": [
                    { "name": "nomic-embed-text:latest", "model": "nomic-embed-text:latest" }
                ]
            }
            """;

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(jsonResponse, System.Text.Encoding.UTF8, "application/json")
            });
        });

        using var client = new HttpClient(handler) { BaseAddress = new Uri("http://127.0.0.1:11434") };
        var options = new LocalAiOptions
        {
            Enabled = true,
            BaseUrl = "http://127.0.0.1:11434",
            GenerationModel = "llama3",
            EmbeddingModel = "nomic-embed-text"
        };
        var service = new OllamaLocalAiHealthService(client, options);

        var health = await service.CheckHealthAsync();

        Assert.True(health.Enabled);
        Assert.True(health.RuntimeReachable);
        Assert.False(health.GenerationModelAvailable);
        Assert.True(health.EmbeddingModelAvailable);
        Assert.Equal(LocalAiHealthStatus.ModelUnavailable, health.Status);
        Assert.Contains("generation model 'llama3' is unavailable", health.DiagnosticMessage);
    }

    [Fact]
    public async Task CheckHealthAsync_WhenEmbeddingModelMissing_ShouldReturnModelUnavailableStatus()
    {
        var handler = new MockHttpMessageHandler((req, ct) =>
        {
            var jsonResponse = """
            {
                "models": [
                    { "name": "llama3:latest", "model": "llama3:latest" }
                ]
            }
            """;

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(jsonResponse, System.Text.Encoding.UTF8, "application/json")
            });
        });

        using var client = new HttpClient(handler) { BaseAddress = new Uri("http://127.0.0.1:11434") };
        var options = new LocalAiOptions
        {
            Enabled = true,
            BaseUrl = "http://127.0.0.1:11434",
            GenerationModel = "llama3",
            EmbeddingModel = "nomic-embed-text"
        };
        var service = new OllamaLocalAiHealthService(client, options);

        var health = await service.CheckHealthAsync();

        Assert.True(health.Enabled);
        Assert.True(health.RuntimeReachable);
        Assert.True(health.GenerationModelAvailable);
        Assert.False(health.EmbeddingModelAvailable);
        Assert.Equal(LocalAiHealthStatus.ModelUnavailable, health.Status);
        Assert.Contains("embedding model 'nomic-embed-text' is unavailable", health.DiagnosticMessage);
    }
}
