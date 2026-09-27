namespace EngineeringKnowledgeHub.Infrastructure;

using EngineeringKnowledgeHub.Application;
using EngineeringKnowledgeHub.Application.Ai;
using EngineeringKnowledgeHub.Application.Enrichment;
using EngineeringKnowledgeHub.Application.Rag;
using EngineeringKnowledgeHub.Application.SemanticSearch;
using EngineeringKnowledgeHub.Infrastructure.Enrichment;
using EngineeringKnowledgeHub.Infrastructure.LocalAi;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // 1. External Knowledge Enrichment HTTP client (Phase 9)
        services.AddHttpClient<IExternalKnowledgeProvider, HttpExternalKnowledgeProvider>()
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                AllowAutoRedirect = true,
                MaxAutomaticRedirections = 5,
                AutomaticDecompression = System.Net.DecompressionMethods.All
            });

        // 2. Local AI Configuration (Phase 10)
        var localAiOptions = new LocalAiOptions();
        var section = configuration.GetSection(LocalAiOptions.SectionName);
        if (section.Exists())
        {
            if (bool.TryParse(section["Enabled"], out var enabled)) localAiOptions.Enabled = enabled;
            if (!string.IsNullOrWhiteSpace(section["BaseUrl"])) localAiOptions.BaseUrl = section["BaseUrl"]!;
            if (!string.IsNullOrWhiteSpace(section["GenerationModel"])) localAiOptions.GenerationModel = section["GenerationModel"]!;
            if (!string.IsNullOrWhiteSpace(section["EmbeddingModel"])) localAiOptions.EmbeddingModel = section["EmbeddingModel"]!;
            if (int.TryParse(section["TimeoutSeconds"], out var timeout)) localAiOptions.TimeoutSeconds = timeout;
            if (int.TryParse(section["EmbeddingDimensions"], out var dims)) localAiOptions.EmbeddingDimensions = dims;
        }

        // Support explicit provider override via "AI:Provider" ("Local" vs "Deterministic")
        var aiProviderOverride = configuration["AI:Provider"];
        if (string.Equals(aiProviderOverride, "Local", StringComparison.OrdinalIgnoreCase))
        {
            localAiOptions.Enabled = true;
        }
        else if (string.Equals(aiProviderOverride, "Deterministic", StringComparison.OrdinalIgnoreCase))
        {
            localAiOptions.Enabled = false;
        }

        services.AddSingleton(localAiOptions);

        // Register DI-managed HttpClients for Local AI runtime
        var baseUri = Uri.TryCreate(localAiOptions.BaseUrl, UriKind.Absolute, out var uri)
            ? uri
            : new Uri(LocalAiOptions.DefaultBaseUrl);

        var requestTimeout = localAiOptions.TimeoutSeconds > 0
            ? TimeSpan.FromSeconds(localAiOptions.TimeoutSeconds)
            : TimeSpan.FromSeconds(LocalAiOptions.DefaultTimeoutSeconds);

        services.AddHttpClient<OllamaAiProvider>(client =>
        {
            client.BaseAddress = baseUri;
            client.Timeout = requestTimeout;
        });

        services.AddHttpClient<OllamaEmbeddingProvider>(client =>
        {
            client.BaseAddress = baseUri;
            client.Timeout = requestTimeout;
        });

        services.AddHttpClient<ILocalAiHealthService, OllamaLocalAiHealthService>(client =>
        {
            client.BaseAddress = baseUri;
            client.Timeout = requestTimeout;
        });

        // 3. Provider selection: Local vs Deterministic
        if (localAiOptions.Enabled)
        {
            localAiOptions.Validate();

            // Override any default deterministic providers with real local providers
            services.RemoveAll<IAiProvider>();
            services.RemoveAll<IEmbeddingProvider>();

            services.AddSingleton<IAiProvider, OllamaAiProvider>();
            services.AddSingleton<IEmbeddingProvider, OllamaEmbeddingProvider>();
        }
        else
        {
            // Register deterministic providers as safe offline fallback
            services.TryAddSingleton<IAiProvider, DeterministicAiProvider>();
            services.TryAddSingleton<IEmbeddingProvider, DeterministicEmbeddingProvider>();
        }

        return services;
    }
}
