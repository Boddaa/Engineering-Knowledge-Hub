using EngineeringKnowledgeHub.Api.Extensions;
using EngineeringKnowledgeHub.Application;
using EngineeringKnowledgeHub.Infrastructure;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting Engineering Knowledge Hub API host...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console());

    // Register layers
    builder.Services.AddInfrastructure(builder.Configuration);

    // Register API components
    builder.Services.AddErrorHandling();
    builder.Services.AddApiHealthChecks();
    builder.Services.AddSwaggerDocumentation();
    builder.Services.AddControllers();

    var app = builder.Build();

    app.UseExceptionHandler();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Engineering Knowledge Hub API v1");
            options.RoutePrefix = "swagger";
        });
    }

    app.UseHttpsRedirection();

    app.MapHealthChecks("/health");
    app.MapControllers();

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Engineering Knowledge Hub API terminated unexpectedly.");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }
