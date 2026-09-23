namespace EngineeringKnowledgeHub.Tests.Domain;

using EngineeringKnowledgeHub.Domain.Entities;
using EngineeringKnowledgeHub.Domain.Enums;
using EngineeringKnowledgeHub.Domain.Exceptions;
using EngineeringKnowledgeHub.Domain.ValueObjects;
using Xunit;

public class KnowledgeArticleTests
{
    private readonly Guid _authorId = Guid.NewGuid();
    private readonly Guid _reviewerId = Guid.NewGuid();
    private readonly Guid _knowledgeTypeId = Guid.NewGuid();
    private readonly DateTimeOffset _now = DateTimeOffset.UtcNow;

    private KnowledgeArticle CreateValidArticle()
    {
        return new KnowledgeArticle(
            Guid.NewGuid(),
            "Camera Calibration Procedure",
            Slug.Create("camera-calibration-procedure"),
            "Step-by-step camera setup guide.",
            "# Steps for camera setup...",
            _knowledgeTypeId,
            _authorId,
            _now);
    }

    [Fact]
    public void Constructor_With_Valid_Parameters_Should_Initialize_In_Draft_State()
    {
        var article = CreateValidArticle();

        Assert.Equal(KnowledgeStatus.Draft, article.Status);
        Assert.Equal(VerificationStatus.Unverified, article.VerificationStatus);
        Assert.Null(article.ReviewerId);
        Assert.Null(article.PublishedAt);
        Assert.Null(article.LastReviewedAt);
        Assert.Equal(_authorId, article.AuthorId);
        Assert.Empty(article.TagIds);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_With_Empty_Title_Should_Throw_DomainRuleViolationException(string? title)
    {
        Assert.Throws<DomainRuleViolationException>(() => new KnowledgeArticle(
            Guid.NewGuid(),
            title!,
            Slug.Create("valid-slug"),
            "Summary",
            "Content",
            _knowledgeTypeId,
            _authorId,
            _now));
    }

    [Fact]
    public void SubmitForReview_When_In_Draft_Should_Transition_To_PendingReview()
    {
        var article = CreateValidArticle();

        article.SubmitForReview();

        Assert.Equal(KnowledgeStatus.PendingReview, article.Status);
    }

    [Fact]
    public void SubmitForReview_When_Already_PendingReview_Should_Throw_DomainRuleViolationException()
    {
        var article = CreateValidArticle();
        article.SubmitForReview();

        Assert.Throws<DomainRuleViolationException>(() => article.SubmitForReview());
    }

    [Fact]
    public void Approve_By_Reviewer_Should_Publish_And_Verify()
    {
        var article = CreateValidArticle();
        article.SubmitForReview();
        var reviewedAt = _now.AddHours(2);

        article.Approve(_reviewerId, reviewedAt);

        Assert.Equal(KnowledgeStatus.Published, article.Status);
        Assert.Equal(VerificationStatus.Verified, article.VerificationStatus);
        Assert.Equal(_reviewerId, article.ReviewerId);
        Assert.Equal(reviewedAt, article.PublishedAt);
        Assert.Equal(reviewedAt, article.LastReviewedAt);
    }

    [Fact]
    public void Approve_By_Author_Must_Throw_DomainRuleViolationException()
    {
        var article = CreateValidArticle();
        article.SubmitForReview();

        // An author cannot approve their own knowledge article
        var ex = Assert.Throws<DomainRuleViolationException>(() => article.Approve(_authorId, _now.AddHours(1)));
        Assert.Contains("Author cannot approve their own knowledge article", ex.Message);
    }

    [Fact]
    public void Reject_By_Reviewer_Should_Return_To_Draft_And_Set_Rejected()
    {
        var article = CreateValidArticle();
        article.SubmitForReview();
        var reviewedAt = _now.AddHours(1);

        article.Reject(_reviewerId, reviewedAt);

        Assert.Equal(KnowledgeStatus.Draft, article.Status);
        Assert.Equal(VerificationStatus.Rejected, article.VerificationStatus);
        Assert.Equal(_reviewerId, article.ReviewerId);
        Assert.Equal(reviewedAt, article.LastReviewedAt);
    }

    [Fact]
    public void UpdateContent_On_Published_Article_Should_Revert_To_Draft_For_ReReview()
    {
        var article = CreateValidArticle();
        article.SubmitForReview();
        article.Approve(_reviewerId, _now.AddHours(1));
        Assert.Equal(KnowledgeStatus.Published, article.Status);

        var updateTime = _now.AddDays(1);
        article.UpdateContent(
            "Updated Procedure",
            Slug.Create("updated-procedure"),
            "Updated summary",
            "Updated content with new steps",
            updateTime);

        Assert.Equal(KnowledgeStatus.Draft, article.Status);
        Assert.Equal(VerificationStatus.Unverified, article.VerificationStatus);
        Assert.Equal(updateTime, article.UpdatedAt);
    }

    [Fact]
    public void Deprecate_When_Published_Should_Succeed()
    {
        var article = CreateValidArticle();
        article.SubmitForReview();
        article.Approve(_reviewerId, _now.AddHours(1));

        article.Deprecate();

        Assert.Equal(KnowledgeStatus.Deprecated, article.Status);
    }

    [Fact]
    public void Deprecate_When_Draft_Should_Throw_DomainRuleViolationException()
    {
        var article = CreateValidArticle();

        Assert.Throws<DomainRuleViolationException>(() => article.Deprecate());
    }

    [Fact]
    public void AddTag_And_RemoveTag_Should_Manage_Tags_Correctly()
    {
        var article = CreateValidArticle();
        var tagId = Guid.NewGuid();

        article.AddTag(tagId);
        Assert.Contains(tagId, article.TagIds);

        article.RemoveTag(tagId);
        Assert.DoesNotContain(tagId, article.TagIds);
    }

    [Fact]
    public void AddProduct_And_RemoveProduct_Should_Manage_Products_Correctly()
    {
        var article = CreateValidArticle();
        var productId = Guid.NewGuid();

        article.AddProduct(productId);
        Assert.Contains(productId, article.ProductIds);

        article.RemoveProduct(productId);
        Assert.DoesNotContain(productId, article.ProductIds);
    }

    [Fact]
    public void Approve_When_In_Draft_Should_Throw_DomainRuleViolationException()
    {
        var article = CreateValidArticle();
        Assert.Equal(KnowledgeStatus.Draft, article.Status);

        Assert.Throws<DomainRuleViolationException>(() => article.Approve(_reviewerId, _now));
    }

    [Fact]
    public void Reject_When_In_Draft_Should_Throw_DomainRuleViolationException()
    {
        var article = CreateValidArticle();
        Assert.Equal(KnowledgeStatus.Draft, article.Status);

        Assert.Throws<DomainRuleViolationException>(() => article.Reject(_reviewerId, _now));
    }

    [Fact]
    public void Approve_With_Empty_ReviewerId_Should_Throw_DomainRuleViolationException()
    {
        var article = CreateValidArticle();
        article.SubmitForReview();

        Assert.Throws<DomainRuleViolationException>(() => article.Approve(Guid.Empty, _now));
    }

    [Fact]
    public void Reject_With_Empty_ReviewerId_Should_Throw_DomainRuleViolationException()
    {
        var article = CreateValidArticle();
        article.SubmitForReview();

        Assert.Throws<DomainRuleViolationException>(() => article.Reject(Guid.Empty, _now));
    }

    [Fact]
    public void Approve_When_Already_Published_Should_Throw_DomainRuleViolationException()
    {
        var article = CreateValidArticle();
        article.SubmitForReview();
        article.Approve(_reviewerId, _now);
        Assert.Equal(KnowledgeStatus.Published, article.Status);

        var anotherReviewer = Guid.NewGuid();
        Assert.Throws<DomainRuleViolationException>(() => article.Approve(anotherReviewer, _now.AddHours(1)));
    }

    [Fact]
    public void Reject_When_Already_Published_Should_Throw_DomainRuleViolationException()
    {
        var article = CreateValidArticle();
        article.SubmitForReview();
        article.Approve(_reviewerId, _now);
        Assert.Equal(KnowledgeStatus.Published, article.Status);

        var anotherReviewer = Guid.NewGuid();
        Assert.Throws<DomainRuleViolationException>(() => article.Reject(anotherReviewer, _now.AddHours(1)));
    }

    [Fact]
    public void SubmitForReview_When_Already_Published_Should_Throw_DomainRuleViolationException()
    {
        var article = CreateValidArticle();
        article.SubmitForReview();
        article.Approve(_reviewerId, _now);
        Assert.Equal(KnowledgeStatus.Published, article.Status);

        Assert.Throws<DomainRuleViolationException>(() => article.SubmitForReview());
    }

    [Fact]
    public void Deprecate_When_PendingReview_Should_Throw_DomainRuleViolationException()
    {
        var article = CreateValidArticle();
        article.SubmitForReview();

        Assert.Throws<DomainRuleViolationException>(() => article.Deprecate());
    }

    [Fact]
    public void Deprecate_When_Already_Deprecated_Should_Throw_DomainRuleViolationException()
    {
        var article = CreateValidArticle();
        article.SubmitForReview();
        article.Approve(_reviewerId, _now);
        article.Deprecate();
        Assert.Equal(KnowledgeStatus.Deprecated, article.Status);

        Assert.Throws<DomainRuleViolationException>(() => article.Deprecate());
    }

    [Fact]
    public void Archive_Should_Transition_To_Archived()
    {
        var article = CreateValidArticle();
        article.Archive();

        Assert.Equal(KnowledgeStatus.Archived, article.Status);
    }

    [Fact]
    public void SubmitForReview_When_Archived_Should_Throw_DomainRuleViolationException()
    {
        var article = CreateValidArticle();
        article.Archive();

        Assert.Throws<DomainRuleViolationException>(() => article.SubmitForReview());
    }

    [Fact]
    public void Approve_When_Archived_Should_Throw_DomainRuleViolationException()
    {
        var article = CreateValidArticle();
        article.Archive();

        Assert.Throws<DomainRuleViolationException>(() => article.Approve(_reviewerId, _now));
    }

    [Fact]
    public void Reject_When_Archived_Should_Throw_DomainRuleViolationException()
    {
        var article = CreateValidArticle();
        article.Archive();

        Assert.Throws<DomainRuleViolationException>(() => article.Reject(_reviewerId, _now));
    }

    [Fact]
    public void Deprecate_When_Archived_Should_Throw_DomainRuleViolationException()
    {
        var article = CreateValidArticle();
        article.Archive();

        Assert.Throws<DomainRuleViolationException>(() => article.Deprecate());
    }

    [Fact]
    public void Approve_When_Deprecated_Should_Throw_DomainRuleViolationException()
    {
        var article = CreateValidArticle();
        article.SubmitForReview();
        article.Approve(_reviewerId, _now);
        article.Deprecate();

        Assert.Throws<DomainRuleViolationException>(() => article.Approve(_reviewerId, _now));
    }

    [Fact]
    public void Reject_When_Deprecated_Should_Throw_DomainRuleViolationException()
    {
        var article = CreateValidArticle();
        article.SubmitForReview();
        article.Approve(_reviewerId, _now);
        article.Deprecate();

        Assert.Throws<DomainRuleViolationException>(() => article.Reject(_reviewerId, _now));
    }
}
