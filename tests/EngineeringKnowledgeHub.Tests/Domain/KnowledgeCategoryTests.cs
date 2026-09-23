namespace EngineeringKnowledgeHub.Tests.Domain;

using EngineeringKnowledgeHub.Domain.Entities;
using EngineeringKnowledgeHub.Domain.Exceptions;
using Xunit;

public class KnowledgeCategoryTests
{
    [Fact]
    public void Constructor_With_Valid_Parameters_Should_Succeed()
    {
        var id = Guid.NewGuid();
        var category = new KnowledgeCategory(id, "Hardware Troubleshooting", "Camera, barrier, loop detector guides");

        Assert.Equal(id, category.Id);
        Assert.Equal("Hardware Troubleshooting", category.Name);
        Assert.Null(category.ParentCategoryId);
    }

    [Fact]
    public void Constructor_When_Self_Referencing_As_Parent_Should_Throw_DomainRuleViolationException()
    {
        var id = Guid.NewGuid();

        Assert.Throws<DomainRuleViolationException>(() =>
            new KnowledgeCategory(id, "Networking", "Network guides", id));
    }

    [Fact]
    public void Update_When_Setting_Self_As_Parent_Should_Throw_DomainRuleViolationException()
    {
        var id = Guid.NewGuid();
        var category = new KnowledgeCategory(id, "Software Architecture");

        Assert.Throws<DomainRuleViolationException>(() =>
            category.Update("Software Architecture", "Description", id));
    }
}
