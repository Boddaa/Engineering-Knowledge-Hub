namespace EngineeringKnowledgeHub.Tests.Infrastructure;

using System.Net;
using System.Text;
using EngineeringKnowledgeHub.Application.Enrichment;
using EngineeringKnowledgeHub.Infrastructure.Enrichment;
using Xunit;

public class HttpExternalKnowledgeProviderTests
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
    public async Task FetchContentAsync_WithSuccessfulResponse_ShouldReturnSuccessWithContent()
    {
        var content = "Sample engineering vendor documentation content.";
        var handler = new MockHttpMessageHandler((req, ct) =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(content, Encoding.UTF8, "text/plain")
            };
            return Task.FromResult(response);
        });

        using var client = new HttpClient(handler);
        using var provider = new HttpExternalKnowledgeProvider(client);

        var request = new ExternalContentRequest("https://vendor.example.com/docs/api.txt");
        var result = await provider.FetchContentAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal("https://vendor.example.com/docs/api.txt", result.Url);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Content);
        Assert.Equal(content, Encoding.UTF8.GetString(result.Content));
        Assert.Equal("text/plain", result.ContentType);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public async Task FetchContentAsync_WithHttp404NotFound_ShouldReturnHttpErrorFailure()
    {
        var handler = new MockHttpMessageHandler((req, ct) =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                ReasonPhrase = "Not Found"
            };
            return Task.FromResult(response);
        });

        using var client = new HttpClient(handler);
        using var provider = new HttpExternalKnowledgeProvider(client);

        var request = new ExternalContentRequest("https://vendor.example.com/missing-doc.html");
        var result = await provider.FetchContentAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal("HttpError", result.ErrorCategory);
        Assert.Equal(404, result.StatusCode);
        Assert.Contains("404", result.ErrorMessage);
    }

    [Fact]
    public async Task FetchContentAsync_WithHttp500InternalServerError_ShouldReturnHttpErrorFailure()
    {
        var handler = new MockHttpMessageHandler((req, ct) =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                ReasonPhrase = "Internal Server Error"
            };
            return Task.FromResult(response);
        });

        using var client = new HttpClient(handler);
        using var provider = new HttpExternalKnowledgeProvider(client);

        var request = new ExternalContentRequest("https://vendor.example.com/error");
        var result = await provider.FetchContentAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal("HttpError", result.ErrorCategory);
        Assert.Equal(500, result.StatusCode);
    }

    [Theory]
    [InlineData("ftp://ftp.example.com/file.txt")]
    [InlineData("file:///C:/local/secret.txt")]
    [InlineData("gopher://gopher.example.com")]
    public async Task FetchContentAsync_WithUnsupportedScheme_ShouldReturnInvalidSchemeFailure(string unsupportedUrl)
    {
        using var provider = new HttpExternalKnowledgeProvider();

        var request = new ExternalContentRequest(unsupportedUrl);
        var result = await provider.FetchContentAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal("InvalidScheme", result.ErrorCategory);
        Assert.Contains("Only HTTP and HTTPS are permitted", result.ErrorMessage);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-a-valid-uri")]
    public async Task FetchContentAsync_WithMalformedUrl_ShouldReturnInvalidUrlFailure(string? invalidUrl)
    {
        using var provider = new HttpExternalKnowledgeProvider();

        var request = new ExternalContentRequest(invalidUrl!);
        var result = await provider.FetchContentAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal("InvalidUrl", result.ErrorCategory);
    }

    [Fact]
    public async Task FetchContentAsync_WhenContentLengthHeaderExceedsLimit_ShouldReturnExceededSizeLimit()
    {
        var handler = new MockHttpMessageHandler((req, ct) =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(new byte[100])
            };
            response.Content.Headers.ContentLength = 1000;
            return Task.FromResult(response);
        });

        using var client = new HttpClient(handler);
        using var provider = new HttpExternalKnowledgeProvider(client);

        var request = new ExternalContentRequest(
            "https://vendor.example.com/large.bin",
            MaxContentSizeBytes: 500);

        var result = await provider.FetchContentAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal("ExceededSizeLimit", result.ErrorCategory);
        Assert.Contains("exceeds maximum configured limit", result.ErrorMessage);
    }

    [Fact]
    public async Task FetchContentAsync_WhenStreamExceedsLimit_ShouldReturnExceededSizeLimit()
    {
        var largeContent = new byte[2000];
        Array.Fill(largeContent, (byte)65);

        var handler = new MockHttpMessageHandler((req, ct) =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(largeContent)
            };
            // Clear content length so it checks the stream reading boundary
            response.Content.Headers.ContentLength = null;
            return Task.FromResult(response);
        });

        using var client = new HttpClient(handler);
        using var provider = new HttpExternalKnowledgeProvider(client);

        var request = new ExternalContentRequest(
            "https://vendor.example.com/large.bin",
            MaxContentSizeBytes: 500);

        var result = await provider.FetchContentAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal("ExceededSizeLimit", result.ErrorCategory);
        Assert.Contains("stream exceeded maximum configured limit", result.ErrorMessage);
    }

    [Fact]
    public async Task FetchContentAsync_WhenCancelled_ShouldReturnCancelledFailure()
    {
        var handler = new MockHttpMessageHandler(async (req, ct) =>
        {
            await Task.Delay(5000, ct);
            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        using var client = new HttpClient(handler);
        using var provider = new HttpExternalKnowledgeProvider(client);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var request = new ExternalContentRequest("https://vendor.example.com/slow");
        var result = await provider.FetchContentAsync(request, cts.Token);

        Assert.False(result.IsSuccess);
        Assert.Equal("Cancelled", result.ErrorCategory);
    }

    [Fact]
    public async Task FetchContentAsync_WhenNetworkFails_ShouldReturnNetworkError()
    {
        var handler = new MockHttpMessageHandler((req, ct) =>
        {
            throw new HttpRequestException("DNS resolution failed");
        });

        using var client = new HttpClient(handler);
        using var provider = new HttpExternalKnowledgeProvider(client);

        var request = new ExternalContentRequest("https://unreachable.vendor.local/doc");
        var result = await provider.FetchContentAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal("NetworkError", result.ErrorCategory);
        Assert.Contains("DNS resolution failed", result.ErrorMessage);
    }
}
