namespace EngineeringKnowledgeHub.Tests.Application;

using System.Net;
using EngineeringKnowledgeHub.Application.DTOs;
using EngineeringKnowledgeHub.Application.Rag;
using EngineeringKnowledgeHub.Application.Search;
using EngineeringKnowledgeHub.Application.SemanticSearch;
using EngineeringKnowledgeHub.Domain.Entities;
using EngineeringKnowledgeHub.Domain.Enums;
using EngineeringKnowledgeHub.Domain.ValueObjects;
using EngineeringKnowledgeHub.Infrastructure.LocalAi;
using EngineeringKnowledgeHub.Tests.Common;
using Xunit;

public class OfflineRagLocalAiIntegrationTests
{
    private sealed class MockHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handlerFunc)
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return handlerFunc(request, cancellationToken);
        }
    }

    private readonly InMemoryKnowledgeArticleRepository _articleRepo = new();
    private readonly InMemoryIncidentRepository _incidentRepo = new();
    private readonly InMemoryDocumentRepository _docRepo = new();
    private readonly InMemoryVectorStore _vectorStore = new();
    private readonly DeterministicEmbeddingProvider _embeddingProvider = new(dimensions: 32);
    private readonly FakeDateTimeProvider _dateTimeProvider = new(DateTimeOffset.UtcNow);

    private readonly SemanticSearchService _semanticSearchService;
    private readonly SemanticIndexService _semanticIndexService;
    private readonly SearchService _searchService;
    private readonly KnowledgeRetriever _retriever;
    private readonly ContextBuilder _contextBuilder = new();

    public OfflineRagLocalAiIntegrationTests()
    {
        _semanticSearchService = new SemanticSearchService(_embeddingProvider, _vectorStore);
        _semanticIndexService = new SemanticIndexService(_embeddingProvider, _vectorStore);
        _searchService = new SearchService(_articleRepo, _semanticSearchService);
        _retriever = new KnowledgeRetriever(_searchService, _semanticSearchService, _incidentRepo, _docRepo, _articleRepo);
    }

    [Fact]
    public async Task AskAsync_WithLocalOllamaProvider_ShouldGenerateAnswerUsingLocalModel_AndPreserveGroundingAndEvidence()
    {
        var article = new KnowledgeArticle(
            Guid.NewGuid(),
            "RS-485 Barrier Bus Termination",
            Slug.FromTitle("RS-485 Barrier Bus Termination"),
            "Termination requirements for the barrier gate controller",
            "RS-485 communication line must be terminated with a 120-ohm resistor at each physical end of the bus to prevent reflection.",
            Guid.NewGuid(),
            Guid.NewGuid(),
            _dateTimeProvider.UtcNow);
        article.SubmitForReview();
        article.Approve(Guid.NewGuid(), _dateTimeProvider.UtcNow);

        await _articleRepo.AddAsync(article);
        await _semanticIndexService.IndexArticleAsync(article);

        var handler = new MockHttpMessageHandler((req, ct) =>
        {
            var jsonResponse = """
            {
                "model": "llama3:8b",
                "message": {
                    "role": "assistant",
                    "content": "According to [1], the RS-485 communication line must be terminated with a 120-ohm resistor at each physical end of the bus."
                },
                "done": true,
                "prompt_eval_count": 80,
                "eval_count": 25
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
            GenerationModel = "llama3:8b",
            TimeoutSeconds = 30
        };

        IAiProvider ollamaProvider = new OllamaAiProvider(client, options);
        var ragService = new RagService(_retriever, _contextBuilder, ollamaProvider, _dateTimeProvider);

        var request = new RagQuestionRequest
        {
            Question = "What resistor is required for RS-485 termination?",
            MaxEvidence = 3
        };

        var response = await ragService.AskAsync(request);

        Assert.NotNull(response);
        Assert.True(response.IsSufficientEvidence);
        Assert.Equal("llama3:8b", response.ModelUsed);
        Assert.Contains("120-ohm resistor", response.Answer);
        Assert.NotEmpty(response.Evidence);
        Assert.Equal(1, response.Evidence[0].CitationNumber);
    }

    [Fact]
    public async Task AskAsync_WhenLocalAiRuntimeFails_ShouldThrowMeaningfulExceptionAndNeverFabricateAnswer()
    {
        var article = new KnowledgeArticle(
            Guid.NewGuid(),
            "Camera Calibration Guide",
            Slug.FromTitle("Camera Calibration Guide"),
            "Camera focus alignment",
            "Calibrate cameras at 25 FPS using test target card at 5 meters distance.",
            Guid.NewGuid(),
            Guid.NewGuid(),
            _dateTimeProvider.UtcNow);
        article.SubmitForReview();
        article.Approve(Guid.NewGuid(), _dateTimeProvider.UtcNow);

        await _articleRepo.AddAsync(article);
        await _semanticIndexService.IndexArticleAsync(article);

        var handler = new MockHttpMessageHandler((req, ct) =>
            throw new HttpRequestException("Local Ollama daemon is offline"));

        using var client = new HttpClient(handler) { BaseAddress = new Uri("http://127.0.0.1:11434") };
        var options = new LocalAiOptions
        {
            Enabled = true,
            GenerationModel = "llama3:8b",
            BaseUrl = "http://127.0.0.1:11434"
        };

        IAiProvider ollamaProvider = new OllamaAiProvider(client, options);
        var ragService = new RagService(_retriever, _contextBuilder, ollamaProvider, _dateTimeProvider);

        var request = new RagQuestionRequest
        {
            Question = "How do I calibrate the camera?",
            MaxEvidence = 3
        };

        // Failure must propagate; the system must NOT fabricate a fake successful answer
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            ragService.AskAsync(request));

        Assert.Contains("unreachable", ex.Message);
    }

    [Fact]
    public async Task AskAsync_WhenLocalAiReturnsUngroundedAnswer_ShouldReturnGroundingFailureFallback()
    {
        var article = new KnowledgeArticle(
            Guid.NewGuid(),
            "Fire Suppression Reset",
            Slug.FromTitle("Fire Suppression Reset"),
            "Reset instructions",
            "Press the yellow button on panel 2 after verifying clear status.",
            Guid.NewGuid(),
            Guid.NewGuid(),
            _dateTimeProvider.UtcNow);
        article.SubmitForReview();
        article.Approve(Guid.NewGuid(), _dateTimeProvider.UtcNow);

        await _articleRepo.AddAsync(article);
        await _semanticIndexService.IndexArticleAsync(article);

        var handler = new MockHttpMessageHandler((req, ct) =>
        {
            // Response has NO citations to evidence
            var jsonResponse = """
            {
                "model": "llama3:8b",
                "message": {
                    "role": "assistant",
                    "content": "Just unplug the main power breaker and wait ten minutes."
                },
                "done": true
            }
            """;

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(jsonResponse, System.Text.Encoding.UTF8, "application/json")
            });
        });

        using var client = new HttpClient(handler) { BaseAddress = new Uri("http://127.0.0.1:11434") };
        var options = new LocalAiOptions { Enabled = true, GenerationModel = "llama3:8b" };

        IAiProvider ollamaProvider = new OllamaAiProvider(client, options);
        var ragService = new RagService(_retriever, _contextBuilder, ollamaProvider, _dateTimeProvider);

        var request = new RagQuestionRequest
        {
            Question = "How do I reset fire suppression?",
            MaxEvidence = 3
        };

        var response = await ragService.AskAsync(request);

        Assert.NotNull(response);
        Assert.False(response.IsSufficientEvidence);
        Assert.Equal(RagService.GroundingFailureFallbackAnswer, response.Answer);
    }
}
