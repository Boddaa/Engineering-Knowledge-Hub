namespace EngineeringKnowledgeHub.Infrastructure.Persistence.Configurations;

using EngineeringKnowledgeHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class KnowledgeVersionConfiguration : IEntityTypeConfiguration<KnowledgeVersion>
{
    public void Configure(EntityTypeBuilder<KnowledgeVersion> builder)
    {
        builder.ToTable("KnowledgeVersions");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.ArticleId)
            .IsRequired();

        builder.HasOne<KnowledgeArticle>()
            .WithMany()
            .HasForeignKey(v => v.ArticleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(v => v.VersionNumber)
            .IsRequired();

        builder.Property(v => v.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(v => v.Content)
            .IsRequired();

        builder.Property(v => v.ChangedById)
            .IsRequired();

        builder.Property(v => v.ChangeSummary)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(v => v.CreatedAt)
            .IsRequired();

        builder.HasIndex(v => new { v.ArticleId, v.VersionNumber })
            .IsUnique();
    }
}
