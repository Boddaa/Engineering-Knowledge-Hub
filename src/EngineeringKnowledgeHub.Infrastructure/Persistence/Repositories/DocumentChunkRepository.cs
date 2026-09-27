namespace EngineeringKnowledgeHub.Infrastructure.Persistence.Repositories;

using EngineeringKnowledgeHub.Application.Common.Interfaces;
using EngineeringKnowledgeHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

public sealed class DocumentChunkRepository(AppDbContext dbContext) : IDocumentChunkRepository
{
    public async Task<DocumentChunk?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.DocumentChunks
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<DocumentChunk>> GetByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        return await dbContext.DocumentChunks
            .Where(c => c.DocumentId == documentId)
            .OrderBy(c => c.ChunkIndex)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(DocumentChunk chunk, CancellationToken cancellationToken = default)
    {
        await dbContext.DocumentChunks.AddAsync(chunk, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<DocumentChunk> chunks, CancellationToken cancellationToken = default)
    {
        await dbContext.DocumentChunks.AddRangeAsync(chunks, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        var chunks = await dbContext.DocumentChunks
            .Where(c => c.DocumentId == documentId)
            .ToListAsync(cancellationToken);

        if (chunks.Count != 0)
        {
            dbContext.DocumentChunks.RemoveRange(chunks);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
