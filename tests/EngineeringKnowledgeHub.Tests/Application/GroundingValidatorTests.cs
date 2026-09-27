namespace EngineeringKnowledgeHub.Tests.Application;

using EngineeringKnowledgeHub.Application.Rag;
using EngineeringKnowledgeHub.Domain.Enums;
using Xunit;

public class GroundingValidatorTests
{
    private readonly GroundingValidator _validator = new();

    private static List<RagEvidenceItem> CreateEvidence(params int[] citationNumbers)
    {
        return citationNumbers.Select(num => new RagEvidenceItem(
            CitationNumber: num,
            SourceType: "KnowledgeArticle",
            ReferenceId: Guid.NewGuid(),
            SecondaryId: null,
            Title: $"Evidence Article {num}",
            Content: $"Content details for article {num}",
            RelevanceScore: 0.8,
            VerificationStatus: VerificationStatus.Verified,
            Metadata: new Dictionary<string, string>())).ToList();
    }

    [Fact]
    public void ValidateGrounding_WithValidCitations_ShouldPass()
    {
        var evidence = CreateEvidence(1, 2);
        var content = "The issue is caused by component failure [1], and the resolution is documented in procedure [2].";

        var result = _validator.ValidateGrounding(content, evidence);

        Assert.True(result.IsGrounded);
        Assert.Null(result.FailureReason);
        Assert.Contains(1, result.CitedNumbers);
        Assert.Contains(2, result.CitedNumbers);
    }

    [Fact]
    public void ValidateGrounding_WithDuplicateValidCitations_ShouldPass()
    {
        var evidence = CreateEvidence(1);
        var content = "According to [1], check network settings. Furthermore, [1] recommends rebooting the switch.";

        var result = _validator.ValidateGrounding(content, evidence);

        Assert.True(result.IsGrounded);
        Assert.Null(result.FailureReason);
        Assert.Contains(1, result.CitedNumbers);
    }

    [Fact]
    public void ValidateGrounding_WithOutOfRangeCitation_ShouldFail()
    {
        var evidence = CreateEvidence(1, 2);
        var content = "The system solution is described in [999].";

        var result = _validator.ValidateGrounding(content, evidence);

        Assert.False(result.IsGrounded);
        Assert.Contains("[999]", result.FailureReason);
    }

    [Fact]
    public void ValidateGrounding_WithMixedValidAndInvalidCitations_ShouldFail()
    {
        var evidence = CreateEvidence(1, 2);
        var content = "Step one is verified in [1], but step two refers to [999].";

        var result = _validator.ValidateGrounding(content, evidence);

        Assert.False(result.IsGrounded);
        Assert.Contains("[999]", result.FailureReason);
    }

    [Fact]
    public void ValidateGrounding_WithNoCitationMarkers_ShouldFail()
    {
        var evidence = CreateEvidence(1, 2);
        var content = "This is a plausible sounding technical claim without any bracketed citations at all.";

        var result = _validator.ValidateGrounding(content, evidence);

        Assert.False(result.IsGrounded);
        Assert.Contains("does not cite any evidence", result.FailureReason);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateGrounding_WithNullOrWhitespaceContent_ShouldFail(string? content)
    {
        var evidence = CreateEvidence(1);
        var result = _validator.ValidateGrounding(content, evidence);

        Assert.False(result.IsGrounded);
    }

    [Fact]
    public void ValidateGrounding_WithEmptyEvidenceList_ShouldFail()
    {
        var content = "Some content citing [1]";
        var result = _validator.ValidateGrounding(content, Array.Empty<RagEvidenceItem>());

        Assert.False(result.IsGrounded);
    }
}
