namespace EngineeringKnowledgeHub.Infrastructure.Persistence.Configurations;

using EngineeringKnowledgeHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class KnowledgeFeedbackConfiguration : IEntityTypeConfiguration<KnowledgeFeedback>
{
    public void Configure(EntityTypeBuilder<KnowledgeFeedback> builder)
    {
        builder.ToTable("KnowledgeFeedbacks");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.ArticleId)
            .IsRequired();

        builder.HasOne<KnowledgeArticle>()
            .WithMany()
            .HasForeignKey(f => f.ArticleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(f => f.UserId)
            .IsRequired();

        builder.Property(f => f.FeedbackType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(f => f.Comment)
            .HasMaxLength(2000);

        builder.Property(f => f.CreatedAt)
            .IsRequired();

        builder.HasIndex(f => f.ArticleId);
    }
}
