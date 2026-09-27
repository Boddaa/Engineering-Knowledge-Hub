namespace EngineeringKnowledgeHub.Tests.Application;

using EngineeringKnowledgeHub.Application.Common.Exceptions;
using EngineeringKnowledgeHub.Application.DTOs;
using EngineeringKnowledgeHub.Application.Rag;
using EngineeringKnowledgeHub.Application.Search;
using EngineeringKnowledgeHub.Application.SemanticSearch;
using EngineeringKnowledgeHub.Domain.Entities;
using EngineeringKnowledgeHub.Domain.Enums;
using EngineeringKnowledgeHub.Domain.ValueObjects;
using EngineeringKnowledgeHub.Tests.Common;
using Xunit;

public class RagServiceTests
{
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
    private readonly DeterministicAiProvider _aiProvider = new();
    private readonly RagService _ragService;

    public RagServiceTests()
    {
        _semanticSearchService = new SemanticSearchService(_embeddingProvider, _vectorStore);
        _semanticIndexService = new SemanticIndexService(_embeddingProvider, _vectorStore);
        _searchService = new SearchService(_articleRepo, _semanticSearchService);
        _retriever = new KnowledgeRetriever(_searchService, _semanticSearchService, _incidentRepo, _docRepo);
        _ragService = new RagService(_retriever, _contextBuilder, _aiProvider, _dateTimeProvider);
    }

    [Fact]
    public async Task AskAsync_WithRelevantKnowledge_ShouldReturnGroundedAnswerAndTraceableEvidence()
    {
        var article = CreateArticle(
            "Toll Barrier Manual Release",
            "Procedure to manually open the barrier gate during power outage",
            "In case of power failure, open the lower cabinet with key #4, locate the red hydraulic valve, and rotate 90 degrees counterclockwise to raise the arm.");

        await _articleRepo.AddAsync(article);
        await _semanticIndexService.IndexArticleAsync(article);

        var request = new RagQuestionRequest
        {
            Question = "How do I manually open the toll barrier during a power outage?",
            MaxEvidence = 3
        };

        var response = await _ragService.AskAsync(request);

        Assert.NotNull(response);
        Assert.True(response.IsSufficientEvidence);
        Assert.NotEmpty(response.Answer);
        Assert.Contains("[1]", response.Answer);
        Assert.Contains("Toll Barrier Manual Release", response.Answer);
        Assert.Single(response.Evidence);

        var citation = response.Evidence[0];
        Assert.Equal(1, citation.CitationNumber);
        Assert.Equal("KnowledgeArticle", citation.SourceType);
        Assert.Equal(article.Id, citation.ReferenceId);
        Assert.Equal(article.Title, citation.Title);
        Assert.False(string.IsNullOrWhiteSpace(citation.Excerpt));
        Assert.True(citation.RelevanceScore > 0.0);
        Assert.Equal("DeterministicLocalModel", response.ModelUsed);
        Assert.Equal(_dateTimeProvider.UtcNow, response.AnsweredAt);
    }

    [Fact]
    public async Task AskAsync_WhenNoRelevantKnowledgeExists_ShouldReturnInsufficientEvidenceWithoutHallucination()
    {
        var request = new RagQuestionRequest
        {
            Question = "What is the quantum encryption protocol for interstellar telemetry?"
        };

        var response = await _ragService.AskAsync(request);

        Assert.NotNull(response);
        Assert.False(response.IsSufficientEvidence);
        Assert.Empty(response.Evidence);
        Assert.Contains("insufficient", response.Answer, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Insufficient evidence was found", response.Answer);
    }

    [Fact]
    public async Task AskAsync_WhenCandidatesBelowMinRelevanceScore_ShouldReportInsufficientEvidence()
    {
        var article = CreateArticle("LAN Switch Setup", "Setting up 24 port Cisco switch", "Configure VLAN 10 and 20.");
        await _articleRepo.AddAsync(article);
        await _semanticIndexService.IndexArticleAsync(article);

        var request = new RagQuestionRequest
        {
            Question = "Hydraulic pressure sensor replacement",
            MinRelevanceScore = 0.9 // Very high threshold
        };

        var response = await _ragService.AskAsync(request);

        Assert.False(response.IsSufficientEvidence);
        Assert.Empty(response.Evidence);
    }

    [Fact]
    public async Task AskAsync_WithProductFilter_ShouldOnlyCiteMatchingProductArticles()
    {
        var targetProduct = Guid.NewGuid();
        var otherProduct = Guid.NewGuid();

        var art1 = CreateArticle("Camera Troubleshooting", "ANPR camera guide", "Reset PoE power.", productId: targetProduct);
        var art2 = CreateArticle("Camera Troubleshooting", "Toll camera guide", "Reset PoE power.", productId: otherProduct);

        await _articleRepo.AddAsync(art1);
        await _articleRepo.AddAsync(art2);
        await _semanticIndexService.IndexArticleAsync(art1);
        await _semanticIndexService.IndexArticleAsync(art2);

        var response = await _ragService.AskAsync(new RagQuestionRequest
        {
            Question = "Camera Troubleshooting PoE power",
            ProductId = targetProduct
        });

        Assert.True(response.IsSufficientEvidence);
        Assert.All(response.Evidence, e => Assert.Equal(art1.Id, e.ReferenceId));
    }

    [Fact]
    public async Task GovernanceSafety_RagService_CannotCreateOrMutateKnowledgeArticles()
    {
        var article = CreateArticle("Existing Article", "Summary", "Content");
        await _articleRepo.AddAsync(article);
        await _semanticIndexService.IndexArticleAsync(article);

        var initialCount = (await _articleRepo.GetAllAsync()).Count;
        var initialVersion = article.UpdatedAt;

        // Ask a question through RAG
        var response = await _ragService.AskAsync(new RagQuestionRequest
        {
            Question = "Can AI publish new knowledge?"
        });

        var finalArticles = await _articleRepo.GetAllAsync();
        Assert.Equal(initialCount, finalArticles.Count);
        var unchangedArticle = await _articleRepo.GetByIdAsync(article.Id);
        Assert.Equal(initialVersion, unchangedArticle!.UpdatedAt);
    }

    [Fact]
    public async Task AskAsync_WithNullRequest_ShouldThrowValidationException()
    {
        await Assert.ThrowsAsync<ValidationException>(() => _ragService.AskAsync(null!));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task AskAsync_WithEmptyQuestion_ShouldThrowValidationException(string question)
    {
        var request = new RagQuestionRequest { Question = question };
        await Assert.ThrowsAsync<ValidationException>(() => _ragService.AskAsync(request));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(51)]
    public async Task AskAsync_WithInvalidMaxEvidence_ShouldThrowValidationException(int max)
    {
        var request = new RagQuestionRequest
        {
            Question = "Valid question",
            MaxEvidence = max
        };

        await Assert.ThrowsAsync<ValidationException>(() => _ragService.AskAsync(request));
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(1.1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public async Task AskAsync_WithInvalidMinRelevanceScore_ShouldThrowValidationException(double score)
    {
        var request = new RagQuestionRequest
        {
            Question = "Valid question",
            MinRelevanceScore = score
        };

        await Assert.ThrowsAsync<ValidationException>(() => _ragService.AskAsync(request));
    }

    [Fact]
    public async Task AskAsync_WithEmptyGuidFilter_ShouldThrowValidationException()
    {
        var request = new RagQuestionRequest
        {
            Question = "Valid question",
            ProductId = Guid.Empty
        };

        await Assert.ThrowsAsync<ValidationException>(() => _ragService.AskAsync(request));
    }

    private static KnowledgeArticle CreateArticle(
        string title,
        string summary,
        string content,
        Guid? productId = null,
        bool isApproved = true)
    {
        var slug = Slug.FromTitle(title + "-" + Guid.NewGuid().ToString("N")[..6]);
        var article = new KnowledgeArticle(
            Guid.NewGuid(),
            title,
            slug,
            summary,
            content,
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTimeOffset.UtcNow);

        if (productId.HasValue)
        {
            article.AddProduct(productId.Value);
        }

        if (isApproved)
        {
            article.SubmitForReview();
            article.Approve(Guid.NewGuid(), DateTimeOffset.UtcNow);
        }

        return article;
    }
}
