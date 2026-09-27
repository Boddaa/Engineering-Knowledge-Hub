namespace EngineeringKnowledgeHub.Infrastructure.Persistence.Repositories;

using EngineeringKnowledgeHub.Application.Common.Interfaces;
using EngineeringKnowledgeHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

public sealed class AuditLogRepository(AppDbContext dbContext) : IAuditLogRepository
{
    public async Task<AuditLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.AuditLogs
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<AuditLog>> GetByEntityAsync(string entityName, Guid entityId, CancellationToken cancellationToken = default)
    {
        return await dbContext.AuditLogs
            .Where(l => l.EntityName == entityName && l.EntityId == entityId)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AuditLog>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.AuditLogs
            .Where(l => l.UserId == userId)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AuditLog>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.AuditLogs
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
    {
        await dbContext.AuditLogs.AddAsync(auditLog, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
