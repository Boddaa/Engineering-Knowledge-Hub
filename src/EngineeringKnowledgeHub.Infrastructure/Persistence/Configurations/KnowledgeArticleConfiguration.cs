namespace EngineeringKnowledgeHub.Infrastructure.Persistence.Configurations;

using System.Text.Json;
using EngineeringKnowledgeHub.Domain.Entities;
using EngineeringKnowledgeHub.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

public class KnowledgeArticleConfiguration : IEntityTypeConfiguration<KnowledgeArticle>
{
    public void Configure(EntityTypeBuilder<KnowledgeArticle> builder)
    {
        builder.ToTable("KnowledgeArticles");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(a => a.Slug)
            .HasConversion(
                slug => slug.Value,
                value => Slug.Create(value))
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(a => a.Slug)
            .IsUnique();

        builder.Property(a => a.Summary)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(a => a.Content)
            .IsRequired();

        builder.Property(a => a.KnowledgeTypeId)
            .IsRequired();

        builder.HasOne<KnowledgeType>()
            .WithMany()
            .HasForeignKey(a => a.KnowledgeTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(a => a.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(a => a.VerificationStatus)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(a => a.AuthorId)
            .IsRequired();

        builder.Property(a => a.ReviewerId);

        builder.Property(a => a.CreatedAt)
            .IsRequired();

        builder.Property(a => a.UpdatedAt);
        builder.Property(a => a.PublishedAt);
        builder.Property(a => a.LastReviewedAt);

        var guidCollectionConverter = new ValueConverter<IReadOnlyCollection<Guid>, string>(
            v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
            v => string.IsNullOrWhiteSpace(v)
                ? new HashSet<Guid>()
                : JsonSerializer.Deserialize<HashSet<Guid>>(v, (JsonSerializerOptions?)null) ?? new HashSet<Guid>());

        var guidCollectionComparer = new ValueComparer<IReadOnlyCollection<Guid>>(
            (c1, c2) => (c1 == null && c2 == null) || (c1 != null && c2 != null && c1.SequenceEqual(c2)),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToList());

        builder.Property(a => a.TagIds)
            .HasConversion(guidCollectionConverter, guidCollectionComparer)
            .HasField("_tagIds")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasColumnType("nvarchar(max)");

        builder.Property(a => a.ProductIds)
            .HasConversion(guidCollectionConverter, guidCollectionComparer)
            .HasField("_productIds")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasColumnType("nvarchar(max)");

        builder.Property(a => a.TechnologyIds)
            .HasConversion(guidCollectionConverter, guidCollectionComparer)
            .HasField("_technologyIds")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasColumnType("nvarchar(max)");

        builder.OwnsMany(a => a.SourceLinks, linkBuilder =>
        {
            linkBuilder.ToTable("ArticleSourceLinks");
            linkBuilder.WithOwner().HasForeignKey("ArticleId");
            linkBuilder.Property<Guid>("ArticleId");
            linkBuilder.Property(l => l.SourceId).IsRequired();
            linkBuilder.Property(l => l.ReferenceText).HasMaxLength(1000);
            linkBuilder.HasKey("ArticleId", nameof(ArticleSourceLink.SourceId));
        });

        builder.Navigation(a => a.SourceLinks).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
