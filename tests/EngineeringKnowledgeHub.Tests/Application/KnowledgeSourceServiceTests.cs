namespace EngineeringKnowledgeHub.Tests.Application;

using EngineeringKnowledgeHub.Application.Common.Exceptions;
using EngineeringKnowledgeHub.Application.DTOs;
using EngineeringKnowledgeHub.Application.Sources;
using EngineeringKnowledgeHub.Domain.Entities;
using EngineeringKnowledgeHub.Domain.Enums;
using EngineeringKnowledgeHub.Domain.ValueObjects;
using EngineeringKnowledgeHub.Tests.Common;
using Xunit;

public class KnowledgeSourceServiceTests
{
    private readonly InMemoryKnowledgeSourceRepository _sourceRepository = new();
    private readonly InMemoryKnowledgeArticleRepository _articleRepository = new();
    private readonly FakeDateTimeProvider _dateTimeProvider = new(DateTimeOffset.UtcNow);
    private readonly KnowledgeSourceService _service;

    public KnowledgeSourceServiceTests()
    {
        _service = new KnowledgeSourceService(
            _sourceRepository,
            _articleRepository,
            _dateTimeProvider);
    }

    private KnowledgeArticle CreateSampleArticle()
    {
        var article = new KnowledgeArticle(
            Guid.NewGuid(),
            "Camera Calibration Article",
            Slug.Create("camera-calibration-article"),
            "Summary of camera calibration",
            "Content steps...",
            Guid.NewGuid(),
            Guid.NewGuid(),
            _dateTimeProvider.UtcNow);

        _articleRepository.AddAsync(article).GetAwaiter().GetResult();
        return article;
    }

    [Fact]
    public async Task CreateSourceAsync_With_Valid_Command_Should_Create_And_Return_Dto()
    {
        var command = new CreateKnowledgeSourceCommand(
            "Hardware Vendor Manual",
            SourceType.VendorDocumentation,
            "https://vendor.example.com/manual",
            "Official documentation",
            TrustLevel.Official,
            true);

        var result = await _service.CreateSourceAsync(command);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("Hardware Vendor Manual", result.Name);
        Assert.Equal(SourceType.VendorDocumentation, result.SourceType);
        Assert.Equal("https://vendor.example.com/manual", result.Url);
        Assert.Equal("Official documentation", result.Description);
        Assert.Equal(TrustLevel.Official, result.TrustLevel);
        Assert.True(result.IsApproved);
        Assert.Equal(_dateTimeProvider.UtcNow, result.CreatedAt);

        var persisted = await _sourceRepository.GetByIdAsync(result.Id);
        Assert.NotNull(persisted);
    }

    [Fact]
    public async Task CreateSourceAsync_With_Null_Command_Should_Throw_ArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _service.CreateSourceAsync(null!));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task CreateSourceAsync_With_Invalid_Name_Should_Throw_ValidationException(string? name)
    {
        var command = new CreateKnowledgeSourceCommand(
            name!,
            SourceType.Internal);

        await Assert.ThrowsAsync<ValidationException>(() =>
            _service.CreateSourceAsync(command));
    }

    [Fact]
    public async Task CreateSourceAsync_With_Invalid_Url_Should_Throw_ValidationException()
    {
        var command = new CreateKnowledgeSourceCommand(
            "Valid Name",
            SourceType.External,
            "not-valid-url");

        await Assert.ThrowsAsync<ValidationException>(() =>
            _service.CreateSourceAsync(command));
    }

    [Fact]
    public async Task CreateSourceAsync_With_Duplicate_Name_Should_Throw_ConflictException()
    {
        var existing = new KnowledgeSource(
            Guid.NewGuid(),
            "Existing Source",
            SourceType.Internal,
            _dateTimeProvider.UtcNow);
        await _sourceRepository.AddAsync(existing);

        var command = new CreateKnowledgeSourceCommand(
            "Existing Source",
            SourceType.External);

        await Assert.ThrowsAsync<ConflictException>(() =>
            _service.CreateSourceAsync(command));
    }

    [Fact]
    public async Task UpdateSourceAsync_With_Valid_Parameters_Should_Update_Source()
    {
        var source = new KnowledgeSource(
            Guid.NewGuid(),
            "Original Source",
            SourceType.Internal,
            _dateTimeProvider.UtcNow);
        await _sourceRepository.AddAsync(source);

        _dateTimeProvider.UtcNow = _dateTimeProvider.UtcNow.AddHours(2);
        var command = new UpdateKnowledgeSourceCommand(
            source.Id,
            "Updated Source Name",
            "https://updated.example.com",
            "Updated description");

        var result = await _service.UpdateSourceAsync(command);

        Assert.Equal("Updated Source Name", result.Name);
        Assert.Equal("https://updated.example.com", result.Url);
        Assert.Equal("Updated description", result.Description);
        Assert.Equal(_dateTimeProvider.UtcNow, result.UpdatedAt);
    }

    [Fact]
    public async Task UpdateSourceAsync_When_Not_Found_Should_Throw_NotFoundException()
    {
        var command = new UpdateKnowledgeSourceCommand(
            Guid.NewGuid(),
            "Non-existent Source");

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.UpdateSourceAsync(command));
    }

    [Fact]
    public async Task UpdateSourceAsync_With_Conflicting_Name_Should_Throw_ConflictException()
    {
        var source1 = new KnowledgeSource(Guid.NewGuid(), "First Source", SourceType.Internal, _dateTimeProvider.UtcNow);
        var source2 = new KnowledgeSource(Guid.NewGuid(), "Second Source", SourceType.Internal, _dateTimeProvider.UtcNow);
        await _sourceRepository.AddAsync(source1);
        await _sourceRepository.AddAsync(source2);

        var command = new UpdateKnowledgeSourceCommand(source2.Id, "First Source");

        await Assert.ThrowsAsync<ConflictException>(() =>
            _service.UpdateSourceAsync(command));
    }

    [Fact]
    public async Task GetSourceByIdAsync_When_Exists_Should_Return_Dto()
    {
        var source = new KnowledgeSource(Guid.NewGuid(), "Source", SourceType.Internal, _dateTimeProvider.UtcNow);
        await _sourceRepository.AddAsync(source);

        var result = await _service.GetSourceByIdAsync(source.Id);

        Assert.NotNull(result);
        Assert.Equal(source.Id, result.Id);
    }

    [Fact]
    public async Task GetSourceByIdAsync_When_Not_Found_Should_Throw_NotFoundException()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.GetSourceByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetSourcesAsync_With_Filters_Should_Return_Matching_Sources()
    {
        var s1 = new KnowledgeSource(Guid.NewGuid(), "Internal Approved", SourceType.Internal, _dateTimeProvider.UtcNow, isApproved: true);
        var s2 = new KnowledgeSource(Guid.NewGuid(), "External Unapproved", SourceType.External, _dateTimeProvider.UtcNow, isApproved: false);
        var s3 = new KnowledgeSource(Guid.NewGuid(), "Internal Unapproved", SourceType.Internal, _dateTimeProvider.UtcNow, isApproved: false);
        await _sourceRepository.AddAsync(s1);
        await _sourceRepository.AddAsync(s2);
        await _sourceRepository.AddAsync(s3);

        var results = await _service.GetSourcesAsync(sourceType: SourceType.Internal, isApproved: true);

        Assert.Single(results);
        Assert.Equal(s1.Id, results[0].Id);
    }

    [Fact]
    public async Task ApproveSourceAsync_Should_Set_Approved_And_Upgrade_Trust()
    {
        var source = new KnowledgeSource(
            Guid.NewGuid(),
            "Untrusted Community Doc",
            SourceType.External,
            _dateTimeProvider.UtcNow,
            trustLevel: TrustLevel.Untrusted,
            isApproved: false);
        await _sourceRepository.AddAsync(source);

        var result = await _service.ApproveSourceAsync(source.Id);

        Assert.True(result.IsApproved);
        Assert.Equal(TrustLevel.Verified, result.TrustLevel);
    }

    [Fact]
    public async Task RevokeSourceApprovalAsync_Should_Revoke_Approval()
    {
        var source = new KnowledgeSource(
            Guid.NewGuid(),
            "Approved Source",
            SourceType.Internal,
            _dateTimeProvider.UtcNow,
            isApproved: true);
        await _sourceRepository.AddAsync(source);

        var result = await _service.RevokeSourceApprovalAsync(source.Id);

        Assert.False(result.IsApproved);
    }

    [Fact]
    public async Task UpdateTrustLevelAsync_Should_Update_Trust_Level()
    {
        var source = new KnowledgeSource(
            Guid.NewGuid(),
            "Source",
            SourceType.Internal,
            _dateTimeProvider.UtcNow,
            trustLevel: TrustLevel.Untrusted);
        await _sourceRepository.AddAsync(source);

        var command = new UpdateTrustLevelCommand(source.Id, TrustLevel.Official);
        var result = await _service.UpdateTrustLevelAsync(command);

        Assert.Equal(TrustLevel.Official, result.TrustLevel);
    }

    [Fact]
    public async Task AttachSourceToArticleAsync_With_Valid_Parameters_Should_Add_Provenance()
    {
        var article = CreateSampleArticle();
        var source = new KnowledgeSource(Guid.NewGuid(), "Hardware Spec", SourceType.VendorDocumentation, _dateTimeProvider.UtcNow);
        await _sourceRepository.AddAsync(source);

        var command = new AttachArticleSourceCommand(
            article.Id,
            source.Id,
            "Chapter 3, Page 45 - Protocol Frame Definition");

        var result = await _service.AttachSourceToArticleAsync(command);

        Assert.Contains(source.Id, result.SourceIds);
        var link = Assert.Single(result.SourceLinks!);
        Assert.Equal(source.Id, link.SourceId);
        Assert.Equal("Chapter 3, Page 45 - Protocol Frame Definition", link.ReferenceText);

        var persistedArticle = await _articleRepository.GetByIdAsync(article.Id);
        Assert.Contains(source.Id, persistedArticle!.SourceIds);
    }

    [Fact]
    public async Task AttachSourceToArticleAsync_When_Article_Not_Found_Should_Throw_NotFoundException()
    {
        var source = new KnowledgeSource(Guid.NewGuid(), "Source", SourceType.Internal, _dateTimeProvider.UtcNow);
        await _sourceRepository.AddAsync(source);

        var command = new AttachArticleSourceCommand(Guid.NewGuid(), source.Id);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.AttachSourceToArticleAsync(command));
    }

    [Fact]
    public async Task AttachSourceToArticleAsync_When_Source_Not_Found_Should_Throw_NotFoundException()
    {
        var article = CreateSampleArticle();

        var command = new AttachArticleSourceCommand(article.Id, Guid.NewGuid());

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.AttachSourceToArticleAsync(command));
    }

    [Fact]
    public async Task AttachSourceToArticleAsync_When_Already_Attached_Should_Throw_ConflictException()
    {
        var article = CreateSampleArticle();
        var source = new KnowledgeSource(Guid.NewGuid(), "Source", SourceType.Internal, _dateTimeProvider.UtcNow);
        await _sourceRepository.AddAsync(source);

        await _service.AttachSourceToArticleAsync(new AttachArticleSourceCommand(article.Id, source.Id));

        await Assert.ThrowsAsync<ConflictException>(() =>
            _service.AttachSourceToArticleAsync(new AttachArticleSourceCommand(article.Id, source.Id, "Duplicate attempt")));
    }

    [Fact]
    public async Task DetachSourceFromArticleAsync_Should_Remove_Link_Without_Deleting_Source()
    {
        var article = CreateSampleArticle();
        var source = new KnowledgeSource(Guid.NewGuid(), "Source", SourceType.Internal, _dateTimeProvider.UtcNow);
        await _sourceRepository.AddAsync(source);

        await _service.AttachSourceToArticleAsync(new AttachArticleSourceCommand(article.Id, source.Id));

        var result = await _service.DetachSourceFromArticleAsync(new DetachArticleSourceCommand(article.Id, source.Id));

        Assert.DoesNotContain(source.Id, result.SourceIds);

        // Verify source itself was not deleted
        var persistedSource = await _sourceRepository.GetByIdAsync(source.Id);
        Assert.NotNull(persistedSource);
    }

    [Fact]
    public async Task DetachSourceFromArticleAsync_When_Not_Attached_Should_Throw_NotFoundException()
    {
        var article = CreateSampleArticle();
        var source = new KnowledgeSource(Guid.NewGuid(), "Source", SourceType.Internal, _dateTimeProvider.UtcNow);
        await _sourceRepository.AddAsync(source);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.DetachSourceFromArticleAsync(new DetachArticleSourceCommand(article.Id, source.Id)));
    }

    [Fact]
    public async Task GetSourcesByArticleIdAsync_Should_Return_All_Attached_Sources()
    {
        var article = CreateSampleArticle();
        var s1 = new KnowledgeSource(Guid.NewGuid(), "Source 1", SourceType.Internal, _dateTimeProvider.UtcNow);
        var s2 = new KnowledgeSource(Guid.NewGuid(), "Source 2", SourceType.VendorDocumentation, _dateTimeProvider.UtcNow);
        await _sourceRepository.AddAsync(s1);
        await _sourceRepository.AddAsync(s2);

        await _service.AttachSourceToArticleAsync(new AttachArticleSourceCommand(article.Id, s1.Id));
        await _service.AttachSourceToArticleAsync(new AttachArticleSourceCommand(article.Id, s2.Id));

        var sources = await _service.GetSourcesByArticleIdAsync(article.Id);

        Assert.Equal(2, sources.Count);
        Assert.Contains(sources, s => s.Id == s1.Id);
        Assert.Contains(sources, s => s.Id == s2.Id);
    }
}
