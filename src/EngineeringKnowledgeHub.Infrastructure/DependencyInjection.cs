namespace EngineeringKnowledgeHub.Infrastructure;

using EngineeringKnowledgeHub.Application;
using EngineeringKnowledgeHub.Application.Enrichment;
using EngineeringKnowledgeHub.Infrastructure.Enrichment;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<IExternalKnowledgeProvider, HttpExternalKnowledgeProvider>();

        return services;
    }
}
