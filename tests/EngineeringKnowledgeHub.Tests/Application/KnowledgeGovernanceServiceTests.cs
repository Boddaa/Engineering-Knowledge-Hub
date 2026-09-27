namespace EngineeringKnowledgeHub.Tests.Application;

using EngineeringKnowledgeHub.Application.Common.Exceptions;
using EngineeringKnowledgeHub.Application.Governance;
using EngineeringKnowledgeHub.Domain.Entities;
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
    public async Task GetVersionHistory_ShouldReturnDeterministicOrder_VersionNumberDescending()
    {
        var article = CreateSampleArticle();

        var v1 = new KnowledgeVersion(Guid.NewGuid(), article.Id, 1, article.Title, "Content 1", _editorId, "V1", _initialTime);
        var v2 = new KnowledgeVersion(Guid.NewGuid(), article.Id, 2, article.Title, "Content 2", _editorId, "V2", _initialTime.AddDays(1));
        var v3 = new KnowledgeVersion(Guid.NewGuid(), article.Id, 3, article.Title, "Content 3", _editorId, "V3", _initialTime.AddDays(2));

        await _versionRepo.AddAsync(v1);
        await _versionRepo.AddAsync(v2);
        await _versionRepo.AddAsync(v3);

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
    public async Task GetVersionHistory_WithEmptyArticleId_ShouldThrowValidationException()
    {
        await Assert.ThrowsAsync<ValidationException>(() =>
            _service.GetVersionHistoryAsync(Guid.Empty));
    }

    [Fact]
    public async Task GetVersionByNumber_WhenFound_ShouldReturnCorrectVersion()
    {
        var article = CreateSampleArticle();

        var v1 = new KnowledgeVersion(Guid.NewGuid(), article.Id, 1, article.Title, "Content 1", _editorId, "V1", _initialTime);
        var v2 = new KnowledgeVersion(Guid.NewGuid(), article.Id, 2, article.Title, "Content 2", _editorId, "V2", _initialTime.AddDays(1));

        await _versionRepo.AddAsync(v1);
        await _versionRepo.AddAsync(v2);

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

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetVersionByNumber_WithZeroOrNegativeVersionNumber_ShouldThrowValidationException(int invalidNumber)
    {
        var article = CreateSampleArticle();

        await Assert.ThrowsAsync<ValidationException>(() =>
            _service.GetVersionByNumberAsync(article.Id, invalidNumber));
    }

    [Fact]
    public async Task GetVersionById_WhenFound_ShouldReturnCorrectVersion()
    {
        var article = CreateSampleArticle();

        var v1 = new KnowledgeVersion(Guid.NewGuid(), article.Id, 1, article.Title, "Content 1", _editorId, "V1", _initialTime);
        await _versionRepo.AddAsync(v1);

        var fetched = await _service.GetVersionByIdAsync(v1.Id);

        Assert.NotNull(fetched);
        Assert.Equal(v1.Id, fetched.Id);
        Assert.Equal(v1.VersionNumber, fetched.VersionNumber);
    }

    [Fact]
    public async Task GetVersionById_WhenNotFound_ShouldThrowNotFoundException()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.GetVersionByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetVersionById_WithEmptyVersionId_ShouldThrowValidationException()
    {
        await Assert.ThrowsAsync<ValidationException>(() =>
            _service.GetVersionByIdAsync(Guid.Empty));
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

    [Fact]
    public async Task GetAllAuditLogs_ShouldReturnAllLogsInCreatedAtDescendingOrder()
    {
        _dateTimeProvider.UtcNow = _initialTime;
        await _service.RecordAuditLogAsync(new RecordAuditLogCommand(_editorId, "Action1", "Entity1", Guid.NewGuid(), "1"));

        _dateTimeProvider.UtcNow = _initialTime.AddMinutes(15);
        await _service.RecordAuditLogAsync(new RecordAuditLogCommand(_editorId, "Action2", "Entity2", Guid.NewGuid(), "2"));

        var allLogs = await _service.GetAllAuditLogsAsync();

        Assert.Equal(2, allLogs.Count);
        Assert.Equal("Action2", allLogs[0].Action);
        Assert.Equal("Action1", allLogs[1].Action);
    }
}
