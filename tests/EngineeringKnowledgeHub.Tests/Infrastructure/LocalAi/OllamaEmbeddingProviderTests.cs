namespace EngineeringKnowledgeHub.Tests.Infrastructure.LocalAi;

using System.Net;
using System.Text.Json;
using EngineeringKnowledgeHub.Application.Common.Exceptions;
using EngineeringKnowledgeHub.Infrastructure.LocalAi;
using Xunit;

public class OllamaEmbeddingProviderTests
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
    public async Task GenerateEmbeddingAsync_WithValidInput_ShouldReturnValidatedVector()
    {
        HttpRequestMessage? capturedRequest = null;
        string? capturedBody = null;

        var handler = new MockHttpMessageHandler(async (req, ct) =>
        {
            capturedRequest = req;
            capturedBody = await req.Content!.ReadAsStringAsync(ct);

            // 4 dimensions for test simplicity
            var jsonResponse = """
            {
                "model": "nomic-embed-text",
                "embeddings": [
                    [0.25, 0.5, 0.75, 1.0]
                ]
            }
            """;

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(jsonResponse, System.Text.Encoding.UTF8, "application/json")
            };
        });

        using var client = new HttpClient(handler) { BaseAddress = new Uri("http://127.0.0.1:11434") };
        var options = new LocalAiOptions
        {
            Enabled = true,
            EmbeddingModel = "nomic-embed-text",
            EmbeddingDimensions = 4
        };

        var provider = new OllamaEmbeddingProvider(client, options);

        Assert.Equal(4, provider.Dimensions);

        var vector = await provider.GenerateEmbeddingAsync("Transformer attention mechanisms");

        Assert.NotNull(vector);
        Assert.Equal(4, vector.Dimensions);
        Assert.Equal(0.25f, vector[0]);
        Assert.Equal(0.5f, vector[1]);
        Assert.Equal(0.75f, vector[2]);
        Assert.Equal(1.0f, vector[3]);

        Assert.NotNull(capturedRequest);
        Assert.Equal("/api/embed", capturedRequest.RequestUri!.AbsolutePath);

        using var doc = JsonDocument.Parse(capturedBody!);
        Assert.Equal("nomic-embed-text", doc.RootElement.GetProperty("model").GetString());
        Assert.Equal("Transformer attention mechanisms", doc.RootElement.GetProperty("input").GetString());
    }

    [Fact]
    public async Task GenerateEmbeddingAsync_WhenDimensionsMismatch_ShouldThrowValidationExceptionWithoutResizing()
    {
        var handler = new MockHttpMessageHandler((req, ct) =>
        {
            // Returns 3 dimensions when 4 are configured
            var jsonResponse = """
            {
                "model": "nomic-embed-text",
                "embeddings": [
                    [0.1, 0.2, 0.3]
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
            EmbeddingModel = "nomic-embed-text",
            EmbeddingDimensions = 4 // Configured 4 dimensions
        };

        var provider = new OllamaEmbeddingProvider(client, options);

        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            provider.GenerateEmbeddingAsync("Test dimension safety"));

        Assert.Contains("dimension mismatch", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("4", ex.Message);
        Assert.Contains("3", ex.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task GenerateEmbeddingAsync_WithNullOrEmptyText_ShouldThrowValidationException(string? invalidText)
    {
        using var client = new HttpClient(new MockHttpMessageHandler((_, _) => Task.FromResult(new HttpResponseMessage())));
        var options = new LocalAiOptions { Enabled = true, EmbeddingModel = "nomic-embed-text", EmbeddingDimensions = 4 };
        var provider = new OllamaEmbeddingProvider(client, options);

        await Assert.ThrowsAsync<ValidationException>(() =>
            provider.GenerateEmbeddingAsync(invalidText!));
    }

    [Fact]
    public async Task GenerateEmbeddingAsync_WhenEmbedEndpointIsNotFound_ShouldFallbackToLegacyEndpoint()
    {
        int callCount = 0;
        var handler = new MockHttpMessageHandler((req, ct) =>
        {
            callCount++;
            if (req.RequestUri!.AbsolutePath == "/api/embed")
            {
                // Simulate older Ollama runtime without /api/embed
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
            }

            if (req.RequestUri.AbsolutePath == "/api/embeddings")
            {
                var jsonResponse = """
                {
                    "embedding": [0.1, 0.2, 0.3, 0.4]
                }
                """;

                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(jsonResponse, System.Text.Encoding.UTF8, "application/json")
                });
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest));
        });

        using var client = new HttpClient(handler) { BaseAddress = new Uri("http://127.0.0.1:11434") };
        var options = new LocalAiOptions { Enabled = true, EmbeddingModel = "nomic-embed-text", EmbeddingDimensions = 4 };
        var provider = new OllamaEmbeddingProvider(client, options);

        var vector = await provider.GenerateEmbeddingAsync("Fallback test");

        Assert.NotNull(vector);
        Assert.Equal(4, vector.Dimensions);
        Assert.Equal(2, callCount); // First /api/embed (404), then /api/embeddings (200)
    }

    [Fact]
    public async Task GenerateBatchEmbeddingsAsync_WithValidInputs_ShouldReturnAllVectors()
    {
        var handler = new MockHttpMessageHandler((req, ct) =>
        {
            var jsonResponse = """
            {
                "model": "nomic-embed-text",
                "embeddings": [
                    [0.1, 0.2, 0.3],
                    [0.4, 0.5, 0.6]
                ]
            }
            """;

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(jsonResponse, System.Text.Encoding.UTF8, "application/json")
            });
        });

        using var client = new HttpClient(handler) { BaseAddress = new Uri("http://127.0.0.1:11434") };
        var options = new LocalAiOptions { Enabled = true, EmbeddingModel = "nomic-embed-text", EmbeddingDimensions = 3 };
        var provider = new OllamaEmbeddingProvider(client, options);

        var texts = new[] { "First text", "Second text" };
        var batch = await provider.GenerateBatchEmbeddingsAsync(texts);

        Assert.Equal(2, batch.Count);
        Assert.Equal(3, batch[0].Dimensions);
        Assert.Equal(3, batch[1].Dimensions);
        Assert.Equal(0.1f, batch[0][0]);
        Assert.Equal(0.4f, batch[1][0]);
    }

    [Fact]
    public async Task GenerateBatchEmbeddingsAsync_WhenBatchCountMismatches_ShouldThrowInvalidOperationException()
    {
        var handler = new MockHttpMessageHandler((req, ct) =>
        {
            // Returns 1 vector when 2 were requested
            var jsonResponse = """
            {
                "model": "nomic-embed-text",
                "embeddings": [
                    [0.1, 0.2, 0.3]
                ]
            }
            """;

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(jsonResponse, System.Text.Encoding.UTF8, "application/json")
            });
        });

        using var client = new HttpClient(handler) { BaseAddress = new Uri("http://127.0.0.1:11434") };
        var options = new LocalAiOptions { Enabled = true, EmbeddingModel = "nomic-embed-text", EmbeddingDimensions = 3 };
        var provider = new OllamaEmbeddingProvider(client, options);

        var texts = new[] { "Text A", "Text B" };
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            provider.GenerateBatchEmbeddingsAsync(texts));
    }

    [Fact]
    public async Task GenerateBatchEmbeddingsAsync_WithEmptyList_ShouldReturnEmptyListWithoutNetworkCall()
    {
        int callCount = 0;
        var handler = new MockHttpMessageHandler((_, _) =>
        {
            callCount++;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        });

        using var client = new HttpClient(handler) { BaseAddress = new Uri("http://127.0.0.1:11434") };
        var options = new LocalAiOptions { Enabled = true, EmbeddingModel = "nomic-embed-text", EmbeddingDimensions = 3 };
        var provider = new OllamaEmbeddingProvider(client, options);

        var result = await provider.GenerateBatchEmbeddingsAsync(Array.Empty<string>());

        Assert.Empty(result);
        Assert.Equal(0, callCount);
    }

    [Fact]
    public async Task GenerateEmbeddingAsync_WhenRuntimeUnreachable_ShouldThrowMeaningfulException()
    {
        var handler = new MockHttpMessageHandler((req, ct) =>
            throw new HttpRequestException("Connection refused by peer"));

        using var client = new HttpClient(handler) { BaseAddress = new Uri("http://127.0.0.1:11434") };
        var options = new LocalAiOptions { Enabled = true, EmbeddingModel = "nomic-embed-text", EmbeddingDimensions = 3, BaseUrl = "http://127.0.0.1:11434" };
        var provider = new OllamaEmbeddingProvider(client, options);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            provider.GenerateEmbeddingAsync("Test input"));

        Assert.Contains("unreachable", ex.Message);
        Assert.Contains("127.0.0.1:11434", ex.Message);
    }
}
