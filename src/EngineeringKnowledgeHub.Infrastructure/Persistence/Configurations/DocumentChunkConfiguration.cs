namespace EngineeringKnowledgeHub.Infrastructure.Persistence.Configurations;

using EngineeringKnowledgeHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class DocumentChunkConfiguration : IEntityTypeConfiguration<DocumentChunk>
{
    public void Configure(EntityTypeBuilder<DocumentChunk> builder)
    {
        builder.ToTable("DocumentChunks");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.DocumentId)
            .IsRequired();

        builder.HasOne<Document>()
            .WithMany()
            .HasForeignKey(c => c.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(c => c.ChunkIndex)
            .IsRequired();

        builder.Property(c => c.Content)
            .IsRequired();

        builder.Property(c => c.TokenCount)
            .IsRequired();

        builder.Property(c => c.EmbeddingReference)
            .HasMaxLength(500);

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.HasIndex(c => new { c.DocumentId, c.ChunkIndex });
    }
}
