namespace EngineeringKnowledgeHub.Tests.Application;

using EngineeringKnowledgeHub.Application.Common.Exceptions;
using EngineeringKnowledgeHub.Application.Common.Services;
using EngineeringKnowledgeHub.Application.Documents;
using EngineeringKnowledgeHub.Application.SemanticSearch;
using EngineeringKnowledgeHub.Domain.Entities;
using EngineeringKnowledgeHub.Domain.Enums;
using EngineeringKnowledgeHub.Tests.Common;
using Xunit;

public class TextChunkerTests
{
    private readonly TextChunker _chunker = new();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ChunkText_WithEmptyOrWhitespaceInput_ShouldReturnEmptyList(string? emptyInput)
    {
        var chunks = _chunker.ChunkText(emptyInput!);

        Assert.NotNull(chunks);
        Assert.Empty(chunks);
    }

    [Fact]
    public void ChunkText_WithShortInput_SmallerThanChunkSize_ShouldReturnSingleChunk()
    {
        var text = "Short text example for chunking.";
        var options = new ChunkingOptions(ChunkSize: 100, ChunkOverlap: 20);

        var chunks = _chunker.ChunkText(text, options);

        Assert.Single(chunks);
        Assert.Equal(0, chunks[0].Index);
        Assert.Equal(text, chunks[0].Content);
        Assert.Equal(0, chunks[0].StartOffset);
        Assert.Equal(text.Length, chunks[0].Length);
        Assert.True(chunks[0].TokenCount > 0);
    }

    [Fact]
    public void ChunkText_WithMultipleChunks_ShouldSplitDeterministicallyWithSequentialIndices()
    {
        // 250 characters
        var text = new string('A', 100) + " " + new string('B', 100) + " " + new string('C', 48);
        var options = new ChunkingOptions(ChunkSize: 100, ChunkOverlap: 20); // step = 80

        var chunks = _chunker.ChunkText(text, options);

        Assert.True(chunks.Count >= 3);
        for (int i = 0; i < chunks.Count; i++)
        {
            Assert.Equal(i, chunks[i].Index);
            Assert.True(chunks[i].Length <= 100);
            Assert.False(string.IsNullOrWhiteSpace(chunks[i].Content));
        }
    }

    [Fact]
    public void ChunkText_WithOverlap_ShouldContainOverlappingTextBetweenConsecutiveChunks()
    {
        var text = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        var options = new ChunkingOptions(ChunkSize: 20, ChunkOverlap: 5); // step = 15

        var chunks = _chunker.ChunkText(text, options);

        Assert.True(chunks.Count > 1);

        for (int i = 0; i < chunks.Count - 1; i++)
        {
            var current = chunks[i];
            var next = chunks[i + 1];

            // Next chunk offset should be current offset + step (15)
            Assert.Equal(current.StartOffset + 15, next.StartOffset);

            // The overlapping region of length 5 should match
            var overlapFromCurrent = current.Content.Substring(current.Content.Length - 5);
            var overlapFromNext = next.Content.Substring(0, 5);
            Assert.Equal(overlapFromCurrent, overlapFromNext);
        }
    }

    [Fact]
    public void ChunkText_DeterministicOutput_ShouldProduceIdenticalResultsForSameInput()
    {
        var text = "A sample text that will be chunked into multiple pieces to test deterministic behavior.";
        var options = new ChunkingOptions(ChunkSize: 30, ChunkOverlap: 10);

        var run1 = _chunker.ChunkText(text, options);
        var run2 = _chunker.ChunkText(text, options);

        Assert.Equal(run1.Count, run2.Count);
        for (int i = 0; i < run1.Count; i++)
        {
            Assert.Equal(run1[i].Index, run2[i].Index);
            Assert.Equal(run1[i].Content, run2[i].Content);
            Assert.Equal(run1[i].StartOffset, run2[i].StartOffset);
            Assert.Equal(run1[i].Length, run2[i].Length);
            Assert.Equal(run1[i].TokenCount, run2[i].TokenCount);
        }
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(-10, 5)]
    [InlineData(50, -1)]
    [InlineData(50, 50)]
    [InlineData(50, 60)]
    public void ChunkText_WithInvalidConfiguration_ShouldThrowValidationException(int chunkSize, int overlap)
    {
        Assert.Throws<ValidationException>(() =>
            _chunker.ChunkText("Sample text", new ChunkingOptions(chunkSize, overlap)));
    }

    [Fact]
    public void ChunkDocument_ShouldPreserveDocumentIdentityAndSequentialChunkIndices()
    {
        var docId = Guid.NewGuid();
        var text = "First segment of the document that is long enough to generate multiple chunks. Second segment containing further diagnostics and configuration instructions.";
        var options = new ChunkingOptions(ChunkSize: 50, ChunkOverlap: 10);

        var docChunks = _chunker.ChunkDocument(docId, text, options);

        Assert.NotEmpty(docChunks);
        for (int i = 0; i < docChunks.Count; i++)
        {
            Assert.Equal(docId, docChunks[i].DocumentId);
            Assert.Equal(i, docChunks[i].ChunkIndex);
            Assert.NotEqual(Guid.Empty, docChunks[i].Id);
            Assert.Null(docChunks[i].EmbeddingReference);
            Assert.False(string.IsNullOrWhiteSpace(docChunks[i].Content));
        }
    }

    [Fact]
    public void ChunkDocument_WithEmptyDocumentId_ShouldThrowValidationException()
    {
        Assert.Throws<ValidationException>(() =>
            _chunker.ChunkDocument(Guid.Empty, "Valid content text"));
    }

    [Fact]
    public async Task DocumentService_ChunkDocumentAsync_WhenDocumentProcessed_ShouldChunkAndPersist()
    {
        var docRepo = new InMemoryDocumentRepository();
        var sourceRepo = new InMemoryKnowledgeSourceRepository();
        var chunkRepo = new InMemoryDocumentChunkRepository();
        var hasher = new Sha256FileHasher();
        var extractor = new PlainTextDocumentExtractor();
        var dateTimeProvider = new FakeDateTimeProvider(DateTimeOffset.UtcNow);

        var service = new DocumentService(
            docRepo,
            sourceRepo,
            hasher,
            extractor,
            dateTimeProvider,
            _chunker,
            chunkRepo);

        var source = new KnowledgeSource(Guid.NewGuid(), "Test Manual", SourceType.Internal, dateTimeProvider.UtcNow);
        await sourceRepo.AddAsync(source);

        var doc = new Document(
            Guid.NewGuid(),
            "manual.txt",
            "docs/manual.txt",
            "text/plain",
            "hash123",
            source.Id,
            Guid.NewGuid(),
            dateTimeProvider.UtcNow);

        doc.MarkProcessing();
        doc.MarkProcessed("This is the extracted text from the processed document. It provides instructions for barrier configuration.", dateTimeProvider.UtcNow);
        await docRepo.AddAsync(doc);

        var chunkDtos = await service.ChunkDocumentAsync(doc.Id, new ChunkingOptions(ChunkSize: 40, ChunkOverlap: 10));

        Assert.NotEmpty(chunkDtos);
        var persistedChunks = await chunkRepo.GetByDocumentIdAsync(doc.Id);
        Assert.Equal(chunkDtos.Count, persistedChunks.Count);
        Assert.All(persistedChunks, c => Assert.Equal(doc.Id, c.DocumentId));
    }

    [Fact]
    public async Task DocumentService_ChunkDocumentAsync_WhenDocumentPendingOrFailed_ShouldThrowValidationException()
    {
        var docRepo = new InMemoryDocumentRepository();
        var sourceRepo = new InMemoryKnowledgeSourceRepository();
        var chunkRepo = new InMemoryDocumentChunkRepository();
        var hasher = new Sha256FileHasher();
        var extractor = new PlainTextDocumentExtractor();
        var dateTimeProvider = new FakeDateTimeProvider(DateTimeOffset.UtcNow);

        var service = new DocumentService(
            docRepo,
            sourceRepo,
            hasher,
            extractor,
            dateTimeProvider,
            _chunker,
            chunkRepo);

        var doc = new Document(
            Guid.NewGuid(),
            "unprocessed.txt",
            "docs/unprocessed.txt",
            "text/plain",
            "hash999",
            Guid.NewGuid(),
            Guid.NewGuid(),
            dateTimeProvider.UtcNow);

        await docRepo.AddAsync(doc);

        await Assert.ThrowsAsync<ValidationException>(() =>
            service.ChunkDocumentAsync(doc.Id));
    }
}
