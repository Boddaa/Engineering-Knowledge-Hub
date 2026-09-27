namespace EngineeringKnowledgeHub.Infrastructure.Persistence;

using EngineeringKnowledgeHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<KnowledgeArticle> KnowledgeArticles => Set<KnowledgeArticle>();
    public DbSet<KnowledgeVersion> KnowledgeVersions => Set<KnowledgeVersion>();
    public DbSet<KnowledgeSource> KnowledgeSources => Set<KnowledgeSource>();
    public DbSet<KnowledgeFeedback> KnowledgeFeedbacks => Set<KnowledgeFeedback>();
    public DbSet<KnowledgeType> KnowledgeTypes => Set<KnowledgeType>();
    public DbSet<KnowledgeCategory> KnowledgeCategories => Set<KnowledgeCategory>();
    public DbSet<KnowledgeTag> KnowledgeTags => Set<KnowledgeTag>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Technology> Technologies => Set<Technology>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<DocumentChunk> DocumentChunks => Set<DocumentChunk>();
    public DbSet<Incident> Incidents => Set<Incident>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
