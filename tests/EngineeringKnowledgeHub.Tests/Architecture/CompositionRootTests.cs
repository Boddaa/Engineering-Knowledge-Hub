namespace EngineeringKnowledgeHub.Tests.Architecture;

using EngineeringKnowledgeHub.Application;
using EngineeringKnowledgeHub.Application.Articles;
using EngineeringKnowledgeHub.Application.Common.Interfaces;
using EngineeringKnowledgeHub.Application.Documents;
using EngineeringKnowledgeHub.Application.Enrichment;
using EngineeringKnowledgeHub.Application.Feedback;
using EngineeringKnowledgeHub.Application.Governance;
using EngineeringKnowledgeHub.Application.Incidents;
using EngineeringKnowledgeHub.Application.ProductsAndTechnologies;
using EngineeringKnowledgeHub.Application.Rag;
using EngineeringKnowledgeHub.Application.Search;
using EngineeringKnowledgeHub.Application.SemanticSearch;
using EngineeringKnowledgeHub.Application.Sources;
using EngineeringKnowledgeHub.Application.Taxonomy;
using EngineeringKnowledgeHub.Infrastructure;
using EngineeringKnowledgeHub.Infrastructure.Persistence;
using EngineeringKnowledgeHub.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public class CompositionRootTests
{
    private readonly IServiceProvider _serviceProvider;

    public CompositionRootTests()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Server=(localdb)\\mssqllocaldb;Database=EngineeringKnowledgeHubTestDb;Trusted_Connection=True;",
                ["LocalAI:Enabled"] = "false"
            })
            .Build();

        services.AddApplication();
        services.AddInfrastructure(configuration);

        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public void Production_DI_Should_Resolve_All_Repository_Interfaces_To_Infrastructure_Implementations()
    {
        using var scope = _serviceProvider.CreateScope();
        var sp = scope.ServiceProvider;

        Assert.IsType<KnowledgeArticleRepository>(sp.GetRequiredService<IKnowledgeArticleRepository>());
        Assert.IsType<KnowledgeVersionRepository>(sp.GetRequiredService<IKnowledgeVersionRepository>());
        Assert.IsType<AuditLogRepository>(sp.GetRequiredService<IAuditLogRepository>());
        Assert.IsType<DocumentRepository>(sp.GetRequiredService<IDocumentRepository>());
        Assert.IsType<DocumentChunkRepository>(sp.GetRequiredService<IDocumentChunkRepository>());
        Assert.IsType<IncidentRepository>(sp.GetRequiredService<IIncidentRepository>());
        Assert.IsType<KnowledgeFeedbackRepository>(sp.GetRequiredService<IKnowledgeFeedbackRepository>());
        Assert.IsType<KnowledgeSourceRepository>(sp.GetRequiredService<IKnowledgeSourceRepository>());
        Assert.IsType<KnowledgeCategoryRepository>(sp.GetRequiredService<IKnowledgeCategoryRepository>());
        Assert.IsType<KnowledgeTagRepository>(sp.GetRequiredService<IKnowledgeTagRepository>());
        Assert.IsType<KnowledgeTypeRepository>(sp.GetRequiredService<IKnowledgeTypeRepository>());
        Assert.IsType<ProductRepository>(sp.GetRequiredService<IProductRepository>());
        Assert.IsType<TechnologyRepository>(sp.GetRequiredService<ITechnologyRepository>());
    }

    [Fact]
    public void Production_DI_Should_Resolve_UnitOfWork()
    {
        using var scope = _serviceProvider.CreateScope();
        var sp = scope.ServiceProvider;

        var uow = sp.GetRequiredService<IUnitOfWork>();
        Assert.NotNull(uow);
        Assert.IsType<EfUnitOfWork>(uow);
    }

    [Fact]
    public void Production_DI_Should_Resolve_Key_Application_Services()
    {
        using var scope = _serviceProvider.CreateScope();
        var sp = scope.ServiceProvider;

        Assert.NotNull(sp.GetRequiredService<IKnowledgeArticleService>());
        Assert.NotNull(sp.GetRequiredService<IKnowledgeGovernanceService>());
        Assert.NotNull(sp.GetRequiredService<IKnowledgeTaxonomyService>());
        Assert.NotNull(sp.GetRequiredService<IProductService>());
        Assert.NotNull(sp.GetRequiredService<ITechnologyService>());
        Assert.NotNull(sp.GetRequiredService<ISemanticIndexService>());
        Assert.NotNull(sp.GetRequiredService<ISemanticSearchService>());
        Assert.NotNull(sp.GetRequiredService<ISearchService>());
        Assert.NotNull(sp.GetRequiredService<IKnowledgeRetriever>());
        Assert.NotNull(sp.GetRequiredService<IRagService>());
        Assert.NotNull(sp.GetRequiredService<IIncidentService>());
        Assert.NotNull(sp.GetRequiredService<IKnowledgeFeedbackService>());
        Assert.NotNull(sp.GetRequiredService<IKnowledgeSourceService>());
        Assert.NotNull(sp.GetRequiredService<IDocumentService>());
        Assert.NotNull(sp.GetRequiredService<IExternalEnrichmentService>());
    }

    [Fact]
    public void DbContext_Should_Throw_When_ConnectionString_Is_Missing()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build(); // No connection strings

        services.AddInfrastructure(configuration);
        using var sp = services.BuildServiceProvider();

        var ex = Assert.Throws<InvalidOperationException>(() => sp.GetRequiredService<AppDbContext>());
        Assert.Contains("DefaultConnection", ex.Message);
    }
}
