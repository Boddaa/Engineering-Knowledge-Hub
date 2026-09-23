namespace EngineeringKnowledgeHub.Application;

using EngineeringKnowledgeHub.Application.Articles;
using EngineeringKnowledgeHub.Application.Common.Interfaces;
using EngineeringKnowledgeHub.Application.Common.Services;
using EngineeringKnowledgeHub.Application.ProductsAndTechnologies;
using EngineeringKnowledgeHub.Application.Taxonomy;
using Microsoft.Extensions.DependencyInjection;

public static class ApplicationDependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();

        services.AddScoped<IKnowledgeArticleService, KnowledgeArticleService>();
        services.AddScoped<IKnowledgeTaxonomyService, KnowledgeTaxonomyService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ITechnologyService, TechnologyService>();

        return services;
    }
}
