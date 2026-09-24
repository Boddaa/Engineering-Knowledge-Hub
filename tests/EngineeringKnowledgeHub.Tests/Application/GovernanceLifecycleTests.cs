namespace EngineeringKnowledgeHub.Tests.Application;

using System.Reflection;
using EngineeringKnowledgeHub.Application.Articles;
using EngineeringKnowledgeHub.Application.Common.Exceptions;
using EngineeringKnowledgeHub.Application.Governance;
using EngineeringKnowledgeHub.Domain.Constants;
using EngineeringKnowledgeHub.Domain.Entities;
using EngineeringKnowledgeHub.Domain.Enums;
using EngineeringKnowledgeHub.Domain.Exceptions;
using EngineeringKnowledgeHub.Domain.ValueObjects;
using EngineeringKnowledgeHub.Tests.Common;
using Xunit;

public class GovernanceLifecycleTests
{
    private readonly InMemoryKnowledgeArticleRepository _articleRepo = new();
    private readonly InMemoryKnowledgeTypeRepository _typeRepo = new();
    private readonly InMemoryKnowledgeTagRepository _tagRepo = new();
    private readonly InMemoryProductRepository _productRepo = new();
    private readonly InMemoryTechnologyRepository _techRepo = new();
    private readonly InMemoryKnowledgeVersionRepository _versionRepo = new();
    private readonly InMemoryAuditLogRepository _auditRepo = new();
    private readonly FakeDateTimeProvider _dateTimeProvider;

    private readonly KnowledgeArticleService _articleService;
    private readonly KnowledgeGovernanceService _governanceService;

    private readonly Guid _knowledgeTypeId = Guid.NewGuid();
    private readonly Guid _authorId = Guid.NewGuid();
    private readonly Guid _reviewerId = Guid.NewGuid();
    private readonly Guid _deprecatorId = Guid.NewGuid();
    private readonly DateTimeOffset _initialTime = new(2026, 9, 24, 12, 0, 0, TimeSpan.Zero);

    public GovernanceLifecycleTests()
    {
        _dateTimeProvider = new FakeDateTimeProvider(_initialTime);

        _articleService = new KnowledgeArticleService(
            _articleRepo,
            _typeRepo,
            _tagRepo,
            _productRepo,
            _techRepo,
            _dateTimeProvider,
            _auditRepo,
            _versionRepo);

        _governanceService = new KnowledgeGovernanceService(
            _versionRepo,
            _auditRepo,
            _articleRepo,
            _dateTimeProvider);

        _typeRepo.AddAsync(new KnowledgeType(_knowledgeTypeId, "Troubleshooting", "Guide")).Wait();
    }

    private async Task<KnowledgeArticle> CreateDraftArticleAsync(string title = "Database Deadlock Resolution")
    {
        var command = new CreateKnowledgeArticleCommand(
            title,
            "Summary of deadlock resolution",
            "Content explaining deadlocks...",
            _knowledgeTypeId,
            _authorId,
            Slug: "deadlock-resolution-" + Guid.NewGuid().ToString("N")[..6]);

        var dto = await _articleService.CreateArticleAsync(command);
        return (await _articleRepo.GetByIdAsync(dto.Id))!;
    }

    [Fact]
    public async Task Test1_Draft_To_PendingReview_To_Approve_MustAlwaysCreatePublishedState_Version1_AndBothAuditRecords()
    {
        var article = await CreateDraftArticleAsync();

        // 1. Submit for review
        _dateTimeProvider.UtcNow = _initialTime.AddHours(1);
        var submitted = await _articleService.SubmitForReviewAsync(article.Id, _authorId);

        Assert.Equal(KnowledgeStatus.PendingReview, submitted.Status);

        var submitAudits = await _auditRepo.GetByEntityAsync(nameof(KnowledgeArticle), article.Id);
        var submitAudit = Assert.Single(submitAudits, a => a.Action == GovernanceAuditActions.ArticleSubmittedForReview);
        Assert.Equal(_authorId, submitAudit.UserId);
        Assert.Equal(_initialTime.AddHours(1), submitAudit.CreatedAt);

        // 2. Approve (publishing must unconditionally create version snapshot)
        _dateTimeProvider.UtcNow = _initialTime.AddHours(2);
        var approved = await _articleService.ApproveArticleAsync(new ApproveKnowledgeArticleCommand(
            article.Id,
            _reviewerId,
            ChangeSummary: "Initial approval and publish"));

        Assert.Equal(KnowledgeStatus.Published, approved.Status);
        Assert.Equal(VerificationStatus.Verified, approved.VerificationStatus);
        Assert.Equal(_reviewerId, approved.ReviewerId);

        // Verify ArticleApproved Audit record
        var articleAudits = await _auditRepo.GetByEntityAsync(nameof(KnowledgeArticle), article.Id);
        var approveAudit = Assert.Single(articleAudits, a => a.Action == GovernanceAuditActions.ArticleApproved);
        Assert.Equal(_reviewerId, approveAudit.UserId);
        Assert.Equal(_initialTime.AddHours(2), approveAudit.CreatedAt);

        // Verify KnowledgeVersion snapshot created automatically
        var versions = await _versionRepo.GetByArticleIdAsync(article.Id);
        var v1 = Assert.Single(versions);
        Assert.Equal(1, v1.VersionNumber);
        Assert.Equal(article.Title, v1.Title);
        Assert.Equal(article.Content, v1.Content);
        Assert.Equal(_reviewerId, v1.ChangedById);
        Assert.Equal("Initial approval and publish", v1.ChangeSummary);
        Assert.Equal(_initialTime.AddHours(2), v1.CreatedAt);

        // Verify KnowledgeVersionCreated Audit record
        var versionAudits = await _auditRepo.GetByEntityAsync(nameof(KnowledgeVersion), v1.Id);
        var versionAudit = Assert.Single(versionAudits, a => a.Action == GovernanceAuditActions.KnowledgeVersionCreated);
        Assert.Equal(_reviewerId, versionAudit.UserId);
        Assert.Equal(_initialTime.AddHours(2), versionAudit.CreatedAt);
        Assert.Contains("Version 1 created for article", versionAudit.Details);
    }

    [Fact]
    public async Task Test2_ApproveSameArticleAgainAfterEditCycle_MustCreateVersion2_WhileVersion1RemainsUnchanged()
    {
        var article = await CreateDraftArticleAsync();

        // 1. Initial review and publication (Version 1)
        await _articleService.SubmitForReviewAsync(article.Id);
        await _articleService.ApproveArticleAsync(new ApproveKnowledgeArticleCommand(
            article.Id,
            _reviewerId,
            ChangeSummary: "Release 1.0"));

        // 2. Author updates content (status drops to Draft for re-governance)
        _dateTimeProvider.UtcNow = _initialTime.AddDays(5);
        await _articleService.UpdateArticleAsync(new UpdateKnowledgeArticleCommand(
            article.Id,
            article.Title,
            "Updated summary for v2",
            "Completely overhauled procedures for v2..."));

        var updatedArticle = await _articleRepo.GetByIdAsync(article.Id);
        Assert.Equal(KnowledgeStatus.Draft, updatedArticle!.Status);
        Assert.Equal(VerificationStatus.Unverified, updatedArticle.VerificationStatus);

        // 3. Submit and approve second release (Version 2)
        _dateTimeProvider.UtcNow = _initialTime.AddDays(6);
        await _articleService.SubmitForReviewAsync(article.Id);

        _dateTimeProvider.UtcNow = _initialTime.AddDays(7);
        await _articleService.ApproveArticleAsync(new ApproveKnowledgeArticleCommand(
            article.Id,
            _reviewerId,
            ChangeSummary: "Release 2.0 with improved procedures"));

        // Verify Version History
        var history = await _governanceService.GetVersionHistoryAsync(article.Id);
        Assert.Equal(2, history.Count);

        // Ordered VersionNumber descending: [0] = V2, [1] = V1
        var v2 = history[0];
        var v1 = history[1];

        Assert.Equal(2, v2.VersionNumber);
        Assert.Equal("Release 2.0 with improved procedures", v2.ChangeSummary);
        Assert.Equal("Completely overhauled procedures for v2...", v2.Content);
        Assert.Equal(_reviewerId, v2.ChangedById);
        Assert.Equal(_initialTime.AddDays(7), v2.CreatedAt);

        // Verify Version 1 remains intact, unchanged, and recoverable
        Assert.Equal(1, v1.VersionNumber);
        Assert.Equal("Release 1.0", v1.ChangeSummary);
        Assert.Equal("Content explaining deadlocks...", v1.Content);
        Assert.Equal(_reviewerId, v1.ChangedById);
        Assert.Equal(_initialTime, v1.CreatedAt);
    }

    [Fact]
    public async Task Test3_VersionSnapshot_MustContainTitleContentReviewerChangeSummaryAndCreatedAtFromPublishedState()
    {
        var article = await CreateDraftArticleAsync("Specific Calibration Title");
        await _articleService.SubmitForReviewAsync(article.Id);

        var approvalTime = _initialTime.AddHours(5);
        _dateTimeProvider.UtcNow = approvalTime;

        await _articleService.ApproveArticleAsync(new ApproveKnowledgeArticleCommand(
            article.Id,
            _reviewerId,
            ChangeSummary: "Custom verified change summary"));

        var version = await _versionRepo.GetByArticleAndVersionNumberAsync(article.Id, 1);
        Assert.NotNull(version);

        Assert.Equal(article.Id, version.ArticleId);
        Assert.Equal(1, version.VersionNumber);
        Assert.Equal("Specific Calibration Title", version.Title);
        Assert.Equal(article.Content, version.Content);
        Assert.Equal(_reviewerId, version.ChangedById);
        Assert.Equal("Custom verified change summary", version.ChangeSummary);
        Assert.Equal(approvalTime, version.CreatedAt);
    }

    [Fact]
    public void Test4_ApproveKnowledgeArticleCommand_CannotExposeBypassOption()
    {
        // Verify via reflection that ApproveKnowledgeArticleCommand does not expose a CreateVersion property
        var createVersionProp = typeof(ApproveKnowledgeArticleCommand)
            .GetProperty("CreateVersion", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

        Assert.Null(createVersionProp);
    }

    [Fact]
    public async Task Test5_PeerReviewMandatory_AuthorCannotApproveOwnArticle()
    {
        var article = await CreateDraftArticleAsync();
        await _articleService.SubmitForReviewAsync(article.Id);

        // Author attempts to approve own article -> rejected by domain rule
        await Assert.ThrowsAsync<DomainRuleViolationException>(() =>
            _articleService.ApproveArticleAsync(new ApproveKnowledgeArticleCommand(article.Id, _authorId)));

        // Verify no version was created
        var versions = await _versionRepo.GetByArticleIdAsync(article.Id);
        Assert.Empty(versions);
    }

    [Fact]
    public async Task Test6_RejectionDeprecationAndArchiveBehavior_RemainUnchanged()
    {
        var article = await CreateDraftArticleAsync();

        // 1. Rejection
        await _articleService.SubmitForReviewAsync(article.Id);
        _dateTimeProvider.UtcNow = _initialTime.AddHours(3);
        var rejected = await _articleService.RejectArticleAsync(new RejectKnowledgeArticleCommand(article.Id, _reviewerId));

        Assert.Equal(KnowledgeStatus.Draft, rejected.Status);
        Assert.Equal(VerificationStatus.Rejected, rejected.VerificationStatus);

        var rejectAudits = await _auditRepo.GetByEntityAsync(nameof(KnowledgeArticle), article.Id);
        var rejectAudit = Assert.Single(rejectAudits, a => a.Action == GovernanceAuditActions.ArticleRejected);
        Assert.Equal(_reviewerId, rejectAudit.UserId);
        Assert.Equal(_initialTime.AddHours(3), rejectAudit.CreatedAt);

        // 2. Re-submit and approve to reach Published status
        await _articleService.SubmitForReviewAsync(article.Id);
        await _articleService.ApproveArticleAsync(new ApproveKnowledgeArticleCommand(article.Id, _reviewerId));

        // 3. Deprecation
        _dateTimeProvider.UtcNow = _initialTime.AddDays(30);
        var deprecated = await _articleService.DeprecateArticleAsync(article.Id, _deprecatorId);
        Assert.Equal(KnowledgeStatus.Deprecated, deprecated.Status);

        var deprecateAudits = await _auditRepo.GetByEntityAsync(nameof(KnowledgeArticle), article.Id);
        var deprecateAudit = Assert.Single(deprecateAudits, a => a.Action == GovernanceAuditActions.ArticleDeprecated);
        Assert.Equal(_deprecatorId, deprecateAudit.UserId);
        Assert.Equal(_initialTime.AddDays(30), deprecateAudit.CreatedAt);

        // 4. Archival
        var archived = await _articleService.ArchiveArticleAsync(article.Id, _deprecatorId);
        Assert.Equal(KnowledgeStatus.Archived, archived.Status);

        var archiveAudits = await _auditRepo.GetByEntityAsync(nameof(KnowledgeArticle), article.Id);
        var archiveAudit = Assert.Single(archiveAudits, a => a.Action == GovernanceAuditActions.ArticleArchived);
        Assert.Equal(_deprecatorId, archiveAudit.UserId);
    }

    [Fact]
    public async Task DeprecateArticle_FromDraftStatus_ShouldFailDomainRule()
    {
        var article = await CreateDraftArticleAsync();

        // Draft cannot be deprecated directly (must be Published)
        await Assert.ThrowsAsync<DomainRuleViolationException>(() =>
            _articleService.DeprecateArticleAsync(article.Id, _deprecatorId));
    }
}
