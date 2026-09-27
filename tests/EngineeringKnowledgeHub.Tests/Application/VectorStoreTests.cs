namespace EngineeringKnowledgeHub.Tests.Application;

using EngineeringKnowledgeHub.Application.Common.Exceptions;
using EngineeringKnowledgeHub.Application.SemanticSearch;
using EngineeringKnowledgeHub.Domain.Enums;
using Xunit;

public class VectorStoreTests
{
    private readonly InMemoryVectorStore _store = new();
    private readonly DeterministicEmbeddingProvider _embeddingProvider = new(dimensions: 16);

    [Fact]
    public async Task UpsertAsync_And_GetByIdAsync_ShouldPersistAndRetrieveRecord()
    {
        var vector = await _embeddingProvider.GenerateEmbeddingAsync("Test sample content.");
        var record = new VectorRecord("rec-1", vector, Guid.NewGuid(), null, null, "Test sample content.", new Dictionary<string, string>());

        await _store.UpsertAsync(record);

        var retrieved = await _store.GetByIdAsync("rec-1");

        Assert.NotNull(retrieved);
        Assert.Equal("rec-1", retrieved.Id);
        Assert.Equal("Test sample content.", retrieved.Text);
        Assert.Equal(vector, retrieved.Vector);
        Assert.Equal(1, await _store.CountAsync());
    }

    [Fact]
    public async Task UpsertAsync_WithExistingId_ShouldUpdateRecordWithoutDuplicating()
    {
        var v1 = await _embeddingProvider.GenerateEmbeddingAsync("Original text.");
        var v2 = await _embeddingProvider.GenerateEmbeddingAsync("Updated text.");

        var r1 = new VectorRecord("rec-1", v1, null, null, null, "Original text.", new Dictionary<string, string>());
        var r2 = new VectorRecord("rec-1", v2, null, null, null, "Updated text.", new Dictionary<string, string>());

        await _store.UpsertAsync(r1);
        await _store.UpsertAsync(r2);

        var retrieved = await _store.GetByIdAsync("rec-1");

        Assert.NotNull(retrieved);
        Assert.Equal("Updated text.", retrieved.Text);
        Assert.Equal(v2, retrieved.Vector);
        Assert.Equal(1, await _store.CountAsync());
    }

    [Fact]
    public async Task DeleteAsync_WhenRecordExists_ShouldRemoveAndReturnTrue()
    {
        var vector = await _embeddingProvider.GenerateEmbeddingAsync("Content to delete.");
        var record = new VectorRecord("rec-del", vector, null, null, null, "Content to delete.", new Dictionary<string, string>());

        await _store.UpsertAsync(record);
        var removed = await _store.DeleteAsync("rec-del");

        Assert.True(removed);
        Assert.Null(await _store.GetByIdAsync("rec-del"));
        Assert.Equal(0, await _store.CountAsync());
    }

    [Fact]
    public async Task DeleteByDocumentIdAsync_ShouldRemoveAllDocumentChunks()
    {
        var docId = Guid.NewGuid();
        var v = await _embeddingProvider.GenerateEmbeddingAsync("Chunk");

        var c1 = VectorRecord.ForDocumentChunk(docId, Guid.NewGuid(), 0, "Chunk 0", v);
        var c2 = VectorRecord.ForDocumentChunk(docId, Guid.NewGuid(), 1, "Chunk 1", v);
        var other = VectorRecord.ForDocumentChunk(Guid.NewGuid(), Guid.NewGuid(), 0, "Other doc chunk", v);

        await _store.UpsertBatchAsync([c1, c2, other]);
        Assert.Equal(3, await _store.CountAsync());

        var deletedCount = await _store.DeleteByDocumentIdAsync(docId);

        Assert.Equal(2, deletedCount);
        Assert.Equal(1, await _store.CountAsync());
        Assert.Null(await _store.GetByIdAsync(c1.Id));
        Assert.Null(await _store.GetByIdAsync(c2.Id));
        Assert.NotNull(await _store.GetByIdAsync(other.Id));
    }

    [Fact]
    public async Task DeleteByArticleIdAsync_ShouldRemoveArticleVector()
    {
        var articleId = Guid.NewGuid();
        var v = await _embeddingProvider.GenerateEmbeddingAsync("Article content");
        var r = VectorRecord.ForArticle(articleId, "Article content", v);

        await _store.UpsertAsync(r);
        Assert.Equal(1, await _store.CountAsync());

        var count = await _store.DeleteByArticleIdAsync(articleId);

        Assert.Equal(1, count);
        Assert.Equal(0, await _store.CountAsync());
    }

    [Fact]
    public async Task SearchNearestAsync_OnEmptyStore_ShouldReturnEmptyList()
    {
        var query = await _embeddingProvider.GenerateEmbeddingAsync("Sample query.");

        var results = await _store.SearchNearestAsync(query, topK: 5);

        Assert.NotNull(results);
        Assert.Empty(results);
    }

    [Fact]
    public async Task SearchNearestAsync_ShouldRankCandidatesBySimilarityAndLimitToTopK()
    {
        var queryVec = await _embeddingProvider.GenerateEmbeddingAsync("Camera barrier communication failure");
        var highMatchVec = await _embeddingProvider.GenerateEmbeddingAsync("Camera barrier communication failure error log");
        var medMatchVec = await _embeddingProvider.GenerateEmbeddingAsync("Barrier gate maintenance procedure");
        var lowMatchVec = await _embeddingProvider.GenerateEmbeddingAsync("Database SQL user permissions and roles");

        await _store.UpsertAsync(new VectorRecord("high", highMatchVec, null, null, null, "High match", new Dictionary<string, string>()));
        await _store.UpsertAsync(new VectorRecord("med", medMatchVec, null, null, null, "Med match", new Dictionary<string, string>()));
        await _store.UpsertAsync(new VectorRecord("low", lowMatchVec, null, null, null, "Low match", new Dictionary<string, string>()));

        var results = await _store.SearchNearestAsync(queryVec, topK: 2);

        Assert.Equal(2, results.Count);
        Assert.Equal("high", results[0].Record.Id);
        Assert.True(results[0].SimilarityScore >= results[1].SimilarityScore);
    }

    [Fact]
    public async Task SearchNearestAsync_WithMetadataFilters_ShouldOnlyReturnMatchingCandidates()
    {
        var queryVec = await _embeddingProvider.GenerateEmbeddingAsync("Diagnostic manual");
        var prodA = Guid.NewGuid();
        var prodB = Guid.NewGuid();

        var r1 = VectorRecord.ForArticle(Guid.NewGuid(), "Camera diagnostics", queryVec, productIds: [prodA], status: KnowledgeStatus.Published);
        var r2 = VectorRecord.ForArticle(Guid.NewGuid(), "Barrier diagnostics", queryVec, productIds: [prodB], status: KnowledgeStatus.Published);
        var r3 = VectorRecord.ForArticle(Guid.NewGuid(), "Draft diagnostics", queryVec, productIds: [prodA], status: KnowledgeStatus.Draft);

        await _store.UpsertBatchAsync([r1, r2, r3]);

        var filter = new VectorSearchFilter(ProductId: prodA, Status: KnowledgeStatus.Published);
        var results = await _store.SearchNearestAsync(queryVec, topK: 10, filter: filter);

        Assert.Single(results);
        Assert.Equal(r1.Id, results[0].Record.Id);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task SearchNearestAsync_WithInvalidTopK_ShouldThrowValidationException(int invalidTopK)
    {
        var query = await _embeddingProvider.GenerateEmbeddingAsync("Query");

        await Assert.ThrowsAsync<ValidationException>(() =>
            _store.SearchNearestAsync(query, topK: invalidTopK));
    }
}
