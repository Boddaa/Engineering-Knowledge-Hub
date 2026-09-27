namespace EngineeringKnowledgeHub.Tests.Domain;

using EngineeringKnowledgeHub.Domain.Entities;
using EngineeringKnowledgeHub.Domain.Exceptions;
using Xunit;

public class DocumentChunkTests
{
    private readonly Guid _chunkId = Guid.NewGuid();
    private readonly Guid _documentId = Guid.NewGuid();
    private readonly DateTimeOffset _now = DateTimeOffset.UtcNow;

    [Fact]
    public void Constructor_WithValidArguments_ShouldInstantiate()
    {
        var chunk = new DocumentChunk(
            _chunkId,
            _documentId,
            0,
            "This is chunk content.",
            4,
            "ref-123",
            _now);

        Assert.Equal(_chunkId, chunk.Id);
        Assert.Equal(_documentId, chunk.DocumentId);
        Assert.Equal(0, chunk.ChunkIndex);
        Assert.Equal("This is chunk content.", chunk.Content);
        Assert.Equal(4, chunk.TokenCount);
        Assert.Equal("ref-123", chunk.EmbeddingReference);
        Assert.Equal(_now, chunk.CreatedAt);
    }

    [Fact]
    public void Constructor_WithEmptyId_ShouldThrowDomainRuleViolationException()
    {
        Assert.Throws<DomainRuleViolationException>(() =>
            new DocumentChunk(Guid.Empty, _documentId, 0, "Content", 1, null, _now));
    }

    [Fact]
    public void Constructor_WithEmptyDocumentId_ShouldThrowDomainRuleViolationException()
    {
        Assert.Throws<DomainRuleViolationException>(() =>
            new DocumentChunk(_chunkId, Guid.Empty, 0, "Content", 1, null, _now));
    }

    [Fact]
    public void Constructor_WithNegativeChunkIndex_ShouldThrowDomainRuleViolationException()
    {
        Assert.Throws<DomainRuleViolationException>(() =>
            new DocumentChunk(_chunkId, _documentId, -1, "Content", 1, null, _now));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithNullOrWhitespaceContent_ShouldThrowDomainRuleViolationException(string? invalidContent)
    {
        Assert.Throws<DomainRuleViolationException>(() =>
            new DocumentChunk(_chunkId, _documentId, 0, invalidContent!, 1, null, _now));
    }

    [Fact]
    public void Constructor_WithNegativeTokenCount_ShouldThrowDomainRuleViolationException()
    {
        Assert.Throws<DomainRuleViolationException>(() =>
            new DocumentChunk(_chunkId, _documentId, 0, "Content", -1, null, _now));
    }

    [Fact]
    public void SetEmbeddingReference_WithValidReference_ShouldUpdateProperty()
    {
        var chunk = new DocumentChunk(_chunkId, _documentId, 0, "Content", 1, null, _now);

        chunk.SetEmbeddingReference("vec_456");

        Assert.Equal("vec_456", chunk.EmbeddingReference);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void SetEmbeddingReference_WithNullOrWhitespace_ShouldThrowDomainRuleViolationException(string? invalidRef)
    {
        var chunk = new DocumentChunk(_chunkId, _documentId, 0, "Content", 1, null, _now);

        Assert.Throws<DomainRuleViolationException>(() =>
            chunk.SetEmbeddingReference(invalidRef!));
    }
}
