namespace EngineeringKnowledgeHub.Tests.Application;

using EngineeringKnowledgeHub.Application.Common.Exceptions;
using EngineeringKnowledgeHub.Application.Taxonomy;
using EngineeringKnowledgeHub.Tests.Common;
using Xunit;

public class KnowledgeTaxonomyServiceTests
{
    private readonly InMemoryKnowledgeCategoryRepository _categoryRepo = new();
    private readonly InMemoryKnowledgeTypeRepository _typeRepo = new();
    private readonly InMemoryKnowledgeTagRepository _tagRepo = new();
    private readonly InMemoryProductRepository _productRepo = new();
    private readonly InMemoryTechnologyRepository _techRepo = new();
    private readonly KnowledgeTaxonomyService _service;

    public KnowledgeTaxonomyServiceTests()
    {
        _service = new KnowledgeTaxonomyService(_categoryRepo, _typeRepo, _tagRepo, _productRepo, _techRepo);
    }

    [Fact]
    public async Task CreateCategory_With_Valid_Command_Should_Succeed()
    {
        var command = new CreateKnowledgeCategoryCommand("Hardware", "Hardware components");
        var result = await _service.CreateCategoryAsync(command);

        Assert.NotNull(result);
        Assert.Equal("Hardware", result.Name);
        Assert.Equal("Hardware components", result.Description);
        Assert.Null(result.ParentCategoryId);
    }

    [Fact]
    public async Task CreateCategory_With_ParentCategory_Should_Succeed()
    {
        var parent = await _service.CreateCategoryAsync(new CreateKnowledgeCategoryCommand("Infrastructure"));
        var childCommand = new CreateKnowledgeCategoryCommand("Networking", "Network devices", ParentCategoryId: parent.Id);

        var child = await _service.CreateCategoryAsync(childCommand);

        Assert.Equal(parent.Id, child.ParentCategoryId);
    }

    [Fact]
    public async Task CreateCategory_With_NonExistent_Parent_Should_Throw_NotFoundException()
    {
        var command = new CreateKnowledgeCategoryCommand("Child", null, ParentCategoryId: Guid.NewGuid());

        await Assert.ThrowsAsync<NotFoundException>(() => _service.CreateCategoryAsync(command));
    }

    [Fact]
    public async Task UpdateCategory_With_Valid_Data_Should_Update()
    {
        var created = await _service.CreateCategoryAsync(new CreateKnowledgeCategoryCommand("Old Name"));
        var updated = await _service.UpdateCategoryAsync(new UpdateKnowledgeCategoryCommand(created.Id, "New Name", "New Desc"));

        Assert.Equal("New Name", updated.Name);
        Assert.Equal("New Desc", updated.Description);
    }

    [Fact]
    public async Task CreateType_And_GetTypeById_Should_Succeed()
    {
        var created = await _service.CreateTypeAsync(new CreateKnowledgeTypeCommand("Troubleshooting", "Guides"));
        var fetched = await _service.GetTypeByIdAsync(created.Id);

        Assert.Equal("Troubleshooting", fetched.Name);
        Assert.Equal("Guides", fetched.Description);
    }

    [Fact]
    public async Task UpdateType_With_Valid_Data_Should_Update()
    {
        var created = await _service.CreateTypeAsync(new CreateKnowledgeTypeCommand("FAQ", "Frequently asked"));
        var updated = await _service.UpdateTypeAsync(new UpdateKnowledgeTypeCommand(created.Id, "FAQ Updated", "New"));

        Assert.Equal("FAQ Updated", updated.Name);
    }

    [Fact]
    public async Task CreateTag_With_Valid_Name_Should_Succeed()
    {
        var created = await _service.CreateTagAsync(new CreateKnowledgeTagCommand("csharp"));

        Assert.Equal("csharp", created.Name);
    }

    [Fact]
    public async Task CreateTag_With_Duplicate_Name_Should_Throw_ConflictException()
    {
        await _service.CreateTagAsync(new CreateKnowledgeTagCommand("docker"));

        await Assert.ThrowsAsync<ConflictException>(() =>
            _service.CreateTagAsync(new CreateKnowledgeTagCommand("docker")));
    }

    [Fact]
    public async Task UpdateTag_With_Duplicate_Name_Should_Throw_ConflictException()
    {
        await _service.CreateTagAsync(new CreateKnowledgeTagCommand("tag1"));
        var tag2 = await _service.CreateTagAsync(new CreateKnowledgeTagCommand("tag2"));

        await Assert.ThrowsAsync<ConflictException>(() =>
            _service.UpdateTagAsync(new UpdateKnowledgeTagCommand(tag2.Id, "tag1")));
    }

    [Fact]
    public async Task GetTagById_With_NonExistent_Id_Should_Throw_NotFoundException()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetTagByIdAsync(Guid.NewGuid()));
    }
}
