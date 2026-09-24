namespace EngineeringKnowledgeHub.Tests.Application;

using EngineeringKnowledgeHub.Application.Common.Exceptions;
using EngineeringKnowledgeHub.Application.Governance;
using EngineeringKnowledgeHub.Domain.Constants;
using EngineeringKnowledgeHub.Domain.Entities;
using EngineeringKnowledgeHub.Domain.Enums;
using EngineeringKnowledgeHub.Domain.ValueObjects;
using EngineeringKnowledgeHub.Tests.Common;
using Xunit;

public class KnowledgeGovernanceServiceTests
{
    private readonly InMemoryKnowledgeVersionRepository _versionRepo = new();
    private readonly InMemoryAuditLogRepository _auditRepo = new();
    private readonly InMemoryKnowledgeArticleRepository _articleRepo = new();
    private readonly FakeDateTimeProvider _dateTimeProvider;
    private readonly KnowledgeGovernanceService _service;

    private readonly Guid _authorId = Guid.NewGuid();
    private readonly Guid _editorId = Guid.NewGuid();
    private readonly Guid _knowledgeTypeId = Guid.NewGuid();
    private readonly DateTimeOffset _initialTime = new(2026, 9, 24, 12, 0, 0, TimeSpan.Zero);

    public KnowledgeGovernanceServiceTests()
    {
        _dateTimeProvider = new FakeDateTimeProvider(_initialTime);
        _service = new KnowledgeGovernanceService(
            _versionRepo,
            _auditRepo,
            _articleRepo,
            _dateTimeProvider);
    }

    private KnowledgeArticle CreateSampleArticle(string title = "Calibration Guide", string content = "Original Content")
    {
        var article = new KnowledgeArticle(
            Guid.NewGuid(),
            title,
            Slug.FromTitle(title + " " + Guid.NewGuid().ToString("N")[..6]),
            "Summary",
            content,
            _knowledgeTypeId,
            _authorId,
            _initialTime);

        _articleRepo.AddAsync(article).Wait();
        return article;
    }

    [Fact]
    public async Task CreateVersion_WithAutomaticNumbering_ShouldStartAtOneAndIncrement()
    {
        var article = CreateSampleArticle();

        var v1 = await _service.CreateVersionAsync(new CreateKnowledgeVersionCommand(
            article.Id,
            _editorId,
            "Initial published snapshot"));

        Assert.Equal(1, v1.VersionNumber);
        Assert.Equal(article.Id, v1.ArticleId);
        Assert.Equal(article.Title, v1.Title);
        Assert.Equal(article.Content, v1.Content);
        Assert.Equal(_editorId, v1.ChangedById);
        Assert.Equal("Initial published snapshot", v1.ChangeSummary);
        Assert.Equal(_initialTime, v1.CreatedAt);

        // Advance time and create second version
        _dateTimeProvider.UtcNow = _initialTime.AddHours(2);
        var v2 = await _service.CreateVersionAsync(new CreateKnowledgeVersionCommand(
            article.Id,
            _editorId,
            "Second revision with updates",
            Content: "Updated Content"));

        Assert.Equal(2, v2.VersionNumber);
        Assert.Equal("Updated Content", v2.Content);
        Assert.Equal(_initialTime.AddHours(2), v2.CreatedAt);
    }

    [Fact]
    public async Task CreateVersion_ShouldRecordAuditLogAutomatically()
    {
        var article = CreateSampleArticle();

        var version = await _service.CreateVersionAsync(new CreateKnowledgeVersionCommand(
            article.Id,
            _editorId,
            "First release"));

        var logs = await _auditRepo.GetByEntityAsync(nameof(KnowledgeVersion), version.Id);
        Assert.Single(logs);

        var log = logs[0];
        Assert.Equal(_editorId, log.UserId);
        Assert.Equal(GovernanceAuditActions.KnowledgeVersionCreated, log.Action);
        Assert.Equal(nameof(KnowledgeVersion), log.EntityName);
        Assert.Equal(version.Id, log.EntityId);
        Assert.Contains("Version 1 created for article", log.Details);
        Assert.Equal(_initialTime, log.CreatedAt);
    }

    [Fact]
    public async Task CreateVersion_WhenArticleNotFound_ShouldThrowNotFoundException()
    {
        var missingArticleId = Guid.NewGuid();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.CreateVersionAsync(new CreateKnowledgeVersionCommand(
                missingArticleId,
                _editorId,
                "Snapshot")));
    }

    [Fact]
    public async Task CreateVersion_WithEmptyArticleId_ShouldThrowValidationException()
    {
        await Assert.ThrowsAsync<ValidationException>(() =>
            _service.CreateVersionAsync(new CreateKnowledgeVersionCommand(
                Guid.Empty,
                _editorId,
                "Snapshot")));
    }

    [Fact]
    public async Task CreateVersion_WithEmptyChangedById_ShouldThrowValidationException()
    {
        var article = CreateSampleArticle();

        await Assert.ThrowsAsync<ValidationException>(() =>
            _service.CreateVersionAsync(new CreateKnowledgeVersionCommand(
                article.Id,
                Guid.Empty,
                "Snapshot")));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateVersion_WithNullOrWhitespaceChangeSummary_ShouldThrowValidationException(string? invalidSummary)
    {
        var article = CreateSampleArticle();

        await Assert.ThrowsAsync<ValidationException>(() =>
            _service.CreateVersionAsync(new CreateKnowledgeVersionCommand(
                article.Id,
                _editorId,
                invalidSummary!)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task CreateVersion_WithZeroOrNegativeVersionNumber_ShouldThrowValidationException(int invalidNumber)
    {
        var article = CreateSampleArticle();

        await Assert.ThrowsAsync<ValidationException>(() =>
            _service.CreateVersionAsync(new CreateKnowledgeVersionCommand(
                article.Id,
                _editorId,
                "Snapshot",
                VersionNumber: invalidNumber)));
    }

    [Fact]
    public async Task CreateVersion_WithDuplicateVersionNumberForSameArticle_ShouldThrowConflictException()
    {
        var article = CreateSampleArticle();

        // Version 1 created explicitly
        await _service.CreateVersionAsync(new CreateKnowledgeVersionCommand(
            article.Id,
            _editorId,
            "First release",
            VersionNumber: 1));

        // Attempting to create Version 1 again for Article A -> Conflict
        var ex = await Assert.ThrowsAsync<ConflictException>(() =>
            _service.CreateVersionAsync(new CreateKnowledgeVersionCommand(
                article.Id,
                _editorId,
                "Duplicate version 1",
                VersionNumber: 1)));

        Assert.Contains("Version 1 already exists", ex.Message);
    }

    [Fact]
    public async Task CreateVersion_SameVersionNumberAcrossDifferentArticles_ShouldBothSucceed()
    {
        var articleA = CreateSampleArticle("Article Alpha");
        var articleB = CreateSampleArticle("Article Beta");

        var vA1 = await _service.CreateVersionAsync(new CreateKnowledgeVersionCommand(
            articleA.Id,
            _editorId,
            "Alpha Version 1",
            VersionNumber: 1));

        var vB1 = await _service.CreateVersionAsync(new CreateKnowledgeVersionCommand(
            articleB.Id,
            _editorId,
            "Beta Version 1",
            VersionNumber: 1));

        Assert.Equal(1, vA1.VersionNumber);
        Assert.Equal(1, vB1.VersionNumber);
        Assert.NotEqual(vA1.Id, vB1.Id);
        Assert.NotEqual(vA1.ArticleId, vB1.ArticleId);
    }

    [Fact]
    public async Task GetVersionHistory_ShouldReturnDeterministicOrder_VersionNumberDescending()
    {
        var article = CreateSampleArticle();

        _dateTimeProvider.UtcNow = _initialTime;
        await _service.CreateVersionAsync(new CreateKnowledgeVersionCommand(article.Id, _editorId, "V1", Content: "Content 1"));

        _dateTimeProvider.UtcNow = _initialTime.AddDays(1);
        await _service.CreateVersionAsync(new CreateKnowledgeVersionCommand(article.Id, _editorId, "V2", Content: "Content 2"));

        _dateTimeProvider.UtcNow = _initialTime.AddDays(2);
        await _service.CreateVersionAsync(new CreateKnowledgeVersionCommand(article.Id, _editorId, "V3", Content: "Content 3"));

        var history = await _service.GetVersionHistoryAsync(article.Id);

        Assert.Equal(3, history.Count);
        Assert.Equal(3, history[0].VersionNumber);
        Assert.Equal(2, history[1].VersionNumber);
        Assert.Equal(1, history[2].VersionNumber);
    }

    [Fact]
    public async Task GetVersionHistory_WhenArticleNotFound_ShouldThrowNotFoundException()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.GetVersionHistoryAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetVersionByNumber_WhenFound_ShouldReturnCorrectVersion()
    {
        var article = CreateSampleArticle();

        await _service.CreateVersionAsync(new CreateKnowledgeVersionCommand(article.Id, _editorId, "V1", Content: "Content 1"));
        await _service.CreateVersionAsync(new CreateKnowledgeVersionCommand(article.Id, _editorId, "V2", Content: "Content 2"));

        var result = await _service.GetVersionByNumberAsync(article.Id, 2);

        Assert.NotNull(result);
        Assert.Equal(2, result.VersionNumber);
        Assert.Equal("Content 2", result.Content);
    }

    [Fact]
    public async Task GetVersionByNumber_WhenNotFound_ShouldThrowNotFoundException()
    {
        var article = CreateSampleArticle();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.GetVersionByNumberAsync(article.Id, 99));
    }

    [Fact]
    public async Task GetVersionById_WhenFound_ShouldReturnCorrectVersion()
    {
        var article = CreateSampleArticle();

        var created = await _service.CreateVersionAsync(new CreateKnowledgeVersionCommand(article.Id, _editorId, "V1"));

        var fetched = await _service.GetVersionByIdAsync(created.Id);

        Assert.NotNull(fetched);
        Assert.Equal(created.Id, fetched.Id);
        Assert.Equal(created.VersionNumber, fetched.VersionNumber);
    }

    [Fact]
    public async Task GetVersionById_WhenNotFound_ShouldThrowNotFoundException()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.GetVersionByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task RecordAuditLog_WithValidCommand_ShouldPersistAndReturnDto()
    {
        var entityId = Guid.NewGuid();
        var command = new RecordAuditLogCommand(
            _editorId,
            "CustomGovernanceEvent",
            "KnowledgePolicy",
            entityId,
            "Policy evaluated successfully.");

        var result = await _service.RecordAuditLogAsync(command);

        Assert.NotNull(result);
        Assert.Equal(_editorId, result.UserId);
        Assert.Equal("CustomGovernanceEvent", result.Action);
        Assert.Equal("KnowledgePolicy", result.EntityName);
        Assert.Equal(entityId, result.EntityId);
        Assert.Equal("Policy evaluated successfully.", result.Details);
        Assert.Equal(_initialTime, result.CreatedAt);
    }

    [Fact]
    public async Task GetAuditLogsByEntity_ShouldReturnLogsInCreatedAtDescendingOrder()
    {
        var entityId = Guid.NewGuid();

        _dateTimeProvider.UtcNow = _initialTime;
        await _service.RecordAuditLogAsync(new RecordAuditLogCommand(_editorId, "Event1", "TestEntity", entityId, "Detail 1"));

        _dateTimeProvider.UtcNow = _initialTime.AddMinutes(10);
        await _service.RecordAuditLogAsync(new RecordAuditLogCommand(_editorId, "Event2", "TestEntity", entityId, "Detail 2"));

        _dateTimeProvider.UtcNow = _initialTime.AddMinutes(20);
        await _service.RecordAuditLogAsync(new RecordAuditLogCommand(_editorId, "Event3", "TestEntity", entityId, "Detail 3"));

        var logs = await _service.GetAuditLogsByEntityAsync("TestEntity", entityId);

        Assert.Equal(3, logs.Count);
        Assert.Equal("Event3", logs[0].Action);
        Assert.Equal("Event2", logs[1].Action);
        Assert.Equal("Event1", logs[2].Action);
    }

    [Fact]
    public async Task GetAuditLogsByUserId_ShouldReturnUserLogsInCreatedAtDescendingOrder()
    {
        var otherUser = Guid.NewGuid();

        _dateTimeProvider.UtcNow = _initialTime;
        await _service.RecordAuditLogAsync(new RecordAuditLogCommand(_editorId, "ActionA", "Entity1", Guid.NewGuid(), "A"));

        _dateTimeProvider.UtcNow = _initialTime.AddMinutes(5);
        await _service.RecordAuditLogAsync(new RecordAuditLogCommand(otherUser, "ActionOther", "Entity2", Guid.NewGuid(), "Other"));

        _dateTimeProvider.UtcNow = _initialTime.AddMinutes(10);
        await _service.RecordAuditLogAsync(new RecordAuditLogCommand(_editorId, "ActionB", "Entity3", Guid.NewGuid(), "B"));

        var userLogs = await _service.GetAuditLogsByUserIdAsync(_editorId);

        Assert.Equal(2, userLogs.Count);
        Assert.Equal("ActionB", userLogs[0].Action);
        Assert.Equal("ActionA", userLogs[1].Action);
    }
}
