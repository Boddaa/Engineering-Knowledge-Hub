namespace EngineeringKnowledgeHub.Infrastructure.Persistence.Configurations;

using EngineeringKnowledgeHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class KnowledgeTagConfiguration : IEntityTypeConfiguration<KnowledgeTag>
{
    public void Configure(EntityTypeBuilder<KnowledgeTag> builder)
    {
        builder.ToTable("KnowledgeTags");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(t => t.Name)
            .IsUnique();
    }
}
