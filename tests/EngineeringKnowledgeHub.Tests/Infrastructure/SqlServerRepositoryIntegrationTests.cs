namespace EngineeringKnowledgeHub.Tests.Infrastructure;

using EngineeringKnowledgeHub.Application.Common.Exceptions;
using EngineeringKnowledgeHub.Domain.Constants;
using EngineeringKnowledgeHub.Domain.Entities;
using EngineeringKnowledgeHub.Domain.Enums;
using EngineeringKnowledgeHub.Domain.ValueObjects;
using EngineeringKnowledgeHub.Infrastructure.Persistence;
using EngineeringKnowledgeHub.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class SqlServerRepositoryIntegrationTests : IAsyncLifetime
{
    private readonly string _databaseName;
    private readonly string _connectionString;
    private AppDbContext _context = null!;

    public SqlServerRepositoryIntegrationTests()
    {
        _databaseName = $"EKH_Test_{Guid.NewGuid():N}";
        _connectionString = $"Server=(localdb)\\mssqllocaldb;Database={_databaseName};Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";
    }

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(_connectionString)
            .Options;

        _context = new AppDbContext(options);
        await _context.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        if (_context != null)
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
    public async Task KnowledgeArticle_Persistence_RoundTrip_Works_Correctly()
    {

        var typeRepo = new KnowledgeTypeRepository(_context);
        var articleRepo = new KnowledgeArticleRepository(_context);

        var typeId = Guid.NewGuid();
        var type = new KnowledgeType(typeId, "Architecture", "Architecture guide");
        await typeRepo.AddAsync(type);

        var articleId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        var tagId1 = Guid.NewGuid();
        var tagId2 = Guid.NewGuid();
        var prodId = Guid.NewGuid();
        var techId = Guid.NewGuid();
        var sourceId = Guid.NewGuid();

        var article = new KnowledgeArticle(
            articleId,
            "Persistence Architecture",
            Slug.FromTitle("Persistence Architecture"),
            "Summary of persistence",
            "Full content of persistence architecture guide.",
            typeId,
            authorId,
            now);

        article.AddTag(tagId1);
        article.AddTag(tagId2);
        article.AddProduct(prodId);
        article.AddTechnology(techId);
        article.AddSource(sourceId, "Section 4.1");

        await articleRepo.AddAsync(article);

        // Fetch back
        var fetched = await articleRepo.GetByIdAsync(articleId);
        Assert.NotNull(fetched);
        Assert.Equal("Persistence Architecture", fetched.Title);
        Assert.Equal(Slug.FromTitle("Persistence Architecture"), fetched.Slug);
        Assert.Equal(KnowledgeStatus.Draft, fetched.Status);
        Assert.Equal(2, fetched.TagIds.Count);
        Assert.Contains(tagId1, fetched.TagIds);
        Assert.Contains(tagId2, fetched.TagIds);
        Assert.Single(fetched.ProductIds);
        Assert.Contains(prodId, fetched.ProductIds);
        Assert.Single(fetched.TechnologyIds);
        Assert.Contains(techId, fetched.TechnologyIds);
        Assert.Single(fetched.SourceLinks);
        Assert.Equal("Section 4.1", fetched.SourceLinks.First().ReferenceText);

        // Test slug exists
        var exists = await articleRepo.ExistsBySlugAsync(fetched.Slug);
        Assert.True(exists);

        // Test update
        fetched.UpdateContent(
            "Persistence Architecture Updated",
            Slug.FromTitle("Persistence Architecture Updated"),
            "Updated summary",
            "Updated content",
            DateTimeOffset.UtcNow);

        await articleRepo.UpdateAsync(fetched);

        var updated = await articleRepo.GetByIdAsync(articleId);
        Assert.NotNull(updated);
        Assert.Equal("Persistence Architecture Updated", updated.Title);
        Assert.Equal("Updated summary", updated.Summary);
    }

    [Fact]
    public async Task KnowledgeVersion_Persistence_RoundTrip_Works_Correctly()
    {

        var typeRepo = new KnowledgeTypeRepository(_context);
        var articleRepo = new KnowledgeArticleRepository(_context);
        var versionRepo = new KnowledgeVersionRepository(_context);

        var typeId = Guid.NewGuid();
        await typeRepo.AddAsync(new KnowledgeType(typeId, "Procedure", "Procedure guide"));

        var articleId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var article = new KnowledgeArticle(
            articleId,
            "Deployment Procedure",
            Slug.FromTitle("Deployment Procedure"),
            "Deployment guide",
            "Step 1: Build. Step 2: Deploy.",
            typeId,
            authorId,
            DateTimeOffset.UtcNow);

        await articleRepo.AddAsync(article);

        var v1 = new KnowledgeVersion(
            Guid.NewGuid(),
            articleId,
            1,
            article.Title,
            article.Content,
            authorId,
            "Initial release",
            DateTimeOffset.UtcNow);

        await versionRepo.AddAsync(v1);

        var v2 = new KnowledgeVersion(
            Guid.NewGuid(),
            articleId,
            2,
            article.Title,
            article.Content + " Step 3: Verify.",
            authorId,
            "Added verification step",
            DateTimeOffset.UtcNow);

        await versionRepo.AddAsync(v2);

        var latestVersionNumber = await versionRepo.GetLatestVersionNumberAsync(articleId);
        Assert.Equal(2, latestVersionNumber);

        var exists1 = await versionRepo.ExistsAsync(articleId, 1);
        Assert.True(exists1);

        var exists3 = await versionRepo.ExistsAsync(articleId, 3);
        Assert.False(exists3);

        var fetchedV1 = await versionRepo.GetByArticleAndVersionNumberAsync(articleId, 1);
        Assert.NotNull(fetchedV1);
        Assert.Equal("Initial release", fetchedV1.ChangeSummary);

        var allVersions = await versionRepo.GetByArticleIdAsync(articleId);
        Assert.Equal(2, allVersions.Count);

        // Conflict check
        var duplicateV1 = new KnowledgeVersion(
            Guid.NewGuid(),
            articleId,
            1,
            "Duplicate",
            "Content",
            authorId,
            "Duplicate",
            DateTimeOffset.UtcNow);

        await Assert.ThrowsAsync<ConflictException>(() => versionRepo.AddAsync(duplicateV1));
    }

    [Fact]
    public async Task KnowledgeSource_Persistence_RoundTrip_Works_Correctly()
    {

        var sourceRepo = new KnowledgeSourceRepository(_context);

        var sourceId = Guid.NewGuid();
        var source = new KnowledgeSource(
            sourceId,
            "Vendor Documentation",
            SourceType.VendorDocumentation,
            DateTimeOffset.UtcNow,
            "https://vendor.example.com",
            "Official vendor docs",
            TrustLevel.Verified,
            isApproved: true);

        await sourceRepo.AddAsync(source);

        var fetched = await sourceRepo.GetByIdAsync(sourceId);
        Assert.NotNull(fetched);
        Assert.Equal("Vendor Documentation", fetched.Name);
        Assert.Equal(SourceType.VendorDocumentation, fetched.SourceType);
        Assert.Equal(TrustLevel.Verified, fetched.TrustLevel);
        Assert.True(fetched.IsApproved);

        var exists = await sourceRepo.ExistsByNameAsync("Vendor Documentation");
        Assert.True(exists);

        var filtered = await sourceRepo.GetFilteredAsync(
            sourceType: SourceType.VendorDocumentation,
            isApproved: true);

        Assert.Contains(filtered, s => s.Id == sourceId);
    }

    [Fact]
    public async Task AuditLog_Persistence_RoundTrip_Works_Correctly()
    {

        var auditRepo = new AuditLogRepository(_context);

        var userId = Guid.NewGuid();
        var entityId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        var log1 = new AuditLog(
            Guid.NewGuid(),
            userId,
            GovernanceAuditActions.ArticleSubmittedForReview,
            nameof(KnowledgeArticle),
            entityId,
            "Article submitted for review",
            now);

        var log2 = new AuditLog(
            Guid.NewGuid(),
            userId,
            GovernanceAuditActions.ArticleApproved,
            nameof(KnowledgeArticle),
            entityId,
            "Article approved",
            now.AddSeconds(10));

        await auditRepo.AddAsync(log1);
        await auditRepo.AddAsync(log2);

        var byEntity = await auditRepo.GetByEntityAsync(nameof(KnowledgeArticle), entityId);
        Assert.Equal(2, byEntity.Count);

        var byUser = await auditRepo.GetByUserIdAsync(userId);
        Assert.Contains(byUser, l => l.Id == log1.Id);
        Assert.Contains(byUser, l => l.Id == log2.Id);
    }

    [Fact]
    public async Task Incident_Persistence_RoundTrip_Works_Correctly()
    {

        var incidentRepo = new IncidentRepository(_context);

        var incidentId = Guid.NewGuid();
        var createdById = Guid.NewGuid();
        var articleId1 = Guid.NewGuid();
        var articleId2 = Guid.NewGuid();

        var incident = new Incident(
            incidentId,
            "Service Outage in Production",
            "Database connection timeout under load.",
            IncidentSeverity.Critical,
            createdById,
            DateTimeOffset.UtcNow,
            "Error 500 across endpoints");

        incident.LinkArticle(articleId1, ArticleRelationshipType.Resolves);
        incident.LinkArticle(articleId2, ArticleRelationshipType.RelatesTo);

        await incidentRepo.AddAsync(incident);

        var fetched = await incidentRepo.GetByIdAsync(incidentId);
        Assert.NotNull(fetched);
        Assert.Equal("Service Outage in Production", fetched.Title);
        Assert.Equal(IncidentSeverity.Critical, fetched.Severity);
        Assert.Equal(2, fetched.ArticleLinks.Count);

        var linkedTo1 = await incidentRepo.GetByArticleIdAsync(articleId1);
        Assert.Single(linkedTo1);
        Assert.Equal(incidentId, linkedTo1[0].Id);
    }
}
