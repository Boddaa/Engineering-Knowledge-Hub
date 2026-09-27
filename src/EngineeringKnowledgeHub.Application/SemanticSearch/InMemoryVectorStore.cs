namespace EngineeringKnowledgeHub.Application.SemanticSearch;

using System.Collections.Concurrent;
using EngineeringKnowledgeHub.Application.Common.Exceptions;

/// <summary>
/// Thread-safe in-memory vector storage implementation.
/// Provides exact cosine-similarity nearest-neighbor search with structured metadata filtering.
/// Suitable for local-first retrieval, unit testing, and isolated evaluation.
/// </summary>
public sealed class InMemoryVectorStore : IVectorStore
{
    private readonly ConcurrentDictionary<string, VectorRecord> _records = new(StringComparer.Ordinal);

    public Task UpsertAsync(VectorRecord record, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(record);

        if (string.IsNullOrWhiteSpace(record.Id))
        {
            throw new ValidationException("VectorRecord Id cannot be null or whitespace.");
        }

        if (record.Vector is null)
        {
            throw new ValidationException("VectorRecord Vector cannot be null.");
        }

        _records[record.Id] = record;
        return Task.CompletedTask;
    }

    public Task UpsertBatchAsync(IReadOnlyList<VectorRecord> records, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(records);

        foreach (var record in records)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (record is null || string.IsNullOrWhiteSpace(record.Id) || record.Vector is null)
            {
                throw new ValidationException("Invalid vector record in batch.");
            }

            _records[record.Id] = record;
        }

        return Task.CompletedTask;
    }

    public Task<VectorRecord?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(id))
        {
            return Task.FromResult<VectorRecord?>(null);
        }

        _records.TryGetValue(id, out var record);
        return Task.FromResult(record);
    }

    public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(id))
        {
            return Task.FromResult(false);
        }

        var removed = _records.TryRemove(id, out _);
        return Task.FromResult(removed);
    }

    public Task<int> DeleteByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (documentId == Guid.Empty)
        {
            return Task.FromResult(0);
        }

        var toRemove = _records.Values
            .Where(r => r.DocumentId == documentId)
            .Select(r => r.Id)
            .ToList();

        int count = 0;
        foreach (var id in toRemove)
        {
            if (_records.TryRemove(id, out _))
            {
                count++;
            }
        }

        return Task.FromResult(count);
    }

    public Task<int> DeleteByArticleIdAsync(Guid articleId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (articleId == Guid.Empty)
        {
            return Task.FromResult(0);
        }

        var toRemove = _records.Values
            .Where(r => r.ArticleId == articleId)
            .Select(r => r.Id)
            .ToList();

        int count = 0;
        foreach (var id in toRemove)
        {
            if (_records.TryRemove(id, out _))
            {
                count++;
            }
        }

        return Task.FromResult(count);
    }

    public Task<IReadOnlyList<VectorSearchResult>> SearchNearestAsync(
        Vector queryVector,
        int topK,
        VectorSearchFilter? filter = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (queryVector is null)
        {
            throw new ValidationException("Query vector cannot be null.");
        }

        if (topK <= 0)
        {
            throw new ValidationException("TopK must be greater than 0.");
        }

        if (_records.IsEmpty)
        {
            return Task.FromResult<IReadOnlyList<VectorSearchResult>>(Array.Empty<VectorSearchResult>());
        }

        var candidates = _records.Values.AsEnumerable();

        if (filter != null)
        {
            candidates = candidates.Where(r => MatchesFilter(r, filter));
        }

        var results = new List<VectorSearchResult>();

        foreach (var record in candidates)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var rawSim = queryVector.CosineSimilarityTo(record.Vector);
            // Search metric semantics: non-negative similarity score [0.0, 1.0]
            var score = Math.Max(0.0, rawSim);

            results.Add(new VectorSearchResult(record, score));
        }

        // Deterministic ordering: descending by similarity score, tie-break by Record.Id ascending
        var sorted = results
            .OrderByDescending(r => r.SimilarityScore)
            .ThenBy(r => r.Record.Id, StringComparer.Ordinal)
            .Take(topK)
            .ToList();

        return Task.FromResult<IReadOnlyList<VectorSearchResult>>(sorted);
    }

    public Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_records.Count);
    }

    public Task ClearAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _records.Clear();
        return Task.CompletedTask;
    }

    private static bool MatchesFilter(VectorRecord record, VectorSearchFilter filter)
    {
        if (filter.ArticleId.HasValue && record.ArticleId != filter.ArticleId.Value)
        {
            return false;
        }

        if (filter.DocumentId.HasValue && record.DocumentId != filter.DocumentId.Value)
        {
            return false;
        }

        if (filter.KnowledgeTypeId.HasValue)
        {
            if (!record.Metadata.TryGetValue("KnowledgeTypeId", out var val) ||
                !Guid.TryParse(val, out var kId) ||
                kId != filter.KnowledgeTypeId.Value)
            {
                return false;
            }
        }

        if (filter.Status.HasValue)
        {
            if (!record.Metadata.TryGetValue("Status", out var val) ||
                !string.Equals(val, filter.Status.Value.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        if (filter.VerificationStatus.HasValue)
        {
            if (!record.Metadata.TryGetValue("VerificationStatus", out var val) ||
                !string.Equals(val, filter.VerificationStatus.Value.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        if (filter.ProductId.HasValue)
        {
            if (!record.Metadata.TryGetValue("ProductIds", out var val) ||
                !val.Split(',', StringSplitOptions.RemoveEmptyEntries).Any(p => string.Equals(p.Trim(), filter.ProductId.Value.ToString(), StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }
        }

        if (filter.TechnologyId.HasValue)
        {
            if (!record.Metadata.TryGetValue("TechnologyIds", out var val) ||
                !val.Split(',', StringSplitOptions.RemoveEmptyEntries).Any(t => string.Equals(t.Trim(), filter.TechnologyId.Value.ToString(), StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }
        }

        if (filter.TagId.HasValue)
        {
            if (!record.Metadata.TryGetValue("TagIds", out var val) ||
                !val.Split(',', StringSplitOptions.RemoveEmptyEntries).Any(t => string.Equals(t.Trim(), filter.TagId.Value.ToString(), StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }
        }

        if (filter.CustomMetadata != null && filter.CustomMetadata.Count > 0)
        {
            foreach (var kvp in filter.CustomMetadata)
            {
                if (!record.Metadata.TryGetValue(kvp.Key, out var val) ||
                    !string.Equals(val, kvp.Value, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }
        }

        return true;
    }
}
