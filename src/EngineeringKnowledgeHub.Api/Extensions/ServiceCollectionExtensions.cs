namespace EngineeringKnowledgeHub.Api.Extensions;

using EngineeringKnowledgeHub.Api.Common;
using Microsoft.OpenApi;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Engineering Knowledge Hub API",
                Version = "v1",
                Description = "Foundational API for the Engineering Knowledge Hub."
            });
        });

        return services;
    }

    public static IServiceCollection AddErrorHandling(this IServiceCollection services)
    {
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        return services;
    }

    public static IServiceCollection AddApiHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks();

        return services;
    }
}
