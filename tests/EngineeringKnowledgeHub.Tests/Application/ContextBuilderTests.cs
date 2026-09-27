namespace EngineeringKnowledgeHub.Tests.Application;

using EngineeringKnowledgeHub.Application.Common.Exceptions;
using EngineeringKnowledgeHub.Application.Rag;
using EngineeringKnowledgeHub.Domain.Enums;
using Xunit;

public class ContextBuilderTests
{
    private readonly ContextBuilder _builder = new();

    [Fact]
    public void BuildContext_WithValidEvidence_ShouldFormatCitationsAndStructurePrompts()
    {
        var evidence = new List<RagEvidenceItem>
        {
            new(
                CitationNumber: 1,
                SourceType: "KnowledgeArticle",
                ReferenceId: Guid.NewGuid(),
                SecondaryId: null,
                Title: "Camera Calibration Guide",
                Content: "Adjust focal length to 12mm and set exposure to 1/1000s.",
                RelevanceScore: 0.85,
                VerificationStatus: VerificationStatus.Verified,
                Metadata: new Dictionary<string, string>()),
            new(
                CitationNumber: 2,
                SourceType: "DocumentChunk",
                ReferenceId: Guid.NewGuid(),
                SecondaryId: Guid.NewGuid(),
                Title: "Camera_Specs.pdf",
                Content: "Operating temperature is -20C to +60C.",
                RelevanceScore: 0.75,
                VerificationStatus: null,
                Metadata: new Dictionary<string, string>())
        };

        var context = _builder.BuildContext("How to calibrate camera?", evidence);

        Assert.True(context.HasSufficientEvidence);
        Assert.Equal(2, context.EvidenceItems.Count);
        Assert.Contains("[1] (KnowledgeArticle) [Verified] Title: Camera Calibration Guide", context.UserPrompt);
        Assert.Contains("[2] (DocumentChunk) Title: Camera_Specs.pdf", context.UserPrompt);
        Assert.Contains("Question: How to calibrate camera?", context.UserPrompt);
        Assert.Contains("grounded in the provided engineering evidence citations", context.SystemPrompt);
        Assert.True(context.EstimatedTokens > 0);
    }

    [Fact]
    public void BuildContext_WithEmptyEvidence_ShouldMarkInsufficientEvidence()
    {
        var context = _builder.BuildContext("Unknown technical question", Array.Empty<RagEvidenceItem>());

        Assert.False(context.HasSufficientEvidence);
        Assert.Empty(context.EvidenceItems);
        Assert.Contains("NO_EVIDENCE_AVAILABLE", context.UserPrompt);
    }

    [Fact]
    public void BuildContext_WithEvidenceBelowThreshold_ShouldFilterAndMarkInsufficient()
    {
        var evidence = new List<RagEvidenceItem>
        {
            new(
                CitationNumber: 1,
                SourceType: "KnowledgeArticle",
                ReferenceId: Guid.NewGuid(),
                SecondaryId: null,
                Title: "Unrelated topic",
                Content: "Content with low relevance score.",
                RelevanceScore: 0.1,
                VerificationStatus: VerificationStatus.Unverified,
                Metadata: new Dictionary<string, string>())
        };

        var options = new RagContextOptions
        {
            InsufficientEvidenceThreshold = 0.5
        };

        var context = _builder.BuildContext("Query with low relevance", evidence, options);

        Assert.False(context.HasSufficientEvidence);
        Assert.Empty(context.EvidenceItems);
    }

    [Fact]
    public void BuildContext_WithRestrictedTokenBudget_ShouldTruncateContentGracefully()
    {
        var largeContent = new string('A', 5000);
        var evidence = new List<RagEvidenceItem>
        {
            new(
                CitationNumber: 1,
                SourceType: "KnowledgeArticle",
                ReferenceId: Guid.NewGuid(),
                SecondaryId: null,
                Title: "Big Article",
                Content: largeContent,
                RelevanceScore: 0.9,
                VerificationStatus: VerificationStatus.Verified,
                Metadata: new Dictionary<string, string>())
        };

        var options = new RagContextOptions
        {
            MaxTokens = 200 // ~800 chars limit
        };

        var context = _builder.BuildContext("Test question", evidence, options);

        Assert.True(context.HasSufficientEvidence);
        Assert.Single(context.EvidenceItems);
        Assert.Contains("...", context.UserPrompt);
        Assert.True(context.EstimatedTokens <= 250);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void BuildContext_WithNullOrEmptyQuestion_ShouldThrowValidationException(string question)
    {
        Assert.Throws<ValidationException>(() => _builder.BuildContext(question, Array.Empty<RagEvidenceItem>()));
    }
}
