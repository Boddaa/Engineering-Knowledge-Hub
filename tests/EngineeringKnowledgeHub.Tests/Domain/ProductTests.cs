namespace EngineeringKnowledgeHub.Tests.Domain;

using EngineeringKnowledgeHub.Domain.Entities;
using EngineeringKnowledgeHub.Domain.Exceptions;
using Xunit;

public class ProductTests
{
    [Fact]
    public void Constructor_Should_Normalize_Code_To_Uppercase()
    {
        var product = new Product(Guid.NewGuid(), "Toll Collection System", "toll-sys-ui", "Lane barrier & billing system");

        Assert.Equal("TOLL-SYS-UI", product.Code);
        Assert.True(product.IsActive);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Constructor_With_Empty_Code_Should_Throw_DomainRuleViolationException(string? code)
    {
        Assert.Throws<DomainRuleViolationException>(() =>
            new Product(Guid.NewGuid(), "Valid Name", code!));
    }

    [Fact]
    public void Deactivate_And_Activate_Should_Change_State()
    {
        var product = new Product(Guid.NewGuid(), "ANPR Camera", "ANPR-CAM");

        product.Deactivate();
        Assert.False(product.IsActive);

        product.Activate();
        Assert.True(product.IsActive);
    }
}
