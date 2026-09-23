namespace EngineeringKnowledgeHub.Tests.Application;

using EngineeringKnowledgeHub.Application.Common.Exceptions;
using EngineeringKnowledgeHub.Application.Feedback;
using EngineeringKnowledgeHub.Domain.Entities;
using EngineeringKnowledgeHub.Domain.Enums;
using EngineeringKnowledgeHub.Domain.ValueObjects;
using EngineeringKnowledgeHub.Tests.Common;
using Xunit;

public class KnowledgeFeedbackServiceTests
{
    private readonly InMemoryKnowledgeFeedbackRepository _feedbackRepository = new();
    private readonly InMemoryKnowledgeArticleRepository _articleRepository = new();
    private readonly FakeDateTimeProvider _dateTimeProvider;
    private readonly KnowledgeFeedbackService _service;

    private readonly Guid _authorId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _knowledgeTypeId = Guid.NewGuid();
    private readonly DateTimeOffset _initialTime = new(2026, 3, 15, 10, 0, 0, TimeSpan.Zero);

    public KnowledgeFeedbackServiceTests()
    {
        _dateTimeProvider = new FakeDateTimeProvider(_initialTime);
        _service = new KnowledgeFeedbackService(_feedbackRepository, _articleRepository, _dateTimeProvider);
    }

    private async Task<KnowledgeArticle> CreatePublishedArticleAsync(string title = "Sample Published Article")
    {
        var article = new KnowledgeArticle(
            Guid.NewGuid(),
            title,
            Slug.FromTitle(title),
            "Summary",
            "Content",
            _knowledgeTypeId,
            _authorId,
            _initialTime);

        // Legitimate domain lifecycle: Draft -> PendingReview -> Published (with reviewer != author)
        article.SubmitForReview();
        var reviewerId = Guid.NewGuid();
        article.Approve(reviewerId, _initialTime.AddMinutes(30));

        await _articleRepository.AddAsync(article);
        return article;
    }

    private async Task<KnowledgeArticle> CreateDraftArticleAsync(string title = "Sample Draft Article")
    {
        var article = new KnowledgeArticle(
            Guid.NewGuid(),
            title,
            Slug.FromTitle(title),
            "Summary",
            "Content",
            _knowledgeTypeId,
            _authorId,
            _initialTime);

        await _articleRepository.AddAsync(article);
        return article;
    }

    private async Task<KnowledgeArticle> CreatePendingReviewArticleAsync(string title = "Sample PendingReview Article")
    {
        var article = new KnowledgeArticle(
            Guid.NewGuid(),
            title,
            Slug.FromTitle(title),
            "Summary",
            "Content",
            _knowledgeTypeId,
            _authorId,
            _initialTime);

        article.SubmitForReview();

        await _articleRepository.AddAsync(article);
        return article;
    }

    private async Task<KnowledgeArticle> CreateDeprecatedArticleAsync(string title = "Sample Deprecated Article")
    {
        var article = await CreatePublishedArticleAsync(title);
        article.Deprecate();
        await _articleRepository.UpdateAsync(article);
        return article;
    }

    private async Task<KnowledgeArticle> CreateArchivedArticleAsync(string title = "Sample Archived Article")
    {
        var article = await CreateDraftArticleAsync(title);
        article.Archive();
        await _articleRepository.UpdateAsync(article);
        return article;
    }

    #region Published Status Validation Tests

    [Fact]
    public async Task SubmitFeedbackAsync_When_Article_Is_Published_Should_Succeed_And_Persist_Feedback()
    {
        var article = await CreatePublishedArticleAsync();
        var command = new SubmitFeedbackCommand(article.Id, _userId, FeedbackType.Helpful, "Great troubleshooting steps.");

        var result = await _service.SubmitFeedbackAsync(command);

        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(article.Id, result.ArticleId);
        Assert.Equal(_userId, result.UserId);
        Assert.Equal(FeedbackType.Helpful, result.FeedbackType);
        Assert.Equal("Great troubleshooting steps.", result.Comment);
        Assert.Equal(_initialTime, result.CreatedAt);

        var persisted = await _feedbackRepository.GetByArticleIdAsync(article.Id);
        Assert.Single(persisted);
        Assert.Equal(result.Id, persisted[0].Id);
    }

    [Fact]
    public async Task SubmitFeedbackAsync_When_Article_Is_Draft_Should_Throw_ValidationException_And_Not_Persist()
    {
        var article = await CreateDraftArticleAsync();
        var command = new SubmitFeedbackCommand(article.Id, _userId, FeedbackType.Helpful, "Premature feedback");

        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            _service.SubmitFeedbackAsync(command));

        Assert.Contains("published", ex.Message, StringComparison.OrdinalIgnoreCase);

        var persisted = await _feedbackRepository.GetByArticleIdAsync(article.Id);
        Assert.Empty(persisted);
    }

    [Fact]
    public async Task SubmitFeedbackAsync_When_Article_Is_PendingReview_Should_Throw_ValidationException_And_Not_Persist()
    {
        var article = await CreatePendingReviewArticleAsync();
        var command = new SubmitFeedbackCommand(article.Id, _userId, FeedbackType.Helpful, "In-review feedback");

        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            _service.SubmitFeedbackAsync(command));

        Assert.Contains("published", ex.Message, StringComparison.OrdinalIgnoreCase);

        var persisted = await _feedbackRepository.GetByArticleIdAsync(article.Id);
        Assert.Empty(persisted);
    }

    [Fact]
    public async Task SubmitFeedbackAsync_When_Article_Is_Deprecated_Should_Throw_ValidationException_And_Not_Persist()
    {
        var article = await CreateDeprecatedArticleAsync();
        var command = new SubmitFeedbackCommand(article.Id, _userId, FeedbackType.Outdated, "Deprecated article feedback");

        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            _service.SubmitFeedbackAsync(command));

        Assert.Contains("published", ex.Message, StringComparison.OrdinalIgnoreCase);

        var persisted = await _feedbackRepository.GetByArticleIdAsync(article.Id);
        Assert.Empty(persisted);
    }

    [Fact]
    public async Task SubmitFeedbackAsync_When_Article_Is_Archived_Should_Throw_ValidationException_And_Not_Persist()
    {
        var article = await CreateArchivedArticleAsync();
        var command = new SubmitFeedbackCommand(article.Id, _userId, FeedbackType.Inaccurate, "Archived article feedback");

        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            _service.SubmitFeedbackAsync(command));

        Assert.Contains("published", ex.Message, StringComparison.OrdinalIgnoreCase);

        var persisted = await _feedbackRepository.GetByArticleIdAsync(article.Id);
        Assert.Empty(persisted);
    }

    #endregion

    #region Input Validation & Retrieval Tests

    [Fact]
    public async Task SubmitFeedbackAsync_With_Null_Command_Should_Throw_ValidationException()
    {
        await Assert.ThrowsAsync<ValidationException>(() =>
            _service.SubmitFeedbackAsync(null!));
    }

    [Fact]
    public async Task SubmitFeedbackAsync_With_Empty_ArticleId_Should_Throw_ValidationException()
    {
        var command = new SubmitFeedbackCommand(Guid.Empty, _userId, FeedbackType.Helpful);

        await Assert.ThrowsAsync<ValidationException>(() =>
            _service.SubmitFeedbackAsync(command));
    }

    [Fact]
    public async Task SubmitFeedbackAsync_With_Empty_UserId_Should_Throw_ValidationException()
    {
        var command = new SubmitFeedbackCommand(Guid.NewGuid(), Guid.Empty, FeedbackType.Helpful);

        await Assert.ThrowsAsync<ValidationException>(() =>
            _service.SubmitFeedbackAsync(command));
    }

    [Fact]
    public async Task SubmitFeedbackAsync_With_Invalid_FeedbackType_Should_Throw_ValidationException()
    {
        var command = new SubmitFeedbackCommand(Guid.NewGuid(), _userId, (FeedbackType)999);

        await Assert.ThrowsAsync<ValidationException>(() =>
            _service.SubmitFeedbackAsync(command));
    }

    [Fact]
    public async Task SubmitFeedbackAsync_When_Article_Not_Found_Should_Throw_NotFoundException()
    {
        var nonExistentId = Guid.NewGuid();
        var command = new SubmitFeedbackCommand(nonExistentId, _userId, FeedbackType.Helpful);

        var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.SubmitFeedbackAsync(command));

        Assert.Contains(nonExistentId.ToString(), ex.Message);
    }

    [Fact]
    public async Task GetFeedbackForArticleAsync_Should_Return_Feedback_For_Specified_Article()
    {
        var article1 = await CreatePublishedArticleAsync("Published Article One");
        var article2 = await CreatePublishedArticleAsync("Published Article Two");

        await _service.SubmitFeedbackAsync(new SubmitFeedbackCommand(article1.Id, _userId, FeedbackType.Helpful, "Helpful"));
        await _service.SubmitFeedbackAsync(new SubmitFeedbackCommand(article1.Id, Guid.NewGuid(), FeedbackType.Outdated, "Needs update"));
        await _service.SubmitFeedbackAsync(new SubmitFeedbackCommand(article2.Id, _userId, FeedbackType.Inaccurate, "Wrong steps"));

        var feedback1 = await _service.GetFeedbackForArticleAsync(article1.Id);
        var feedback2 = await _service.GetFeedbackForArticleAsync(article2.Id);

        Assert.Equal(2, feedback1.Count);
        Assert.Single(feedback2);
        Assert.All(feedback1, f => Assert.Equal(article1.Id, f.ArticleId));
        Assert.All(feedback2, f => Assert.Equal(article2.Id, f.ArticleId));
    }

    [Fact]
    public async Task GetFeedbackForArticleAsync_With_Empty_ArticleId_Should_Throw_ValidationException()
    {
        await Assert.ThrowsAsync<ValidationException>(() =>
            _service.GetFeedbackForArticleAsync(Guid.Empty));
    }

    #endregion
}
