namespace EngineeringKnowledgeHub.Api.Extensions;

using EngineeringKnowledgeHub.Api.Common;
using EngineeringKnowledgeHub.Api.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSecurityServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<GovernanceAuthenticationOptions>(
            configuration.GetSection(GovernanceAuthenticationOptions.SectionName));
        services.Configure<GovernanceAuthenticationOptions>(
            GovernanceAuthenticationDefaults.AuthenticationScheme,
            configuration.GetSection(GovernanceAuthenticationOptions.SectionName));

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = GovernanceAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = GovernanceAuthenticationDefaults.AuthenticationScheme;
        })
        .AddScheme<GovernanceAuthenticationOptions, GovernanceAuthenticationHandler>(
            GovernanceAuthenticationDefaults.AuthenticationScheme,
            options =>
            {
                configuration.GetSection(GovernanceAuthenticationOptions.SectionName).Bind(options);
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(GovernanceAuthenticationDefaults.AuditGovernancePolicy, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireRole("Admin", "Auditor");
            });
        });

        return services;
    }

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
