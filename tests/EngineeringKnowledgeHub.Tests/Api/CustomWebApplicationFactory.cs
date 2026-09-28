namespace EngineeringKnowledgeHub.Tests.Api;

using EngineeringKnowledgeHub.Domain.Entities;
using EngineeringKnowledgeHub.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly string _databaseName;
    private readonly string _connectionString;

    public Guid DefaultKnowledgeTypeId { get; private set; }
    public Guid DefaultTagId { get; private set; }
    public Guid DefaultProductId { get; private set; }
    public Guid DefaultTechnologyId { get; private set; }

    public CustomWebApplicationFactory()
    {
        _databaseName = $"EKH_ApiTest_{Guid.NewGuid():N}";
        _connectionString = $"Server=(localdb)\\mssqllocaldb;Database={_databaseName};Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<AppDbContext>();

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(_connectionString);
            });
        });
    }

    public async Task InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.MigrateAsync();

        DefaultKnowledgeTypeId = Guid.NewGuid();
        context.KnowledgeTypes.Add(new KnowledgeType(DefaultKnowledgeTypeId, "Architecture Guide", "System architecture"));

        DefaultTagId = Guid.NewGuid();
        context.KnowledgeTags.Add(new KnowledgeTag(DefaultTagId, "csharp"));

        DefaultProductId = Guid.NewGuid();
        context.Products.Add(new Product(DefaultProductId, "Knowledge Hub", "EKH"));

        DefaultTechnologyId = Guid.NewGuid();
        context.Technologies.Add(new Technology(DefaultTechnologyId, ".NET 10"));

        await context.SaveChangesAsync();
    }

    public new async Task DisposeAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        try
        {
            await context.Database.EnsureDeletedAsync();
        }
        catch
        {
            // Best-effort database cleanup
        }

        await base.DisposeAsync();
    }
}
