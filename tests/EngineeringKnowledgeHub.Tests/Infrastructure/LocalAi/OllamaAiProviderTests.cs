namespace EngineeringKnowledgeHub.Tests.Infrastructure.LocalAi;

using System.Net;
using System.Text.Json;
using EngineeringKnowledgeHub.Application.Common.Exceptions;
using EngineeringKnowledgeHub.Application.Rag;
using EngineeringKnowledgeHub.Infrastructure.LocalAi;
using Xunit;

public class OllamaAiProviderTests
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
    public async Task GenerateAsync_WithValidRequest_ShouldSendCorrectPayloadAndReturnResponse()
    {
        HttpRequestMessage? capturedRequest = null;
        string? capturedBody = null;

        var handler = new MockHttpMessageHandler(async (req, ct) =>
        {
            capturedRequest = req;
            capturedBody = await req.Content!.ReadAsStringAsync(ct);

            var jsonResponse = """
            {
                "model": "llama3:8b",
                "created_at": "2026-09-27T12:00:00Z",
                "message": {
                    "role": "assistant",
                    "content": "According to [1], RS-485 termination requires a 120-ohm resistor."
                },
                "done": true,
                "prompt_eval_count": 42,
                "eval_count": 18
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
            GenerationModel = "llama3:8b",
            TimeoutSeconds = 30
        };

        var provider = new OllamaAiProvider(client, options);

        var promptRequest = new AiPromptRequest(
            SystemPrompt: "You are an engineering assistant. Answer based on citations.",
            UserPrompt: "What is RS-485 termination resistor value?",
            MaxTokens: 256,
            Temperature: 0.2);

        var response = await provider.GenerateAsync(promptRequest);

        Assert.NotNull(response);
        Assert.Equal("According to [1], RS-485 termination requires a 120-ohm resistor.", response.Content);
        Assert.Equal("llama3:8b", response.ModelName);
        Assert.Equal(42, response.PromptTokens);
        Assert.Equal(18, response.CompletionTokens);

        // Verify request payload sent to Ollama
        Assert.NotNull(capturedRequest);
        Assert.Equal(HttpMethod.Post, capturedRequest.Method);
        Assert.Equal("/api/chat", capturedRequest.RequestUri!.AbsolutePath);

        using var doc = JsonDocument.Parse(capturedBody!);
        var root = doc.RootElement;
        Assert.Equal("llama3:8b", root.GetProperty("model").GetString());
        Assert.False(root.GetProperty("stream").GetBoolean());

        var messages = root.GetProperty("messages");
        Assert.Equal(2, messages.GetArrayLength());
        Assert.Equal("system", messages[0].GetProperty("role").GetString());
        Assert.Equal("You are an engineering assistant. Answer based on citations.", messages[0].GetProperty("content").GetString());
        Assert.Equal("user", messages[1].GetProperty("role").GetString());
        Assert.Equal("What is RS-485 termination resistor value?", messages[1].GetProperty("content").GetString());

        var modelOptions = root.GetProperty("options");
        Assert.Equal(0.2, modelOptions.GetProperty("temperature").GetDouble(), 2);
        Assert.Equal(256, modelOptions.GetProperty("num_predict").GetInt32());
    }

    [Fact]
    public async Task GenerateAsync_WithoutSystemPrompt_ShouldOnlySendUserMessage()
    {
        string? capturedBody = null;

        var handler = new MockHttpMessageHandler(async (req, ct) =>
        {
            capturedBody = await req.Content!.ReadAsStringAsync(ct);

            var jsonResponse = """
            {
                "model": "llama3",
                "message": { "role": "assistant", "content": "Direct response." },
                "done": true
            }
            """;

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(jsonResponse, System.Text.Encoding.UTF8, "application/json")
            };
        });

        using var client = new HttpClient(handler) { BaseAddress = new Uri("http://127.0.0.1:11434") };
        var options = new LocalAiOptions { Enabled = true, GenerationModel = "llama3" };
        var provider = new OllamaAiProvider(client, options);

        var promptRequest = new AiPromptRequest(SystemPrompt: "", UserPrompt: "User prompt only.");
        var response = await provider.GenerateAsync(promptRequest);

        Assert.Equal("Direct response.", response.Content);
        Assert.Equal(0, response.PromptTokens);
        Assert.Equal(0, response.CompletionTokens);

        using var doc = JsonDocument.Parse(capturedBody!);
        var messages = doc.RootElement.GetProperty("messages");
        Assert.Equal(1, messages.GetArrayLength());
        Assert.Equal("user", messages[0].GetProperty("role").GetString());
    }

    [Fact]
    public async Task GenerateAsync_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        var handler = new MockHttpMessageHandler((req, ct) =>
        {
            var tcs = new TaskCompletionSource<HttpResponseMessage>();
            ct.Register(() => tcs.SetCanceled(ct));
            return tcs.Task;
        });

        using var client = new HttpClient(handler) { BaseAddress = new Uri("http://127.0.0.1:11434") };
        var options = new LocalAiOptions { Enabled = true, GenerationModel = "llama3" };
        var provider = new OllamaAiProvider(client, options);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            provider.GenerateAsync(new AiPromptRequest("sys", "user"), cts.Token));
    }

    [Fact]
    public async Task GenerateAsync_WhenRuntimeUnreachable_ShouldThrowMeaningfulException()
    {
        var handler = new MockHttpMessageHandler((req, ct) =>
            throw new HttpRequestException("Connection refused by peer"));

        using var client = new HttpClient(handler) { BaseAddress = new Uri("http://127.0.0.1:11434") };
        var options = new LocalAiOptions { Enabled = true, GenerationModel = "llama3", BaseUrl = "http://127.0.0.1:11434" };
        var provider = new OllamaAiProvider(client, options);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            provider.GenerateAsync(new AiPromptRequest("sys", "user")));

        Assert.Contains("unreachable", ex.Message);
        Assert.Contains("127.0.0.1:11434", ex.Message);
    }

    [Fact]
    public async Task GenerateAsync_WhenRuntimeTimesOut_ShouldThrowMeaningfulTimeoutException()
    {
        var handler = new MockHttpMessageHandler((req, ct) =>
            throw new TaskCanceledException("The operation was canceled due to timeout."));

        using var client = new HttpClient(handler) { BaseAddress = new Uri("http://127.0.0.1:11434") };
        var options = new LocalAiOptions { Enabled = true, GenerationModel = "llama3", TimeoutSeconds = 15 };
        var provider = new OllamaAiProvider(client, options);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            provider.GenerateAsync(new AiPromptRequest("sys", "user")));

        Assert.Contains("timed out", ex.Message);
        Assert.Contains("15", ex.Message);
    }

    [Fact]
    public async Task GenerateAsync_WhenServerReturnsHttpError_ShouldThrowMeaningfulException()
    {
        var handler = new MockHttpMessageHandler((req, ct) =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                Content = new StringContent("""{"error": "model 'llama3' not found, try pulling it first"}""")
            };
            return Task.FromResult(response);
        });

        using var client = new HttpClient(handler) { BaseAddress = new Uri("http://127.0.0.1:11434") };
        var options = new LocalAiOptions { Enabled = true, GenerationModel = "llama3" };
        var provider = new OllamaAiProvider(client, options);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            provider.GenerateAsync(new AiPromptRequest("sys", "user")));

        Assert.Contains("404", ex.Message);
        Assert.Contains("not found", ex.Message);
    }

    [Fact]
    public async Task GenerateAsync_WhenMalformedJsonReturned_ShouldThrowMeaningfulException()
    {
        var handler = new MockHttpMessageHandler((req, ct) =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("not-valid-json")
            };
            return Task.FromResult(response);
        });

        using var client = new HttpClient(handler) { BaseAddress = new Uri("http://127.0.0.1:11434") };
        var options = new LocalAiOptions { Enabled = true, GenerationModel = "llama3" };
        var provider = new OllamaAiProvider(client, options);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            provider.GenerateAsync(new AiPromptRequest("sys", "user")));

        Assert.Contains("invalid or malformed", ex.Message);
    }

    [Fact]
    public async Task GenerateAsync_WhenNullRequest_ShouldThrowValidationException()
    {
        using var client = new HttpClient(new MockHttpMessageHandler((_, _) => Task.FromResult(new HttpResponseMessage())));
        var options = new LocalAiOptions { Enabled = true, GenerationModel = "llama3" };
        var provider = new OllamaAiProvider(client, options);

        await Assert.ThrowsAsync<ValidationException>(() =>
            provider.GenerateAsync(null!));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task GenerateAsync_WhenEmptyUserPrompt_ShouldThrowValidationException(string? invalidPrompt)
    {
        using var client = new HttpClient(new MockHttpMessageHandler((_, _) => Task.FromResult(new HttpResponseMessage())));
        var options = new LocalAiOptions { Enabled = true, GenerationModel = "llama3" };
        var provider = new OllamaAiProvider(client, options);

        await Assert.ThrowsAsync<ValidationException>(() =>
            provider.GenerateAsync(new AiPromptRequest("sys", invalidPrompt!)));
    }
}
