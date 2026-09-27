namespace EngineeringKnowledgeHub.Infrastructure.Persistence.Repositories;

using EngineeringKnowledgeHub.Application.Common.Interfaces;
using EngineeringKnowledgeHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

public sealed class DocumentRepository(AppDbContext dbContext) : IDocumentRepository
{
    public async Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Documents
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<Document?> GetByHashAsync(string fileHash, CancellationToken cancellationToken = default)
    {
        var normalizedHash = fileHash.Trim();
        return await dbContext.Documents
            .FirstOrDefaultAsync(d => d.FileHash == normalizedHash, cancellationToken);
    }

    public async Task<IReadOnlyList<Document>> GetBySourceIdAsync(Guid sourceId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Documents
            .Where(d => d.SourceId == sourceId)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByHashAsync(string fileHash, CancellationToken cancellationToken = default)
    {
        var normalizedHash = fileHash.Trim();
        return await dbContext.Documents
            .AnyAsync(d => d.FileHash == normalizedHash, cancellationToken);
    }

    public async Task AddAsync(Document document, CancellationToken cancellationToken = default)
    {
        await dbContext.Documents.AddAsync(document, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Document document, CancellationToken cancellationToken = default)
    {
        dbContext.Documents.Update(document);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
