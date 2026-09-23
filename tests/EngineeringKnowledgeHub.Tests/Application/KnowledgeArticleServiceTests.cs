namespace EngineeringKnowledgeHub.Tests.Application;

using EngineeringKnowledgeHub.Application.Articles;
using EngineeringKnowledgeHub.Application.Common.Exceptions;
using EngineeringKnowledgeHub.Domain.Entities;
using EngineeringKnowledgeHub.Domain.Enums;
using EngineeringKnowledgeHub.Domain.Exceptions;
using EngineeringKnowledgeHub.Tests.Common;
using Xunit;

public class KnowledgeArticleServiceTests
{
    private readonly InMemoryKnowledgeArticleRepository _articleRepo = new();
    private readonly InMemoryKnowledgeTypeRepository _typeRepo = new();
    private readonly InMemoryKnowledgeTagRepository _tagRepo = new();
    private readonly InMemoryProductRepository _productRepo = new();
    private readonly InMemoryTechnologyRepository _techRepo = new();
    private readonly KnowledgeArticleService _service;

    private readonly Guid _knowledgeTypeId = Guid.NewGuid();
    private readonly Guid _authorId = Guid.NewGuid();
    private readonly Guid _reviewerId = Guid.NewGuid();
    private readonly DateTimeOffset _now = DateTimeOffset.UtcNow;

    public KnowledgeArticleServiceTests()
    {
        _service = new KnowledgeArticleService(_articleRepo, _typeRepo, _tagRepo, _productRepo, _techRepo);

        // Seed default knowledge type
        _typeRepo.AddAsync(new KnowledgeType(_knowledgeTypeId, "Troubleshooting", "Troubleshooting guide")).Wait();
    }

    [Fact]
    public async Task CreateArticle_With_Valid_Command_Should_Succeed()
    {
        var command = new CreateKnowledgeArticleCommand(
            "Camera Calibration Procedure",
            "Guide to calibrate camera",
            "# Steps...",
            _knowledgeTypeId,
            _authorId,
            _now,
            Slug: "camera-calibration-procedure");

        var result = await _service.CreateArticleAsync(command);

        Assert.NotNull(result);
        Assert.Equal("Camera Calibration Procedure", result.Title);
        Assert.Equal("camera-calibration-procedure", result.Slug);
        Assert.Equal(KnowledgeStatus.Draft, result.Status);
        Assert.Equal(VerificationStatus.Unverified, result.VerificationStatus);
        Assert.Equal(_authorId, result.AuthorId);

        var persisted = await _articleRepo.GetByIdAsync(result.Id);
        Assert.NotNull(persisted);
    }

    [Fact]
    public async Task CreateArticle_Without_Slug_Should_Generate_Slug_From_Title()
    {
        var command = new CreateKnowledgeArticleCommand(
            "Toll System Setup Guide 2026",
            "Summary",
            "Content",
            _knowledgeTypeId,
            _authorId,
            _now);

        var result = await _service.CreateArticleAsync(command);

        Assert.Equal("toll-system-setup-guide-2026", result.Slug);
    }

    [Fact]
    public async Task CreateArticle_With_Arabic_Title_Should_Generate_Valid_Arabic_Slug()
    {
        var command = new CreateKnowledgeArticleCommand(
            "دليل حل مشاكل البوابة",
            "الملخص",
            "المحتوى",
            _knowledgeTypeId,
            _authorId,
            _now);

        var result = await _service.CreateArticleAsync(command);

        Assert.Equal("دليل-حل-مشاكل-البوابة", result.Slug);
    }

    [Fact]
    public async Task CreateArticle_With_NonExistent_KnowledgeType_Should_Throw_NotFoundException()
    {
        var nonExistentTypeId = Guid.NewGuid();
        var command = new CreateKnowledgeArticleCommand(
            "Title", "Summary", "Content", nonExistentTypeId, _authorId, _now);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.CreateArticleAsync(command));
    }

    [Fact]
    public async Task CreateArticle_With_Duplicate_Slug_Should_Throw_ConflictException()
    {
        var command1 = new CreateKnowledgeArticleCommand(
            "Unique Title", "Summary", "Content", _knowledgeTypeId, _authorId, _now, Slug: "common-slug");
        await _service.CreateArticleAsync(command1);

        var command2 = new CreateKnowledgeArticleCommand(
            "Another Title", "Summary", "Content", _knowledgeTypeId, _authorId, _now, Slug: "common-slug");

        await Assert.ThrowsAsync<ConflictException>(() => _service.CreateArticleAsync(command2));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task CreateArticle_With_Invalid_Title_Should_Throw_ValidationException(string? title)
    {
        var command = new CreateKnowledgeArticleCommand(
            title!, "Summary", "Content", _knowledgeTypeId, _authorId, _now);

        await Assert.ThrowsAsync<ValidationException>(() => _service.CreateArticleAsync(command));
    }

    [Fact]
    public async Task UpdateArticle_With_Valid_Command_Should_Update_And_Persist()
    {
        var createCommand = new CreateKnowledgeArticleCommand(
            "Original Title", "Original Summary", "Original Content", _knowledgeTypeId, _authorId, _now);
        var created = await _service.CreateArticleAsync(createCommand);

        var updateCommand = new UpdateKnowledgeArticleCommand(
            created.Id,
            "Updated Title",
            "Updated Summary",
            "Updated Content",
            _now.AddHours(2),
            Slug: "updated-title");

        var updated = await _service.UpdateArticleAsync(updateCommand);

        Assert.Equal("Updated Title", updated.Title);
        Assert.Equal("updated-title", updated.Slug);
        Assert.Equal("Updated Summary", updated.Summary);
        Assert.Equal(_now.AddHours(2), updated.UpdatedAt);
    }

    [Fact]
    public async Task UpdateArticle_With_NonExistent_Id_Should_Throw_NotFoundException()
    {
        var updateCommand = new UpdateKnowledgeArticleCommand(
            Guid.NewGuid(), "Title", "Summary", "Content", _now);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.UpdateArticleAsync(updateCommand));
    }

    [Fact]
    public async Task GetArticleById_With_Existing_Id_Should_Return_Dto()
    {
        var createCommand = new CreateKnowledgeArticleCommand(
            "Title", "Summary", "Content", _knowledgeTypeId, _authorId, _now);
        var created = await _service.CreateArticleAsync(createCommand);

        var fetched = await _service.GetArticleByIdAsync(created.Id);

        Assert.Equal(created.Id, fetched.Id);
        Assert.Equal("Title", fetched.Title);
    }

    [Fact]
    public async Task GetArticleById_With_NonExistent_Id_Should_Throw_NotFoundException()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetArticleByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task SubmitForReview_Should_Transition_Draft_To_PendingReview()
    {
        var created = await _service.CreateArticleAsync(new CreateKnowledgeArticleCommand(
            "Title", "Summary", "Content", _knowledgeTypeId, _authorId, _now));

        var submitted = await _service.SubmitForReviewAsync(created.Id);

        Assert.Equal(KnowledgeStatus.PendingReview, submitted.Status);
    }

    [Fact]
    public async Task ApproveArticle_By_PeerReviewer_Should_Publish_And_Verify()
    {
        var created = await _service.CreateArticleAsync(new CreateKnowledgeArticleCommand(
            "Title", "Summary", "Content", _knowledgeTypeId, _authorId, _now));
        await _service.SubmitForReviewAsync(created.Id);

        var approveCommand = new ApproveKnowledgeArticleCommand(created.Id, _reviewerId, _now.AddHours(1));
        var approved = await _service.ApproveArticleAsync(approveCommand);

        Assert.Equal(KnowledgeStatus.Published, approved.Status);
        Assert.Equal(VerificationStatus.Verified, approved.VerificationStatus);
        Assert.Equal(_reviewerId, approved.ReviewerId);
    }

    [Fact]
    public async Task ApproveArticle_By_Author_Must_Throw_DomainRuleViolationException()
    {
        var created = await _service.CreateArticleAsync(new CreateKnowledgeArticleCommand(
            "Title", "Summary", "Content", _knowledgeTypeId, _authorId, _now));
        await _service.SubmitForReviewAsync(created.Id);

        var approveCommand = new ApproveKnowledgeArticleCommand(created.Id, _authorId, _now.AddHours(1));

        await Assert.ThrowsAsync<DomainRuleViolationException>(() => _service.ApproveArticleAsync(approveCommand));
    }

    [Fact]
    public async Task ApproveArticle_When_Still_In_Draft_Should_Throw_DomainRuleViolationException()
    {
        var created = await _service.CreateArticleAsync(new CreateKnowledgeArticleCommand(
            "Title", "Summary", "Content", _knowledgeTypeId, _authorId, _now));

        var approveCommand = new ApproveKnowledgeArticleCommand(created.Id, _reviewerId, _now.AddHours(1));

        await Assert.ThrowsAsync<DomainRuleViolationException>(() => _service.ApproveArticleAsync(approveCommand));
    }

    [Fact]
    public async Task RejectArticle_By_PeerReviewer_Should_Return_To_Draft_And_Mark_Rejected()
    {
        var created = await _service.CreateArticleAsync(new CreateKnowledgeArticleCommand(
            "Title", "Summary", "Content", _knowledgeTypeId, _authorId, _now));
        await _service.SubmitForReviewAsync(created.Id);

        var rejectCommand = new RejectKnowledgeArticleCommand(created.Id, _reviewerId, _now.AddHours(1));
        var rejected = await _service.RejectArticleAsync(rejectCommand);

        Assert.Equal(KnowledgeStatus.Draft, rejected.Status);
        Assert.Equal(VerificationStatus.Rejected, rejected.VerificationStatus);
    }

    [Fact]
    public async Task DeprecateArticle_When_Published_Should_Succeed()
    {
        var created = await _service.CreateArticleAsync(new CreateKnowledgeArticleCommand(
            "Title", "Summary", "Content", _knowledgeTypeId, _authorId, _now));
        await _service.SubmitForReviewAsync(created.Id);
        await _service.ApproveArticleAsync(new ApproveKnowledgeArticleCommand(created.Id, _reviewerId, _now.AddHours(1)));

        var deprecated = await _service.DeprecateArticleAsync(created.Id);

        Assert.Equal(KnowledgeStatus.Deprecated, deprecated.Status);
    }

    [Fact]
    public async Task ArchiveArticle_Should_Transition_To_Archived()
    {
        var created = await _service.CreateArticleAsync(new CreateKnowledgeArticleCommand(
            "Title", "Summary", "Content", _knowledgeTypeId, _authorId, _now));

        var archived = await _service.ArchiveArticleAsync(created.Id);

        Assert.Equal(KnowledgeStatus.Archived, archived.Status);
    }

    [Fact]
    public async Task AddTag_And_RemoveTag_Should_Manage_Article_Tags()
    {
        var created = await _service.CreateArticleAsync(new CreateKnowledgeArticleCommand(
            "Title", "Summary", "Content", _knowledgeTypeId, _authorId, _now));

        var tagId = Guid.NewGuid();
        await _tagRepo.AddAsync(new KnowledgeTag(tagId, "networking"));

        var withTag = await _service.AddTagAsync(created.Id, tagId);
        Assert.Contains(tagId, withTag.TagIds);

        var withoutTag = await _service.RemoveTagAsync(created.Id, tagId);
        Assert.DoesNotContain(tagId, withoutTag.TagIds);
    }

    [Fact]
    public async Task AddTag_With_NonExistent_Tag_Should_Throw_NotFoundException()
    {
        var created = await _service.CreateArticleAsync(new CreateKnowledgeArticleCommand(
            "Title", "Summary", "Content", _knowledgeTypeId, _authorId, _now));

        await Assert.ThrowsAsync<NotFoundException>(() => _service.AddTagAsync(created.Id, Guid.NewGuid()));
    }

    [Fact]
    public async Task AddProduct_And_RemoveProduct_Should_Manage_Article_Products()
    {
        var created = await _service.CreateArticleAsync(new CreateKnowledgeArticleCommand(
            "Title", "Summary", "Content", _knowledgeTypeId, _authorId, _now));

        var productId = Guid.NewGuid();
        await _productRepo.AddAsync(new Product(productId, "Toll System", "TOLL-01"));

        var withProduct = await _service.AddProductAsync(created.Id, productId);
        Assert.Contains(productId, withProduct.ProductIds);

        var withoutProduct = await _service.RemoveProductAsync(created.Id, productId);
        Assert.DoesNotContain(productId, withoutProduct.ProductIds);
    }

    [Fact]
    public async Task AddTechnology_And_RemoveTechnology_Should_Manage_Article_Technologies()
    {
        var created = await _service.CreateArticleAsync(new CreateKnowledgeArticleCommand(
            "Title", "Summary", "Content", _knowledgeTypeId, _authorId, _now));

        var techId = Guid.NewGuid();
        await _techRepo.AddAsync(new Technology(techId, "ASP.NET Core"));

        var withTech = await _service.AddTechnologyAsync(created.Id, techId);
        Assert.Contains(techId, withTech.TechnologyIds);

        var withoutTech = await _service.RemoveTechnologyAsync(created.Id, techId);
        Assert.DoesNotContain(techId, withoutTech.TechnologyIds);
    }
}
