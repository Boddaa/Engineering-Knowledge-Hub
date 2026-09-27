namespace EngineeringKnowledgeHub.Infrastructure.Persistence.Configurations;

using EngineeringKnowledgeHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class KnowledgeSourceConfiguration : IEntityTypeConfiguration<KnowledgeSource>
{
    public void Configure(EntityTypeBuilder<KnowledgeSource> builder)
    {
        builder.ToTable("KnowledgeSources");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(s => s.Name)
            .IsUnique();

        builder.Property(s => s.SourceType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(s => s.Url)
            .HasMaxLength(2048)
            .IsRequired();

        builder.Property(s => s.Description)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(s => s.TrustLevel)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(s => s.IsApproved)
            .IsRequired();

        builder.Property(s => s.CreatedAt)
            .IsRequired();

        builder.Property(s => s.UpdatedAt);
    }
}
