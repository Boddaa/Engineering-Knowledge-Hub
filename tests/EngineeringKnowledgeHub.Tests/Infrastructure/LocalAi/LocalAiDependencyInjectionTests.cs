namespace EngineeringKnowledgeHub.Tests.Infrastructure.LocalAi;

using EngineeringKnowledgeHub.Application.Ai;
using EngineeringKnowledgeHub.Application.Rag;
using EngineeringKnowledgeHub.Application.SemanticSearch;
using EngineeringKnowledgeHub.Infrastructure;
using EngineeringKnowledgeHub.Infrastructure.LocalAi;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public class LocalAiDependencyInjectionTests
{
    [Fact]
    public void AddInfrastructure_WhenLocalAiDisabled_ShouldRegisterDeterministicProvidersAsSingleton()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["LocalAI:Enabled"] = "false"
            })
            .Build();

        services.AddInfrastructure(configuration);

        // Verify descriptor lifetime: deterministic providers are Singleton
        var aiDescriptor = services.Last(d => d.ServiceType == typeof(IAiProvider));
        var embedDescriptor = services.Last(d => d.ServiceType == typeof(IEmbeddingProvider));
        Assert.Equal(ServiceLifetime.Singleton, aiDescriptor.Lifetime);
        Assert.Equal(ServiceLifetime.Singleton, embedDescriptor.Lifetime);

        using var provider = services.BuildServiceProvider();

        var aiProvider = provider.GetRequiredService<IAiProvider>();
        var embeddingProvider = provider.GetRequiredService<IEmbeddingProvider>();
        var healthService = provider.GetRequiredService<ILocalAiHealthService>();

        Assert.IsType<DeterministicAiProvider>(aiProvider);
        Assert.IsType<DeterministicEmbeddingProvider>(embeddingProvider);
        Assert.IsType<OllamaLocalAiHealthService>(healthService);
    }

    [Fact]
    public void AddInfrastructure_WhenLocalAiEnabled_ShouldRegisterOllamaProvidersWithTransientLifetime()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["LocalAI:Enabled"] = "true",
                ["LocalAI:BaseUrl"] = "http://127.0.0.1:11434",
                ["LocalAI:GenerationModel"] = "llama3:8b",
                ["LocalAI:EmbeddingModel"] = "nomic-embed-text",
                ["LocalAI:TimeoutSeconds"] = "60",
                ["LocalAI:EmbeddingDimensions"] = "768"
            })
            .Build();

        services.AddInfrastructure(configuration);

        // Verify descriptor lifetime: typed HttpClients are Transient (avoiding singleton lifetime mismatch)
        var aiDescriptor = services.Last(d => d.ServiceType == typeof(IAiProvider));
        var embedDescriptor = services.Last(d => d.ServiceType == typeof(IEmbeddingProvider));
        Assert.Equal(ServiceLifetime.Transient, aiDescriptor.Lifetime);
        Assert.Equal(ServiceLifetime.Transient, embedDescriptor.Lifetime);

        // Verify no singleton descriptors exist for IAiProvider or IEmbeddingProvider
        Assert.DoesNotContain(services, d => d.ServiceType == typeof(IAiProvider) && d.Lifetime == ServiceLifetime.Singleton);
        Assert.DoesNotContain(services, d => d.ServiceType == typeof(IEmbeddingProvider) && d.Lifetime == ServiceLifetime.Singleton);

        using var provider = services.BuildServiceProvider();

        var aiProvider = provider.GetRequiredService<IAiProvider>();
        var embeddingProvider = provider.GetRequiredService<IEmbeddingProvider>();
        var healthService = provider.GetRequiredService<ILocalAiHealthService>();

        Assert.IsType<OllamaAiProvider>(aiProvider);
        Assert.IsType<OllamaEmbeddingProvider>(embeddingProvider);
        Assert.IsType<OllamaLocalAiHealthService>(healthService);
    }

    [Fact]
    public void AddInfrastructure_WhenAiProviderExplicitlySetToLocal_ShouldActivateLocalProviders()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AI:Provider"] = "Local",
                ["LocalAI:BaseUrl"] = "http://127.0.0.1:11434",
                ["LocalAI:GenerationModel"] = "llama3",
                ["LocalAI:EmbeddingModel"] = "nomic-embed-text"
            })
            .Build();

        services.AddInfrastructure(configuration);

        var aiDescriptor = services.Last(d => d.ServiceType == typeof(IAiProvider));
        Assert.Equal(ServiceLifetime.Transient, aiDescriptor.Lifetime);

        using var provider = services.BuildServiceProvider();

        var aiProvider = provider.GetRequiredService<IAiProvider>();
        var embeddingProvider = provider.GetRequiredService<IEmbeddingProvider>();

        Assert.IsType<OllamaAiProvider>(aiProvider);
        Assert.IsType<OllamaEmbeddingProvider>(embeddingProvider);
    }

    [Fact]
    public void AddInfrastructure_WhenAiProviderExplicitlySetToDeterministic_ShouldOverrideLocalEnabled()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["LocalAI:Enabled"] = "true",
                ["AI:Provider"] = "Deterministic"
            })
            .Build();

        services.AddInfrastructure(configuration);

        var aiDescriptor = services.Last(d => d.ServiceType == typeof(IAiProvider));
        Assert.Equal(ServiceLifetime.Singleton, aiDescriptor.Lifetime);

        using var provider = services.BuildServiceProvider();

        var aiProvider = provider.GetRequiredService<IAiProvider>();
        var embeddingProvider = provider.GetRequiredService<IEmbeddingProvider>();

        Assert.IsType<DeterministicAiProvider>(aiProvider);
        Assert.IsType<DeterministicEmbeddingProvider>(embeddingProvider);
    }
}
