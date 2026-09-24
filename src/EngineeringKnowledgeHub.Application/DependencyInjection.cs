namespace EngineeringKnowledgeHub.Application;

using EngineeringKnowledgeHub.Application.Articles;
using EngineeringKnowledgeHub.Application.Common.Interfaces;
using EngineeringKnowledgeHub.Application.Common.Services;
using EngineeringKnowledgeHub.Application.Documents;
using EngineeringKnowledgeHub.Application.Feedback;
using EngineeringKnowledgeHub.Application.Governance;
using EngineeringKnowledgeHub.Application.Incidents;
using EngineeringKnowledgeHub.Application.ProductsAndTechnologies;
using EngineeringKnowledgeHub.Application.Search;
using EngineeringKnowledgeHub.Application.Sources;
using EngineeringKnowledgeHub.Application.Taxonomy;
using Microsoft.Extensions.DependencyInjection;

public static class ApplicationDependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddSingleton<IFileHasher, Sha256FileHasher>();
        services.AddSingleton<IDocumentTextExtractor, PlainTextDocumentExtractor>();

        services.AddScoped<IKnowledgeArticleService, KnowledgeArticleService>();
        services.AddScoped<IKnowledgeGovernanceService, KnowledgeGovernanceService>();
        services.AddScoped<IKnowledgeTaxonomyService, KnowledgeTaxonomyService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ITechnologyService, TechnologyService>();
        services.AddScoped<ISearchService, SearchService>();
        services.AddScoped<IIncidentService, IncidentService>();
        services.AddScoped<IKnowledgeFeedbackService, KnowledgeFeedbackService>();
        services.AddScoped<IKnowledgeSourceService, KnowledgeSourceService>();
        services.AddScoped<IDocumentService, DocumentService>();

        return services;
    }
}
