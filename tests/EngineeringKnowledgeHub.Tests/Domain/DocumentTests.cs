namespace EngineeringKnowledgeHub.Tests.Domain;

using EngineeringKnowledgeHub.Domain.Entities;
using EngineeringKnowledgeHub.Domain.Enums;
using EngineeringKnowledgeHub.Domain.Exceptions;
using Xunit;

public class DocumentTests
{
    private readonly Guid _sourceId = Guid.NewGuid();
    private readonly Guid _uploadedById = Guid.NewGuid();
    private readonly DateTimeOffset _now = DateTimeOffset.UtcNow;

    private Document CreateValidDocument()
    {
        return new Document(
            Guid.NewGuid(),
            "protocol_spec.pdf",
            "storage/protocol_spec.pdf",
            "application/pdf",
            "a591a6d40bf420404a011733cfb7b190d62c65bf0bcda32b57b277d9ad9f146e",
            _sourceId,
            _uploadedById,
            _now);
    }

    [Fact]
    public void Constructor_With_Valid_Parameters_Should_Initialize_In_Pending_Status()
    {
        var doc = CreateValidDocument();

        Assert.NotEqual(Guid.Empty, doc.Id);
        Assert.Equal("protocol_spec.pdf", doc.FileName);
        Assert.Equal("storage/protocol_spec.pdf", doc.StoragePath);
        Assert.Equal("application/pdf", doc.ContentType);
        Assert.Equal("a591a6d40bf420404a011733cfb7b190d62c65bf0bcda32b57b277d9ad9f146e", doc.FileHash);
        Assert.Equal(_sourceId, doc.SourceId);
        Assert.Equal(_uploadedById, doc.UploadedById);
        Assert.Equal(DocumentProcessingStatus.Pending, doc.ProcessingStatus);
        Assert.Equal(_now, doc.CreatedAt);
        Assert.Null(doc.ProcessedAt);
        Assert.Null(doc.FailureReason);
        Assert.Null(doc.ExtractedText);
    }

    [Fact]
    public void Constructor_With_Empty_Id_Should_Throw_DomainRuleViolationException()
    {
        Assert.Throws<DomainRuleViolationException>(() => new Document(
            Guid.Empty,
            "file.pdf",
            "path",
            "type",
            "hash",
            _sourceId,
            _uploadedById,
            _now));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_With_Invalid_FileName_Should_Throw_DomainRuleViolationException(string? fileName)
    {
        Assert.Throws<DomainRuleViolationException>(() => new Document(
            Guid.NewGuid(),
            fileName!,
            "path",
            "type",
            "hash",
            _sourceId,
            _uploadedById,
            _now));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_With_Invalid_FileHash_Should_Throw_DomainRuleViolationException(string? fileHash)
    {
        Assert.Throws<DomainRuleViolationException>(() => new Document(
            Guid.NewGuid(),
            "file.pdf",
            "path",
            "type",
            fileHash!,
            _sourceId,
            _uploadedById,
            _now));
    }

    [Fact]
    public void MarkProcessing_From_Pending_Should_Transition_To_Processing()
    {
        var doc = CreateValidDocument();

        doc.MarkProcessing();

        Assert.Equal(DocumentProcessingStatus.Processing, doc.ProcessingStatus);
    }

    [Fact]
    public void MarkProcessing_From_Processing_Should_Throw_DomainRuleViolationException()
    {
        var doc = CreateValidDocument();
        doc.MarkProcessing();

        Assert.Throws<DomainRuleViolationException>(() => doc.MarkProcessing());
    }

    [Fact]
    public void MarkProcessed_From_Processing_Should_Set_Status_And_ExtractedText()
    {
        var doc = CreateValidDocument();
        doc.MarkProcessing();

        var processedAt = _now.AddMinutes(5);
        doc.MarkProcessed("Extracted specification text...", processedAt);

        Assert.Equal(DocumentProcessingStatus.Processed, doc.ProcessingStatus);
        Assert.Equal("Extracted specification text...", doc.ExtractedText);
        Assert.Equal(processedAt, doc.ProcessedAt);
        Assert.Null(doc.FailureReason);
    }

    [Fact]
    public void MarkProcessed_From_Pending_Should_Throw_DomainRuleViolationException()
    {
        var doc = CreateValidDocument();

        Assert.Throws<DomainRuleViolationException>(() =>
            doc.MarkProcessed("text", _now.AddMinutes(1)));
    }

    [Fact]
    public void MarkFailed_Should_Set_Failed_Status_And_Reason()
    {
        var doc = CreateValidDocument();
        doc.MarkProcessing();

        var failedAt = _now.AddMinutes(3);
        doc.MarkFailed("Corrupt file stream.", failedAt);

        Assert.Equal(DocumentProcessingStatus.Failed, doc.ProcessingStatus);
        Assert.Equal("Corrupt file stream.", doc.FailureReason);
        Assert.Equal(failedAt, doc.ProcessedAt);
    }

    [Fact]
    public void MarkProcessing_From_Failed_Should_Allow_Retry()
    {
        var doc = CreateValidDocument();
        doc.MarkProcessing();
        doc.MarkFailed("Network timeout during parsing.", _now.AddMinutes(1));

        doc.MarkProcessing();

        Assert.Equal(DocumentProcessingStatus.Processing, doc.ProcessingStatus);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void MarkFailed_With_Empty_Reason_Should_Throw_DomainRuleViolationException(string? reason)
    {
        var doc = CreateValidDocument();
        doc.MarkProcessing();

        Assert.Throws<DomainRuleViolationException>(() =>
            doc.MarkFailed(reason!, _now.AddMinutes(1)));
    }
}
