namespace EngineeringKnowledgeHub.Tests.Domain;

using System.Reflection;
using EngineeringKnowledgeHub.Domain.Entities;
using EngineeringKnowledgeHub.Domain.Exceptions;
using Xunit;

public class AuditLogTests
{
    private readonly Guid _validId = Guid.NewGuid();
    private readonly Guid _validUserId = Guid.NewGuid();
    private readonly Guid _validEntityId = Guid.NewGuid();
    private readonly DateTimeOffset _validCreatedAt = DateTimeOffset.UtcNow;

    [Fact]
    public void Constructor_WithValidArguments_ShouldInstantiate()
    {
        var audit = new AuditLog(
            _validId,
            _validUserId,
            "  ArticleApproved  ",
            "  KnowledgeArticle  ",
            _validEntityId,
            "  Approved by lead reviewer  ",
            _validCreatedAt);

        Assert.Equal(_validId, audit.Id);
        Assert.Equal(_validUserId, audit.UserId);
        Assert.Equal("ArticleApproved", audit.Action);
        Assert.Equal("KnowledgeArticle", audit.EntityName);
        Assert.Equal(_validEntityId, audit.EntityId);
        Assert.Equal("Approved by lead reviewer", audit.Details);
        Assert.Equal(_validCreatedAt, audit.CreatedAt);
    }

    [Fact]
    public void Constructor_WithNullDetails_ShouldDefaultToEmptyString()
    {
        var audit = new AuditLog(
            _validId,
            _validUserId,
            "ArticleApproved",
            "KnowledgeArticle",
            _validEntityId,
            null!,
            _validCreatedAt);

        Assert.Equal(string.Empty, audit.Details);
    }

    [Fact]
    public void Constructor_WithEmptyId_ShouldThrow()
    {
        Assert.Throws<DomainRuleViolationException>(() =>
            new AuditLog(
                Guid.Empty,
                _validUserId,
                "ArticleApproved",
                "KnowledgeArticle",
                _validEntityId,
                "Details",
                _validCreatedAt));
    }

    [Fact]
    public void Constructor_WithEmptyUserId_ShouldThrow()
    {
        Assert.Throws<DomainRuleViolationException>(() =>
            new AuditLog(
                _validId,
                Guid.Empty,
                "ArticleApproved",
                "KnowledgeArticle",
                _validEntityId,
                "Details",
                _validCreatedAt));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithNullOrWhitespaceAction_ShouldThrow(string? invalidAction)
    {
        Assert.Throws<DomainRuleViolationException>(() =>
            new AuditLog(
                _validId,
                _validUserId,
                invalidAction!,
                "KnowledgeArticle",
                _validEntityId,
                "Details",
                _validCreatedAt));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithNullOrWhitespaceEntityName_ShouldThrow(string? invalidEntityName)
    {
        Assert.Throws<DomainRuleViolationException>(() =>
            new AuditLog(
                _validId,
                _validUserId,
                "ArticleApproved",
                invalidEntityName!,
                _validEntityId,
                "Details",
                _validCreatedAt));
    }

    [Fact]
    public void Constructor_WithEmptyEntityId_ShouldThrow()
    {
        Assert.Throws<DomainRuleViolationException>(() =>
            new AuditLog(
                _validId,
                _validUserId,
                "ArticleApproved",
                "KnowledgeArticle",
                Guid.Empty,
                "Details",
                _validCreatedAt));
    }

    [Fact]
    public void AuditLog_ShouldBeImmutable_NoPublicSetters()
    {
        var publicSetters = typeof(AuditLog)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.SetMethod is not null && p.SetMethod.IsPublic);

        Assert.Empty(publicSetters);
    }
}
