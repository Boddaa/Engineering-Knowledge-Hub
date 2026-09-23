namespace EngineeringKnowledgeHub.Tests.Infrastructure;

using EngineeringKnowledgeHub.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public class InfrastructureTests
{
    [Fact]
    public void AddInfrastructure_Should_Register_Successfully_Without_Exceptions()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        // Act
        var result = services.AddInfrastructure(configuration);

        // Assert
        Assert.Same(services, result);

        // Verify that building the service provider succeeds
        using var provider = services.BuildServiceProvider();
        Assert.NotNull(provider);
    }
}
