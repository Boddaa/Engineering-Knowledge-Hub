namespace EngineeringKnowledgeHub.Tests.Infrastructure;

using EngineeringKnowledgeHub.Domain.Entities;
using EngineeringKnowledgeHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class AppDbContextModelTests
{
    [Fact]
    public void AppDbContext_Can_Be_Constructed_With_SqlServer_Options()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=EngineeringKnowledgeHubTestDb;Trusted_Connection=True;")
            .Options;

        using var context = new AppDbContext(options);
        Assert.NotNull(context);
    }

    [Fact]
    public void AppDbContext_Model_Should_Be_Valid_And_Contain_All_Implemented_Entities()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=EngineeringKnowledgeHubTestDb;Trusted_Connection=True;")
            .Options;

        using var context = new AppDbContext(options);
        var model = context.Model;

        Assert.NotNull(model);

        // Verify all 13 domain entity types are mapped
        var expectedEntityTypes = new[]
        {
            typeof(KnowledgeArticle),
            typeof(KnowledgeVersion),
            typeof(KnowledgeSource),
            typeof(KnowledgeFeedback),
            typeof(KnowledgeType),
            typeof(KnowledgeCategory),
            typeof(KnowledgeTag),
            typeof(Product),
            typeof(Technology),
            typeof(Document),
            typeof(DocumentChunk),
            typeof(Incident),
            typeof(AuditLog)
        };

        foreach (var entityType in expectedEntityTypes)
        {
            var efType = model.FindEntityType(entityType);
            Assert.NotNull(efType);
            Assert.NotNull(efType.FindPrimaryKey());
        }
    }

    [Fact]
    public void AppDbContext_Should_Map_Relationships_And_Indexes_Correctly()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=EngineeringKnowledgeHubTestDb;Trusted_Connection=True;")
            .Options;

        using var context = new AppDbContext(options);
        var model = context.Model;

        // Article Slug index
        var articleType = model.FindEntityType(typeof(KnowledgeArticle))!;
        var slugProp = articleType.FindProperty(nameof(KnowledgeArticle.Slug));
        Assert.NotNull(slugProp);
        var slugIndex = articleType.GetIndexes().FirstOrDefault(i => i.Properties.Contains(slugProp));
        Assert.NotNull(slugIndex);
        Assert.True(slugIndex.IsUnique);

        // Version unique index on (ArticleId, VersionNumber)
        var versionType = model.FindEntityType(typeof(KnowledgeVersion))!;
        var versionIndex = versionType.GetIndexes()
            .FirstOrDefault(i => i.Properties.Any(p => p.Name == nameof(KnowledgeVersion.ArticleId))
                              && i.Properties.Any(p => p.Name == nameof(KnowledgeVersion.VersionNumber)));
        Assert.NotNull(versionIndex);
        Assert.True(versionIndex.IsUnique);

        // Incident links owned table
        var incidentType = model.FindEntityType(typeof(Incident))!;
        var articleLinksNav = incidentType.FindNavigation(nameof(Incident.ArticleLinks));
        Assert.NotNull(articleLinksNav);
    }
}
