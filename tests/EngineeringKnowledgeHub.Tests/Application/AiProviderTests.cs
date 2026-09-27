namespace EngineeringKnowledgeHub.Tests.Application;

using EngineeringKnowledgeHub.Application.Common.Exceptions;
using EngineeringKnowledgeHub.Application.Rag;
using Xunit;

public class AiProviderTests
{
    private readonly DeterministicAiProvider _provider = new();

    [Fact]
    public async Task GenerateAsync_WithValidCitationsPrompt_ShouldSynthesizeAnswerWithCitations()
    {
        var prompt =
            "Evidence Citations:\n\n" +
            "[1] (KnowledgeArticle) [Verified] Title: Camera Calibration\n" +
            "Content: Adjust lens aperture and set exposure to 1/1000s.\n\n" +
            "[2] (DocumentChunk) Title: Maintenance_Doc.pdf\n" +
            "Content: Clean lens surface with optical micro-fiber cloth.\n\n" +
            "Question: How to maintain camera calibration?\n" +
            "Instructions: Synthesize answer.";

        var request = new AiPromptRequest(
            SystemPrompt: "Answer based only on citations.",
            UserPrompt: prompt);

        var response = await _provider.GenerateAsync(request);

        Assert.NotNull(response);
        Assert.Equal("DeterministicLocalModel", response.ModelName);
        Assert.Contains("[1]", response.Content);
        Assert.Contains("[2]", response.Content);
        Assert.Contains("Camera Calibration", response.Content);
        Assert.True(response.PromptTokens > 0);
        Assert.True(response.CompletionTokens > 0);
    }

    [Fact]
    public async Task GenerateAsync_WithNoEvidenceToken_ShouldReturnInsufficientEvidenceNotice()
    {
        var request = new AiPromptRequest(
            SystemPrompt: "Answer strictly from evidence.",
            UserPrompt: "Question: What is secret algorithm?\nNO_EVIDENCE_AVAILABLE");

        var response = await _provider.GenerateAsync(request);

        Assert.Contains("insufficient evidence", response.Content, StringComparison.OrdinalIgnoreCase);
        Assert.True(response.CompletionTokens > 0);
    }

    [Fact]
    public async Task GenerateAsync_WithPromptWithoutCitations_ShouldReturnNoCitationsFallback()
    {
        var request = new AiPromptRequest(
            SystemPrompt: "Answer strictly from evidence.",
            UserPrompt: "Just a plain question without citations.");

        var response = await _provider.GenerateAsync(request);

        Assert.Contains("No relevant engineering evidence citations were provided", response.Content);
    }

    [Fact]
    public async Task GenerateAsync_WithNullRequest_ShouldThrowValidationException()
    {
        await Assert.ThrowsAsync<ValidationException>(() => _provider.GenerateAsync(null!));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GenerateAsync_WithEmptyUserPrompt_ShouldThrowValidationException(string userPrompt)
    {
        var request = new AiPromptRequest("System", userPrompt);
        await Assert.ThrowsAsync<ValidationException>(() => _provider.GenerateAsync(request));
    }

    [Fact]
    public async Task ProviderIndependence_CanSubstituteAlternativeProvider()
    {
        IAiProvider customProvider = new MockCustomAiProvider();

        var request = new AiPromptRequest("System", "User prompt");
        var response = await customProvider.GenerateAsync(request);

        Assert.Equal("MockLlmModel", response.ModelName);
        Assert.Equal("Custom generated response", response.Content);
    }

    private class MockCustomAiProvider : IAiProvider
    {
        public Task<AiResponse> GenerateAsync(AiPromptRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new AiResponse("Custom generated response", "MockLlmModel", 10, 10));
        }
    }
}
