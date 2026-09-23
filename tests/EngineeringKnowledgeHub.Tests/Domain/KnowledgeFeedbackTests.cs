namespace EngineeringKnowledgeHub.Tests.Domain;

using EngineeringKnowledgeHub.Domain.Entities;
using EngineeringKnowledgeHub.Domain.Enums;
using EngineeringKnowledgeHub.Domain.Exceptions;
using Xunit;

public class KnowledgeFeedbackTests
{
    private readonly Guid _feedbackId = Guid.NewGuid();
    private readonly Guid _articleId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly DateTimeOffset _now = DateTimeOffset.UtcNow;

    [Fact]
    public void Constructor_With_Valid_Parameters_Should_Initialize_Properties()
    {
        var feedback = new KnowledgeFeedback(
            _feedbackId,
            _articleId,
            _userId,
            FeedbackType.Helpful,
            _now,
            "  Very clear instructions.  ");

        Assert.Equal(_feedbackId, feedback.Id);
        Assert.Equal(_articleId, feedback.ArticleId);
        Assert.Equal(_userId, feedback.UserId);
        Assert.Equal(FeedbackType.Helpful, feedback.FeedbackType);
        Assert.Equal(_now, feedback.CreatedAt);
        Assert.Equal("Very clear instructions.", feedback.Comment);
    }

    [Fact]
    public void Constructor_With_Empty_Id_Should_Throw_DomainRuleViolationException()
    {
        Assert.Throws<DomainRuleViolationException>(() =>
            new KnowledgeFeedback(Guid.Empty, _articleId, _userId, FeedbackType.Helpful, _now));
    }

    [Fact]
    public void Constructor_With_Empty_ArticleId_Should_Throw_DomainRuleViolationException()
    {
        Assert.Throws<DomainRuleViolationException>(() =>
            new KnowledgeFeedback(_feedbackId, Guid.Empty, _userId, FeedbackType.Helpful, _now));
    }

    [Fact]
    public void Constructor_With_Empty_UserId_Should_Throw_DomainRuleViolationException()
    {
        Assert.Throws<DomainRuleViolationException>(() =>
            new KnowledgeFeedback(_feedbackId, _articleId, Guid.Empty, FeedbackType.Helpful, _now));
    }

    [Fact]
    public void Constructor_With_Invalid_FeedbackType_Should_Throw_DomainRuleViolationException()
    {
        Assert.Throws<DomainRuleViolationException>(() =>
            new KnowledgeFeedback(_feedbackId, _articleId, _userId, (FeedbackType)999, _now));
    }
}
