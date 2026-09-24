namespace EngineeringKnowledgeHub.Tests.Domain;

using System.Reflection;
using EngineeringKnowledgeHub.Domain.Entities;
using EngineeringKnowledgeHub.Domain.Exceptions;
using Xunit;

public class KnowledgeVersionTests
{
    private readonly Guid _validId = Guid.NewGuid();
    private readonly Guid _validArticleId = Guid.NewGuid();
    private readonly Guid _validChangedById = Guid.NewGuid();
    private readonly DateTimeOffset _validCreatedAt = DateTimeOffset.UtcNow;

    [Fact]
    public void Constructor_WithValidArguments_ShouldInstantiate()
    {
        var version = new KnowledgeVersion(
            _validId,
            _validArticleId,
            1,
            "  Version 1 Title  ",
            "Initial published content.",
            _validChangedById,
            "  First release  ",
            _validCreatedAt);

        Assert.Equal(_validId, version.Id);
        Assert.Equal(_validArticleId, version.ArticleId);
        Assert.Equal(1, version.VersionNumber);
        Assert.Equal("Version 1 Title", version.Title);
        Assert.Equal("Initial published content.", version.Content);
        Assert.Equal(_validChangedById, version.ChangedById);
        Assert.Equal("First release", version.ChangeSummary);
        Assert.Equal(_validCreatedAt, version.CreatedAt);
    }

    [Fact]
    public void Constructor_WithNullChangeSummary_ShouldDefaultToEmptyString()
    {
        var version = new KnowledgeVersion(
            _validId,
            _validArticleId,
            1,
            "Title",
            "Content",
            _validChangedById,
            null!,
            _validCreatedAt);

        Assert.Equal(string.Empty, version.ChangeSummary);
    }

    [Fact]
    public void Constructor_WithEmptyId_ShouldThrow()
    {
        Assert.Throws<DomainRuleViolationException>(() =>
            new KnowledgeVersion(
                Guid.Empty,
                _validArticleId,
                1,
                "Title",
                "Content",
                _validChangedById,
                "Summary",
                _validCreatedAt));
    }

    [Fact]
    public void Constructor_WithEmptyArticleId_ShouldThrow()
    {
        Assert.Throws<DomainRuleViolationException>(() =>
            new KnowledgeVersion(
                _validId,
                Guid.Empty,
                1,
                "Title",
                "Content",
                _validChangedById,
                "Summary",
                _validCreatedAt));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Constructor_WithZeroOrNegativeVersionNumber_ShouldThrow(int invalidVersionNumber)
    {
        var ex = Assert.Throws<DomainRuleViolationException>(() =>
            new KnowledgeVersion(
                _validId,
                _validArticleId,
                invalidVersionNumber,
                "Title",
                "Content",
                _validChangedById,
                "Summary",
                _validCreatedAt));

        Assert.Contains("Version number must be greater than zero", ex.Message);
    }

    [Fact]
    public void Constructor_WithEmptyChangedById_ShouldThrow()
    {
        Assert.Throws<DomainRuleViolationException>(() =>
            new KnowledgeVersion(
                _validId,
                _validArticleId,
                1,
                "Title",
                "Content",
                Guid.Empty,
                "Summary",
                _validCreatedAt));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithNullOrWhitespaceTitle_ShouldThrow(string? invalidTitle)
    {
        Assert.Throws<DomainRuleViolationException>(() =>
            new KnowledgeVersion(
                _validId,
                _validArticleId,
                1,
                invalidTitle!,
                "Content",
                _validChangedById,
                "Summary",
                _validCreatedAt));
    }

    [Fact]
    public void Constructor_WithTitleExceedingMaxLength_ShouldThrow()
    {
        var longTitle = new string('x', 201);

        var ex = Assert.Throws<DomainRuleViolationException>(() =>
            new KnowledgeVersion(
                _validId,
                _validArticleId,
                1,
                longTitle,
                "Content",
                _validChangedById,
                "Summary",
                _validCreatedAt));

        Assert.Contains("exceeds maximum length", ex.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithNullOrWhitespaceContent_ShouldThrow(string? invalidContent)
    {
        Assert.Throws<DomainRuleViolationException>(() =>
            new KnowledgeVersion(
                _validId,
                _validArticleId,
                1,
                "Title",
                invalidContent!,
                _validChangedById,
                "Summary",
                _validCreatedAt));
    }

    [Fact]
    public void KnowledgeVersion_ShouldBeImmutable_NoPublicSetters()
    {
        var publicSetters = typeof(KnowledgeVersion)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.SetMethod is not null && p.SetMethod.IsPublic);

        Assert.Empty(publicSetters);
    }
}
