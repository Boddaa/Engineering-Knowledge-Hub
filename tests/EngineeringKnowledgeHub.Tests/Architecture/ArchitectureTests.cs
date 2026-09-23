namespace EngineeringKnowledgeHub.Tests.Architecture;

using System.Reflection;
using EngineeringKnowledgeHub.Application;
using EngineeringKnowledgeHub.Domain;
using EngineeringKnowledgeHub.Infrastructure;
using Xunit;

public class ArchitectureTests
{
    private static readonly Assembly DomainAssembly = typeof(DomainAssemblyMarker).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(ApplicationAssemblyMarker).Assembly;
    private static readonly Assembly InfrastructureAssembly = typeof(DependencyInjection).Assembly;
    private static readonly Assembly ApiAssembly = typeof(Program).Assembly;

    [Fact]
    public void Domain_Should_Not_Depend_On_Infrastructure()
    {
        var referencedAssemblies = DomainAssembly.GetReferencedAssemblies();
        var infrastructureName = InfrastructureAssembly.GetName().Name;

        Assert.DoesNotContain(referencedAssemblies, a => a.Name == infrastructureName);
    }

    [Fact]
    public void Domain_Should_Not_Depend_On_Api()
    {
        var referencedAssemblies = DomainAssembly.GetReferencedAssemblies();
        var apiName = ApiAssembly.GetName().Name;

        Assert.DoesNotContain(referencedAssemblies, a => a.Name == apiName);
    }

    [Fact]
    public void Domain_Should_Not_Depend_On_Application()
    {
        var referencedAssemblies = DomainAssembly.GetReferencedAssemblies();
        var applicationName = ApplicationAssembly.GetName().Name;

        Assert.DoesNotContain(referencedAssemblies, a => a.Name == applicationName);
    }

    [Fact]
    public void Domain_Should_Not_Depend_On_Framework_Or_External_Libraries()
    {
        var referencedAssemblies = DomainAssembly.GetReferencedAssemblies();

        Assert.DoesNotContain(referencedAssemblies, a => a.Name?.StartsWith("Microsoft.AspNetCore") == true);
        Assert.DoesNotContain(referencedAssemblies, a => a.Name?.StartsWith("Microsoft.EntityFrameworkCore") == true);
    }

    [Fact]
    public void Application_Should_Not_Depend_On_Api()
    {
        var referencedAssemblies = ApplicationAssembly.GetReferencedAssemblies();
        var apiName = ApiAssembly.GetName().Name;

        Assert.DoesNotContain(referencedAssemblies, a => a.Name == apiName);
    }

    [Fact]
    public void Application_Should_Not_Depend_On_Infrastructure()
    {
        var referencedAssemblies = ApplicationAssembly.GetReferencedAssemblies();
        var infrastructureName = InfrastructureAssembly.GetName().Name;

        Assert.DoesNotContain(referencedAssemblies, a => a.Name == infrastructureName);
    }

    [Fact]
    public void Domain_Entities_Should_Not_Have_Public_Setters()
    {
        var entityTypes = DomainAssembly.GetTypes()
            .Where(t => t.Namespace?.StartsWith("EngineeringKnowledgeHub.Domain.Entities") == true 
                     && t.IsClass 
                     && t.GetProperty("Id") != null);

        foreach (var type in entityTypes)
        {
            var publicSetters = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.SetMethod is not null && p.SetMethod.IsPublic);

            Assert.Empty(publicSetters);
        }
    }

    [Fact]
    public void Application_Services_Should_Implement_Interfaces()
    {
        var serviceTypes = ApplicationAssembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("Service"));

        foreach (var serviceType in serviceTypes)
        {
            var interfaces = serviceType.GetInterfaces();
            Assert.NotEmpty(interfaces);
            Assert.Contains(interfaces, i => i.Name == $"I{serviceType.Name}");
        }
    }
}

