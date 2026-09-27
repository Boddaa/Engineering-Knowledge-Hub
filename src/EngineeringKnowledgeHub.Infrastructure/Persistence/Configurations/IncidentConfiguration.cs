namespace EngineeringKnowledgeHub.Infrastructure.Persistence.Configurations;

using EngineeringKnowledgeHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class IncidentConfiguration : IEntityTypeConfiguration<Incident>
{
    public void Configure(EntityTypeBuilder<Incident> builder)
    {
        builder.ToTable("Incidents");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(i => i.ProblemDescription)
            .IsRequired();

        builder.Property(i => i.Symptoms)
            .IsRequired();

        builder.Property(i => i.RootCause)
            .IsRequired();

        builder.Property(i => i.Resolution)
            .IsRequired();

        builder.Property(i => i.Severity)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(i => i.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(i => i.CreatedById)
            .IsRequired();

        builder.Property(i => i.ResolvedById);

        builder.Property(i => i.CreatedAt)
            .IsRequired();

        builder.Property(i => i.ResolvedAt);

        builder.OwnsMany(i => i.ArticleLinks, linkBuilder =>
        {
            linkBuilder.ToTable("IncidentArticleLinks");
            linkBuilder.WithOwner().HasForeignKey("IncidentId");
            linkBuilder.Property<Guid>("IncidentId");
            linkBuilder.Property(l => l.ArticleId).IsRequired();
            linkBuilder.Property(l => l.RelationshipType)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();
            linkBuilder.HasKey("IncidentId", nameof(IncidentArticleLink.ArticleId));
        });

        builder.Navigation(i => i.ArticleLinks).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
