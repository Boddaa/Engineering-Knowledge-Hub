namespace EngineeringKnowledgeHub.Tests.Common;

using System.Collections.Concurrent;
using EngineeringKnowledgeHub.Application.Common.Exceptions;
using EngineeringKnowledgeHub.Application.Common.Interfaces;
using EngineeringKnowledgeHub.Domain.Entities;
using EngineeringKnowledgeHub.Domain.Enums;
using EngineeringKnowledgeHub.Domain.ValueObjects;

public class FakeDateTimeProvider(DateTimeOffset utcNow) : IDateTimeProvider
{
    public DateTimeOffset UtcNow { get; set; } = utcNow;
}

public class InMemoryKnowledgeArticleRepository : IKnowledgeArticleRepository
{
    private readonly ConcurrentDictionary<Guid, KnowledgeArticle> _articles = new();

    public Task<KnowledgeArticle?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _articles.TryGetValue(id, out var article);
        return Task.FromResult(article);
    }

    public Task<IReadOnlyList<KnowledgeArticle>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<KnowledgeArticle> list = _articles.Values.ToList();
        return Task.FromResult(list);
    }

    public Task<bool> ExistsBySlugAsync(Slug slug, CancellationToken cancellationToken = default)
    {
        var exists = _articles.Values.Any(a => a.Slug == slug);
        return Task.FromResult(exists);
    }

    public Task AddAsync(KnowledgeArticle article, CancellationToken cancellationToken = default)
    {
        _articles[article.Id] = article;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(KnowledgeArticle article, CancellationToken cancellationToken = default)
    {
        _articles[article.Id] = article;
        return Task.CompletedTask;
    }
}

public class InMemoryKnowledgeCategoryRepository : IKnowledgeCategoryRepository
{
    private readonly ConcurrentDictionary<Guid, KnowledgeCategory> _categories = new();

    public Task<IReadOnlyList<KnowledgeCategory>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<KnowledgeCategory> list = _categories.Values.OrderBy(c => c.Name).ToList();
        return Task.FromResult(list);
    }

    public Task<KnowledgeCategory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _categories.TryGetValue(id, out var category);
        return Task.FromResult(category);
    }

    public Task AddAsync(KnowledgeCategory category, CancellationToken cancellationToken = default)
    {
        _categories[category.Id] = category;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(KnowledgeCategory category, CancellationToken cancellationToken = default)
    {
        _categories[category.Id] = category;
        return Task.CompletedTask;
    }
}

public class InMemoryKnowledgeTypeRepository : IKnowledgeTypeRepository
{
    private readonly ConcurrentDictionary<Guid, KnowledgeType> _types = new();

    public Task<IReadOnlyList<KnowledgeType>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<KnowledgeType> list = _types.Values.OrderBy(t => t.Name).ToList();
        return Task.FromResult(list);
    }

    public Task<KnowledgeType?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _types.TryGetValue(id, out var type);
        return Task.FromResult(type);
    }

    public Task AddAsync(KnowledgeType type, CancellationToken cancellationToken = default)
    {
        _types[type.Id] = type;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(KnowledgeType type, CancellationToken cancellationToken = default)
    {
        _types[type.Id] = type;
        return Task.CompletedTask;
    }
}

public class InMemoryKnowledgeTagRepository : IKnowledgeTagRepository
{
    private readonly ConcurrentDictionary<Guid, KnowledgeTag> _tags = new();

    public Task<IReadOnlyList<KnowledgeTag>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<KnowledgeTag> list = _tags.Values.OrderBy(t => t.Name).ToList();
        return Task.FromResult(list);
    }

    public Task<KnowledgeTag?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _tags.TryGetValue(id, out var tag);
        return Task.FromResult(tag);
    }

    public Task<KnowledgeTag?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var tag = _tags.Values.FirstOrDefault(t => string.Equals(t.Name, name.Trim(), StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(tag);
    }

    public Task AddAsync(KnowledgeTag tag, CancellationToken cancellationToken = default)
    {
        _tags[tag.Id] = tag;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(KnowledgeTag tag, CancellationToken cancellationToken = default)
    {
        _tags[tag.Id] = tag;
        return Task.CompletedTask;
    }
}

public class InMemoryProductRepository : IProductRepository
{
    private readonly ConcurrentDictionary<Guid, Product> _products = new();

    public Task<IReadOnlyList<Product>> GetAllAsync(bool? isActive = null, CancellationToken cancellationToken = default)
    {
        var query = _products.Values.AsEnumerable();
        if (isActive.HasValue)
        {
            query = query.Where(p => p.IsActive == isActive.Value);
        }

        IReadOnlyList<Product> list = query.OrderBy(p => p.Name).ToList();
        return Task.FromResult(list);
    }

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _products.TryGetValue(id, out var product);
        return Task.FromResult(product);
    }

    public Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var normalizedCode = code.Trim().ToUpperInvariant();
        var exists = _products.Values.Any(p => string.Equals(p.Code, normalizedCode, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(exists);
    }

    public Task AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        _products[product.Id] = product;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        _products[product.Id] = product;
        return Task.CompletedTask;
    }
}

public class InMemoryTechnologyRepository : ITechnologyRepository
{
    private readonly ConcurrentDictionary<Guid, Technology> _technologies = new();

    public Task<IReadOnlyList<Technology>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Technology> list = _technologies.Values.OrderBy(t => t.Name).ToList();
        return Task.FromResult(list);
    }

    public Task<Technology?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _technologies.TryGetValue(id, out var technology);
        return Task.FromResult(technology);
    }

    public Task AddAsync(Technology technology, CancellationToken cancellationToken = default)
    {
        _technologies[technology.Id] = technology;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Technology technology, CancellationToken cancellationToken = default)
    {
        _technologies[technology.Id] = technology;
        return Task.CompletedTask;
    }
}

public class InMemoryIncidentRepository : IIncidentRepository
{
    private readonly ConcurrentDictionary<Guid, Incident> _incidents = new();

    public Task<Incident?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _incidents.TryGetValue(id, out var incident);
        return Task.FromResult(incident);
    }

    public Task<IReadOnlyList<Incident>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Incident> list = _incidents.Values.ToList();
        return Task.FromResult(list);
    }

    public Task<IReadOnlyList<Incident>> GetByArticleIdAsync(Guid articleId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Incident> list = _incidents.Values
            .Where(i => i.ArticleLinks.Any(l => l.ArticleId == articleId))
            .ToList();
        return Task.FromResult(list);
    }

    public Task AddAsync(Incident incident, CancellationToken cancellationToken = default)
    {
        _incidents[incident.Id] = incident;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Incident incident, CancellationToken cancellationToken = default)
    {
        _incidents[incident.Id] = incident;
        return Task.CompletedTask;
    }
}

public class InMemoryKnowledgeFeedbackRepository : IKnowledgeFeedbackRepository
{
    private readonly ConcurrentBag<KnowledgeFeedback> _feedbacks = [];

    public Task AddAsync(KnowledgeFeedback feedback, CancellationToken cancellationToken = default)
    {
        _feedbacks.Add(feedback);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<KnowledgeFeedback>> GetByArticleIdAsync(Guid articleId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<KnowledgeFeedback> list = _feedbacks
            .Where(f => f.ArticleId == articleId)
            .OrderByDescending(f => f.CreatedAt)
            .ToList();
        return Task.FromResult(list);
    }
}

public class InMemoryKnowledgeSourceRepository : IKnowledgeSourceRepository
{
    private readonly ConcurrentDictionary<Guid, KnowledgeSource> _sources = new();

    public Task<KnowledgeSource?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _sources.TryGetValue(id, out var source);
        return Task.FromResult(source);
    }

    public Task<IReadOnlyList<KnowledgeSource>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<KnowledgeSource> list = _sources.Values.ToList();
        return Task.FromResult(list);
    }

    public Task<IReadOnlyList<KnowledgeSource>> GetFilteredAsync(
        SourceType? sourceType = null,
        TrustLevel? trustLevel = null,
        bool? isApproved = null,
        CancellationToken cancellationToken = default)
    {
        var query = _sources.Values.AsEnumerable();

        if (sourceType.HasValue)
        {
            query = query.Where(s => s.SourceType == sourceType.Value);
        }

        if (trustLevel.HasValue)
        {
            query = query.Where(s => s.TrustLevel == trustLevel.Value);
        }

        if (isApproved.HasValue)
        {
            query = query.Where(s => s.IsApproved == isApproved.Value);
        }

        IReadOnlyList<KnowledgeSource> list = query.ToList();
        return Task.FromResult(list);
    }

    public Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var exists = _sources.Values.Any(s => string.Equals(s.Name, name.Trim(), StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(exists);
    }

    public Task AddAsync(KnowledgeSource source, CancellationToken cancellationToken = default)
    {
        _sources[source.Id] = source;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(KnowledgeSource source, CancellationToken cancellationToken = default)
    {
        _sources[source.Id] = source;
        return Task.CompletedTask;
    }
}

public class InMemoryDocumentRepository : IDocumentRepository
{
    private readonly ConcurrentDictionary<Guid, Document> _documents = new();

    public Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _documents.TryGetValue(id, out var document);
        return Task.FromResult(document);
    }

    public Task<Document?> GetByHashAsync(string fileHash, CancellationToken cancellationToken = default)
    {
        var document = _documents.Values.FirstOrDefault(d => string.Equals(d.FileHash, fileHash.Trim(), StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(document);
    }

    public Task<IReadOnlyList<Document>> GetBySourceIdAsync(Guid sourceId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Document> list = _documents.Values.Where(d => d.SourceId == sourceId).ToList();
        return Task.FromResult(list);
    }

    public Task<bool> ExistsByHashAsync(string fileHash, CancellationToken cancellationToken = default)
    {
        var exists = _documents.Values.Any(d => string.Equals(d.FileHash, fileHash.Trim(), StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(exists);
    }

    public Task AddAsync(Document document, CancellationToken cancellationToken = default)
    {
        _documents[document.Id] = document;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Document document, CancellationToken cancellationToken = default)
    {
        _documents[document.Id] = document;
        return Task.CompletedTask;
    }
}

public class InMemoryKnowledgeVersionRepository : IKnowledgeVersionRepository
{
    private readonly ConcurrentDictionary<Guid, KnowledgeVersion> _versions = new();

    public Task<KnowledgeVersion?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _versions.TryGetValue(id, out var version);
        return Task.FromResult(version);
    }

    public Task<KnowledgeVersion?> GetByArticleAndVersionNumberAsync(Guid articleId, int versionNumber, CancellationToken cancellationToken = default)
    {
        var version = _versions.Values.FirstOrDefault(v => v.ArticleId == articleId && v.VersionNumber == versionNumber);
        return Task.FromResult(version);
    }

    public Task<IReadOnlyList<KnowledgeVersion>> GetByArticleIdAsync(Guid articleId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<KnowledgeVersion> list = _versions.Values
            .Where(v => v.ArticleId == articleId)
            .ToList();
        return Task.FromResult(list);
    }

    public Task<int> GetLatestVersionNumberAsync(Guid articleId, CancellationToken cancellationToken = default)
    {
        var versions = _versions.Values.Where(v => v.ArticleId == articleId).ToList();
        var max = versions.Count != 0 ? versions.Max(v => v.VersionNumber) : 0;
        return Task.FromResult(max);
    }

    public Task<bool> ExistsAsync(Guid articleId, int versionNumber, CancellationToken cancellationToken = default)
    {
        var exists = _versions.Values.Any(v => v.ArticleId == articleId && v.VersionNumber == versionNumber);
        return Task.FromResult(exists);
    }

    public Task AddAsync(KnowledgeVersion version, CancellationToken cancellationToken = default)
    {
        if (_versions.Values.Any(v => v.ArticleId == version.ArticleId && v.VersionNumber == version.VersionNumber))
        {
            throw new ConflictException($"Version {version.VersionNumber} already exists for article '{version.ArticleId}'.");
        }

        _versions[version.Id] = version;
        return Task.CompletedTask;
    }
}

public class InMemoryAuditLogRepository : IAuditLogRepository
{
    private readonly List<AuditLog> _logs = [];
    private readonly object _lock = new();

    public Task<AuditLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var log = _logs.FirstOrDefault(l => l.Id == id);
            return Task.FromResult(log);
        }
    }

    public Task<IReadOnlyList<AuditLog>> GetByEntityAsync(string entityName, Guid entityId, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            IReadOnlyList<AuditLog> list = _logs
                .Where(l => string.Equals(l.EntityName, entityName, StringComparison.OrdinalIgnoreCase) && l.EntityId == entityId)
                .ToList();
            return Task.FromResult(list);
        }
    }

    public Task<IReadOnlyList<AuditLog>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            IReadOnlyList<AuditLog> list = _logs
                .Where(l => l.UserId == userId)
                .ToList();
            return Task.FromResult(list);
        }
    }

    public Task<IReadOnlyList<AuditLog>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            IReadOnlyList<AuditLog> list = _logs.ToList();
            return Task.FromResult(list);
        }
    }

    public Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            _logs.Add(auditLog);
            return Task.CompletedTask;
        }
    }
}

public class InMemoryDocumentChunkRepository : IDocumentChunkRepository
{
    private readonly ConcurrentDictionary<Guid, DocumentChunk> _chunks = new();

    public Task<DocumentChunk?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _chunks.TryGetValue(id, out var chunk);
        return Task.FromResult(chunk);
    }

    public Task<IReadOnlyList<DocumentChunk>> GetByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<DocumentChunk> list = _chunks.Values
            .Where(c => c.DocumentId == documentId)
            .OrderBy(c => c.ChunkIndex)
            .ToList();
        return Task.FromResult(list);
    }

    public Task AddAsync(DocumentChunk chunk, CancellationToken cancellationToken = default)
    {
        _chunks[chunk.Id] = chunk;
        return Task.CompletedTask;
    }

    public Task AddRangeAsync(IEnumerable<DocumentChunk> chunks, CancellationToken cancellationToken = default)
    {
        foreach (var chunk in chunks)
        {
            _chunks[chunk.Id] = chunk;
        }
        return Task.CompletedTask;
    }

    public Task DeleteByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        var toRemove = _chunks.Values
            .Where(c => c.DocumentId == documentId)
            .Select(c => c.Id)
            .ToList();

        foreach (var id in toRemove)
        {
            _chunks.TryRemove(id, out _);
        }

        return Task.CompletedTask;
    }
}

public class PassThroughUnitOfWork : IUnitOfWork
{
    public bool TransactionExecuted { get; set; }

    public async Task ExecuteTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken = default)
    {
        TransactionExecuted = true;
        await operation(cancellationToken);
    }

    public async Task<T> ExecuteTransactionAsync<T>(Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken = default)
    {
        TransactionExecuted = true;
        return await operation(cancellationToken);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(0);
    }
}


