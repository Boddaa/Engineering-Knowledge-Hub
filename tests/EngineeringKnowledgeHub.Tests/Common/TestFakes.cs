namespace EngineeringKnowledgeHub.Tests.Common;

using System.Collections.Concurrent;
using EngineeringKnowledgeHub.Application.Common.Interfaces;
using EngineeringKnowledgeHub.Domain.Entities;
using EngineeringKnowledgeHub.Domain.ValueObjects;

public class InMemoryKnowledgeArticleRepository : IKnowledgeArticleRepository
{
    private readonly ConcurrentDictionary<Guid, KnowledgeArticle> _articles = new();

    public Task<KnowledgeArticle?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _articles.TryGetValue(id, out var article);
        return Task.FromResult(article);
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
