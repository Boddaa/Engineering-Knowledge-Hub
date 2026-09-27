namespace EngineeringKnowledgeHub.Infrastructure.Persistence.Configurations;

using EngineeringKnowledgeHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("Documents");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.FileName)
            .HasMaxLength(260)
            .IsRequired();

        builder.Property(d => d.StoragePath)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(d => d.ContentType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(d => d.FileHash)
            .HasMaxLength(128)
            .IsRequired();

        builder.HasIndex(d => d.FileHash)
            .IsUnique();

        builder.Property(d => d.SourceId)
            .IsRequired();

        builder.HasOne<KnowledgeSource>()
            .WithMany()
            .HasForeignKey(d => d.SourceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(d => d.ProcessingStatus)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(d => d.UploadedById)
            .IsRequired();

        builder.Property(d => d.CreatedAt)
            .IsRequired();

        builder.Property(d => d.ProcessedAt);

        builder.Property(d => d.FailureReason)
            .HasMaxLength(2000);

        builder.Property(d => d.ExtractedText);
    }
}
