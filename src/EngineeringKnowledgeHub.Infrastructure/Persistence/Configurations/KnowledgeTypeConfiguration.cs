namespace EngineeringKnowledgeHub.Infrastructure.Persistence.Configurations;

using EngineeringKnowledgeHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class KnowledgeTypeConfiguration : IEntityTypeConfiguration<KnowledgeType>
{
    public void Configure(EntityTypeBuilder<KnowledgeType> builder)
    {
        builder.ToTable("KnowledgeTypes");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(t => t.Name)
            .IsUnique();

        builder.Property(t => t.Description)
            .HasMaxLength(1000)
            .IsRequired();
    }
}
