namespace EngineeringKnowledgeHub.Tests.Governance;

using EngineeringKnowledgeHub.Application.Articles;
using EngineeringKnowledgeHub.Application.Common.Interfaces;
using EngineeringKnowledgeHub.Application.Common.Services;
using EngineeringKnowledgeHub.Domain.Entities;
using EngineeringKnowledgeHub.Domain.Enums;
using EngineeringKnowledgeHub.Domain.ValueObjects;
using EngineeringKnowledgeHub.Infrastructure.Persistence;
using EngineeringKnowledgeHub.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class GovernancePublicationAtomicityTests : IAsyncLifetime
{
    private readonly string _databaseName;
    private readonly string _connectionString;
    private AppDbContext? _context;
    private bool _isSqlServerAvailable = true;

    public GovernancePublicationAtomicityTests()
    {
        _databaseName = $"EKH_GovTest_{Guid.NewGuid():N}";
        _connectionString = $"Server=(localdb)\\mssqllocaldb;Database={_databaseName};Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";
    }

    public async Task InitializeAsync()
    {
        try
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(_connectionString)
                .Options;

            _context = new AppDbContext(options);
            await _context.Database.MigrateAsync();
        }
        catch (Exception)
        {
            _isSqlServerAvailable = false;
        }
    }

    public async Task DisposeAsync()
    {
        if (_context != null && _isSqlServerAvailable)
        {
            try
            {
                await _context.Database.EnsureDeletedAsync();
            }
            catch
            {
                // Best-effort cleanup
            }
            await _context.DisposeAsync();
        }
    }

    [Fact]
    public async Task Approve_ShouldAtomically_PublishArticle_AndCreateKnowledgeVersion()
    {
        if (!_isSqlServerAvailable || _context == null) return;

        var articleRepo = new KnowledgeArticleRepository(_context);
        var typeRepo = new KnowledgeTypeRepository(_context);
        var tagRepo = new KnowledgeTagRepository(_context);
        var prodRepo = new ProductRepository(_context);
        var techRepo = new TechnologyRepository(_context);
        var auditRepo = new AuditLogRepository(_context);
        var versionRepo = new KnowledgeVersionRepository(_context);
        var uow = new EfUnitOfWork(_context);
        var dateTimeProvider = new SystemDateTimeProvider();

        var service = new KnowledgeArticleService(
            articleRepo,
            typeRepo,
            tagRepo,
            prodRepo,
            techRepo,
            dateTimeProvider,
            auditRepo,
            versionRepo,
            uow);

        var typeId = Guid.NewGuid();
        await typeRepo.AddAsync(new KnowledgeType(typeId, "Troubleshooting", "Troubleshooting guide"));

        var authorId = Guid.NewGuid();
        var reviewerId = Guid.NewGuid();

        // 1. Create article
        var createResult = await service.CreateArticleAsync(new CreateKnowledgeArticleCommand(
            "Service Outage Recovery",
            "Disaster recovery steps",
            "Step 1: Check logs. Step 2: Restart services.",
            typeId,
            authorId));

        var articleId = createResult.Id;

        // 2. Submit for review
        await service.SubmitForReviewAsync(articleId, authorId);

        // 3. Approve article
        var approvedArticle = await service.ApproveArticleAsync(new ApproveKnowledgeArticleCommand(
            articleId,
            reviewerId,
            "Initial publication approval"));

        // Assert article is Published & Verified
        Assert.Equal(KnowledgeStatus.Published, approvedArticle.Status);
        Assert.Equal(VerificationStatus.Verified, approvedArticle.VerificationStatus);

        // Assert historical KnowledgeVersion exists in DB
        var versions = await versionRepo.GetByArticleIdAsync(articleId);
        Assert.Single(versions);
        Assert.Equal(1, versions[0].VersionNumber);
        Assert.Equal("Service Outage Recovery", versions[0].Title);
        Assert.Equal("Initial publication approval", versions[0].ChangeSummary);

        // Assert audit logs exist for both ArticleApproved and KnowledgeVersionCreated
        var articleAudits = await auditRepo.GetByEntityAsync(nameof(KnowledgeArticle), articleId);
        Assert.Contains(articleAudits, a => a.Action == "ArticleApproved");

        var versionAudits = await auditRepo.GetByEntityAsync(nameof(KnowledgeVersion), versions[0].Id);
        Assert.Contains(versionAudits, a => a.Action == "KnowledgeVersionCreated");
    }

    [Fact]
    public async Task Approve_WhenVersionPersistenceFails_ShouldRollback_ArticlePublication()
    {
        if (!_isSqlServerAvailable || _context == null) return;

        var articleRepo = new KnowledgeArticleRepository(_context);
        var typeRepo = new KnowledgeTypeRepository(_context);
        var tagRepo = new KnowledgeTagRepository(_context);
        var prodRepo = new ProductRepository(_context);
        var techRepo = new TechnologyRepository(_context);
        var auditRepo = new AuditLogRepository(_context);
        var failingVersionRepo = new FailingOnAddVersionRepository(new KnowledgeVersionRepository(_context));
        var uow = new EfUnitOfWork(_context);
        var dateTimeProvider = new SystemDateTimeProvider();

        var service = new KnowledgeArticleService(
            articleRepo,
            typeRepo,
            tagRepo,
            prodRepo,
            techRepo,
            dateTimeProvider,
            auditRepo,
            failingVersionRepo,
            uow);

        var typeId = Guid.NewGuid();
        await typeRepo.AddAsync(new KnowledgeType(typeId, "Configuration", "Configuration guide"));

        var authorId = Guid.NewGuid();
        var reviewerId = Guid.NewGuid();

        // 1. Create and submit article
        var createResult = await service.CreateArticleAsync(new CreateKnowledgeArticleCommand(
            "Rollback Invariant Test",
            "Summary",
            "Content for rollback test",
            typeId,
            authorId));

        var articleId = createResult.Id;
        await service.SubmitForReviewAsync(articleId, authorId);

        // 2. Approve should fail due to version repository failure
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ApproveArticleAsync(new ApproveKnowledgeArticleCommand(
                articleId,
                reviewerId,
                "Should fail and rollback")));

        // 3. Clear EF change tracker cache and fetch fresh from DB to verify rollback
        _context.ChangeTracker.Clear();
        var persistedArticle = await articleRepo.GetByIdAsync(articleId);

        Assert.NotNull(persistedArticle);
        // Publication MUST have rolled back: status must NOT be Published
        Assert.Equal(KnowledgeStatus.PendingReview, persistedArticle.Status);
        Assert.Equal(VerificationStatus.Unverified, persistedArticle.VerificationStatus);

        // No version snapshot should exist
        var realVersionRepo = new KnowledgeVersionRepository(_context);
        var versions = await realVersionRepo.GetByArticleIdAsync(articleId);
        Assert.Empty(versions);
    }

    private sealed class FailingOnAddVersionRepository(IKnowledgeVersionRepository inner) : IKnowledgeVersionRepository
    {
        public Task<KnowledgeVersion?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            inner.GetByIdAsync(id, cancellationToken);

        public Task<KnowledgeVersion?> GetByArticleAndVersionNumberAsync(Guid articleId, int versionNumber, CancellationToken cancellationToken = default) =>
            inner.GetByArticleAndVersionNumberAsync(articleId, versionNumber, cancellationToken);

        public Task<IReadOnlyList<KnowledgeVersion>> GetByArticleIdAsync(Guid articleId, CancellationToken cancellationToken = default) =>
            inner.GetByArticleIdAsync(articleId, cancellationToken);

        public Task<int> GetLatestVersionNumberAsync(Guid articleId, CancellationToken cancellationToken = default) =>
            inner.GetLatestVersionNumberAsync(articleId, cancellationToken);

        public Task<bool> ExistsAsync(Guid articleId, int versionNumber, CancellationToken cancellationToken = default) =>
            inner.ExistsAsync(articleId, versionNumber, cancellationToken);

        public Task AddAsync(KnowledgeVersion version, CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("Simulated database failure during KnowledgeVersion persistence.");
        }
    }
}
