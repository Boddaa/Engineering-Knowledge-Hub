namespace EngineeringKnowledgeHub.Tests.Application;

using System.Text;
using EngineeringKnowledgeHub.Application.Common.Exceptions;
using EngineeringKnowledgeHub.Application.Common.Services;
using EngineeringKnowledgeHub.Application.Documents;
using EngineeringKnowledgeHub.Domain.Entities;
using EngineeringKnowledgeHub.Domain.Enums;
using EngineeringKnowledgeHub.Tests.Common;
using Xunit;

public class DocumentServiceTests
{
    private readonly InMemoryDocumentRepository _documentRepository = new();
    private readonly InMemoryKnowledgeSourceRepository _sourceRepository = new();
    private readonly Sha256FileHasher _fileHasher = new();
    private readonly PlainTextDocumentExtractor _textExtractor = new();
    private readonly FakeDateTimeProvider _dateTimeProvider = new(DateTimeOffset.UtcNow);
    private readonly DocumentService _service;

    public DocumentServiceTests()
    {
        _service = new DocumentService(
            _documentRepository,
            _sourceRepository,
            _fileHasher,
            _textExtractor,
            _dateTimeProvider);
    }

    private async Task<KnowledgeSource> CreateSampleSourceAsync()
    {
        var source = new KnowledgeSource(
            Guid.NewGuid(),
            "Camera Diagnostics Manual",
            SourceType.VendorDocumentation,
            _dateTimeProvider.UtcNow);

        await _sourceRepository.AddAsync(source);
        return source;
    }

    [Fact]
    public async Task UploadDocumentAsync_With_Valid_Parameters_Should_Compute_Sha256_And_Persist()
    {
        var source = await CreateSampleSourceAsync();
        var content = Encoding.UTF8.GetBytes("# Firmware Update Guide\nSteps to update camera firmware.");

        var command = new UploadDocumentCommand(
            "firmware_guide.md",
            "text/markdown",
            source.Id,
            Guid.NewGuid(),
            content);

        var result = await _service.UploadDocumentAsync(command);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("firmware_guide.md", result.FileName);
        Assert.Equal("text/markdown", result.ContentType);
        Assert.Equal(source.Id, result.SourceId);
        Assert.Equal(DocumentProcessingStatus.Pending, result.ProcessingStatus);
        Assert.False(string.IsNullOrWhiteSpace(result.FileHash));
        Assert.Equal(_fileHasher.ComputeHash(content), result.FileHash);
        Assert.Equal(_dateTimeProvider.UtcNow, result.CreatedAt);

        var persisted = await _documentRepository.GetByIdAsync(result.Id);
        Assert.NotNull(persisted);
        Assert.Equal(result.FileHash, persisted.FileHash);
    }

    [Fact]
    public async Task UploadDocumentAsync_With_Null_Command_Should_Throw_ArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _service.UploadDocumentAsync(null!));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task UploadDocumentAsync_With_Invalid_FileName_Should_Throw_ValidationException(string? fileName)
    {
        var source = await CreateSampleSourceAsync();
        var command = new UploadDocumentCommand(
            fileName!,
            "text/plain",
            source.Id,
            Guid.NewGuid(),
            Encoding.UTF8.GetBytes("test"));

        await Assert.ThrowsAsync<ValidationException>(() =>
            _service.UploadDocumentAsync(command));
    }

    [Fact]
    public async Task UploadDocumentAsync_With_Empty_Content_Should_Throw_ValidationException()
    {
        var source = await CreateSampleSourceAsync();
        var command = new UploadDocumentCommand(
            "file.txt",
            "text/plain",
            source.Id,
            Guid.NewGuid(),
            []);

        await Assert.ThrowsAsync<ValidationException>(() =>
            _service.UploadDocumentAsync(command));
    }

    [Fact]
    public async Task UploadDocumentAsync_When_Source_Not_Found_Should_Throw_NotFoundException()
    {
        var command = new UploadDocumentCommand(
            "file.txt",
            "text/plain",
            Guid.NewGuid(),
            Guid.NewGuid(),
            Encoding.UTF8.GetBytes("sample content"));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.UploadDocumentAsync(command));
    }

    [Fact]
    public async Task UploadDocumentAsync_With_Duplicate_Hash_Should_Throw_ConflictException()
    {
        var source = await CreateSampleSourceAsync();
        var content = Encoding.UTF8.GetBytes("Identical file content for deduplication test");

        var firstCommand = new UploadDocumentCommand(
            "first.txt",
            "text/plain",
            source.Id,
            Guid.NewGuid(),
            content);
        await _service.UploadDocumentAsync(firstCommand);

        var secondCommand = new UploadDocumentCommand(
            "second_copy.txt",
            "text/plain",
            source.Id,
            Guid.NewGuid(),
            content);

        await Assert.ThrowsAsync<ConflictException>(() =>
            _service.UploadDocumentAsync(secondCommand));
    }

    [Fact]
    public async Task GetDocumentByIdAsync_When_Found_Should_Return_Dto()
    {
        var source = await CreateSampleSourceAsync();
        var command = new UploadDocumentCommand(
            "doc.txt",
            "text/plain",
            source.Id,
            Guid.NewGuid(),
            Encoding.UTF8.GetBytes("sample"));

        var uploaded = await _service.UploadDocumentAsync(command);
        var result = await _service.GetDocumentByIdAsync(uploaded.Id);

        Assert.Equal(uploaded.Id, result.Id);
        Assert.Equal("doc.txt", result.FileName);
    }

    [Fact]
    public async Task GetDocumentByIdAsync_When_Not_Found_Should_Throw_NotFoundException()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.GetDocumentByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetDocumentsBySourceIdAsync_Should_Return_Documents_For_Source()
    {
        var source = await CreateSampleSourceAsync();
        var cmd1 = new UploadDocumentCommand("doc1.txt", "text/plain", source.Id, Guid.NewGuid(), Encoding.UTF8.GetBytes("one"));
        var cmd2 = new UploadDocumentCommand("doc2.txt", "text/plain", source.Id, Guid.NewGuid(), Encoding.UTF8.GetBytes("two"));

        await _service.UploadDocumentAsync(cmd1);
        await _service.UploadDocumentAsync(cmd2);

        var docs = await _service.GetDocumentsBySourceIdAsync(source.Id);

        Assert.Equal(2, docs.Count);
    }

    [Fact]
    public async Task GetDocumentsBySourceIdAsync_When_Source_Not_Found_Should_Throw_NotFoundException()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.GetDocumentsBySourceIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task ProcessDocumentAsync_With_Text_Document_Should_Extract_Text_And_Mark_Processed()
    {
        var source = await CreateSampleSourceAsync();
        var textContent = "Diagnostic log entry: Code 0x123 - Sensor calibration OK.";
        var bytes = Encoding.UTF8.GetBytes(textContent);

        var uploadCmd = new UploadDocumentCommand("diag.log", "text/plain", source.Id, Guid.NewGuid(), bytes);
        var uploaded = await _service.UploadDocumentAsync(uploadCmd);

        _dateTimeProvider.UtcNow = _dateTimeProvider.UtcNow.AddMinutes(5);
        var processCmd = new ProcessDocumentCommand(uploaded.Id, bytes);
        var processed = await _service.ProcessDocumentAsync(processCmd);

        Assert.Equal(DocumentProcessingStatus.Processed, processed.ProcessingStatus);
        Assert.Equal(textContent, processed.ExtractedText);
        Assert.Equal(_dateTimeProvider.UtcNow, processed.ProcessedAt);
        Assert.Null(processed.FailureReason);
    }

    [Fact]
    public async Task ProcessDocumentAsync_Without_Content_Should_Mark_Failed()
    {
        var source = await CreateSampleSourceAsync();
        var bytes = Encoding.UTF8.GetBytes("some file");

        var uploadCmd = new UploadDocumentCommand("file.txt", "text/plain", source.Id, Guid.NewGuid(), bytes);
        var uploaded = await _service.UploadDocumentAsync(uploadCmd);

        var processCmd = new ProcessDocumentCommand(uploaded.Id, null);
        var failed = await _service.ProcessDocumentAsync(processCmd);

        Assert.Equal(DocumentProcessingStatus.Failed, failed.ProcessingStatus);
        Assert.Contains("not available", failed.FailureReason);
    }

    [Fact]
    public async Task ProcessDocumentAsync_When_Not_Found_Should_Throw_NotFoundException()
    {
        var processCmd = new ProcessDocumentCommand(Guid.NewGuid(), Encoding.UTF8.GetBytes("test"));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.ProcessDocumentAsync(processCmd));
    }

    [Fact]
    public async Task ProcessDocumentAsync_When_Extractor_Throws_Should_Mark_Failed_With_Sanitized_Reason()
    {
        var faultyExtractor = new FaultyTextExtractor { ThrowException = true };
        var service = new DocumentService(
            _documentRepository,
            _sourceRepository,
            _fileHasher,
            faultyExtractor,
            _dateTimeProvider);

        var source = await CreateSampleSourceAsync();
        var uploadCmd = new UploadDocumentCommand("corrupt.txt", "text/plain", source.Id, Guid.NewGuid(), Encoding.UTF8.GetBytes("corrupt content"));
        var uploaded = await service.UploadDocumentAsync(uploadCmd);

        var processCmd = new ProcessDocumentCommand(uploaded.Id, Encoding.UTF8.GetBytes("corrupt content"));
        var failed = await service.ProcessDocumentAsync(processCmd);

        Assert.Equal(DocumentProcessingStatus.Failed, failed.ProcessingStatus);
        Assert.Equal("Document extraction failed due to an unexpected processing error.", failed.FailureReason);
        Assert.DoesNotContain("Raw sensitive", failed.FailureReason);

        var persisted = await _documentRepository.GetByIdAsync(uploaded.Id);
        Assert.Equal(DocumentProcessingStatus.Failed, persisted!.ProcessingStatus);
        Assert.Equal("Document extraction failed due to an unexpected processing error.", persisted.FailureReason);
    }

    [Fact]
    public async Task ProcessDocumentAsync_When_Cancelled_Should_Propagate_And_Not_Mark_Document_As_Failed()
    {
        var faultyExtractor = new FaultyTextExtractor { ThrowCancellation = true };
        var service = new DocumentService(
            _documentRepository,
            _sourceRepository,
            _fileHasher,
            faultyExtractor,
            _dateTimeProvider);

        var source = await CreateSampleSourceAsync();
        var uploadCmd = new UploadDocumentCommand("cancel.txt", "text/plain", source.Id, Guid.NewGuid(), Encoding.UTF8.GetBytes("data"));
        var uploaded = await service.UploadDocumentAsync(uploadCmd);

        var processCmd = new ProcessDocumentCommand(uploaded.Id, Encoding.UTF8.GetBytes("data"));

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            service.ProcessDocumentAsync(processCmd));

        var persisted = await _documentRepository.GetByIdAsync(uploaded.Id);
        Assert.NotNull(persisted);
        Assert.NotEqual(DocumentProcessingStatus.Failed, persisted.ProcessingStatus);
        Assert.Null(persisted.FailureReason);
    }

    [Fact]
    public async Task ProcessDocumentAsync_When_Failed_Should_Allow_Retry_To_Success()
    {
        var faultyExtractor = new FaultyTextExtractor { ThrowException = true };
        var service = new DocumentService(
            _documentRepository,
            _sourceRepository,
            _fileHasher,
            faultyExtractor,
            _dateTimeProvider);

        var source = await CreateSampleSourceAsync();
        var uploadCmd = new UploadDocumentCommand("retry.txt", "text/plain", source.Id, Guid.NewGuid(), Encoding.UTF8.GetBytes("content"));
        var uploaded = await service.UploadDocumentAsync(uploadCmd);

        var processCmd = new ProcessDocumentCommand(uploaded.Id, Encoding.UTF8.GetBytes("content"));
        var failed = await service.ProcessDocumentAsync(processCmd);
        Assert.Equal(DocumentProcessingStatus.Failed, failed.ProcessingStatus);

        // Fix the issue and retry
        faultyExtractor.ThrowException = false;
        var retried = await service.ProcessDocumentAsync(processCmd);

        Assert.Equal(DocumentProcessingStatus.Processed, retried.ProcessingStatus);
        Assert.Equal("Extracted text", retried.ExtractedText);
        Assert.Null(retried.FailureReason);

        var persisted = await _documentRepository.GetByIdAsync(uploaded.Id);
        Assert.Equal(DocumentProcessingStatus.Processed, persisted!.ProcessingStatus);
        Assert.Null(persisted.FailureReason);
    }

    private class FaultyTextExtractor : EngineeringKnowledgeHub.Application.Common.Interfaces.IDocumentTextExtractor
    {
        public bool ThrowCancellation { get; set; }
        public bool ThrowException { get; set; }

        public bool CanExtract(string contentType, string fileName) => true;

        public Task<string> ExtractTextAsync(Stream contentStream, string contentType, CancellationToken cancellationToken = default)
        {
            if (ThrowCancellation)
            {
                throw new OperationCanceledException(cancellationToken);
            }

            if (ThrowException)
            {
                throw new InvalidOperationException("Raw sensitive internal exception details with database path");
            }

            return Task.FromResult("Extracted text");
        }
    }
}
