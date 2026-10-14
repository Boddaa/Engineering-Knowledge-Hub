namespace EngineeringKnowledgeHub.Tests.Api;

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using EngineeringKnowledgeHub.Application.DTOs;
using Xunit;

public class AuditSecurityApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public AuditSecurityApiTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetAuditLogs_WithoutAuthentication_ShouldReturn401Unauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/audit");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetEntityAuditLogs_WithoutAuthentication_ShouldReturn401Unauthorized()
    {
        using var client = _factory.CreateClient();
        var entityId = Guid.NewGuid();

        var response = await client.GetAsync($"/api/audit/entities/KnowledgeArticle/{entityId}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetUserAuditLogs_WithoutAuthentication_ShouldReturn401Unauthorized()
    {
        using var client = _factory.CreateClient();
        var userId = Guid.NewGuid();

        var response = await client.GetAsync($"/api/audit/users/{userId}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetAuditLogs_WithInvalidToken_ShouldReturn401Unauthorized()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "completely-invalid-token");

        var response = await client.GetAsync("/api/audit");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AuditEndpoints_WithNonAuditorUser_ShouldReturn403Forbidden()
    {
        using var client = _factory.CreateClient();
        // dev-engineer-token has roles [ "Engineer", "Viewer" ] which lack "Admin" and "Auditor"
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "dev-engineer-token");
        var testId = Guid.NewGuid();

        // 1. Global audit
        var globalResponse = await client.GetAsync("/api/audit");
        Assert.Equal(HttpStatusCode.Forbidden, globalResponse.StatusCode);

        // 2. Entity audit
        var entityResponse = await client.GetAsync($"/api/audit/entities/KnowledgeArticle/{testId}");
        Assert.Equal(HttpStatusCode.Forbidden, entityResponse.StatusCode);

        // 3. User audit
        var userResponse = await client.GetAsync($"/api/audit/users/{testId}");
        Assert.Equal(HttpStatusCode.Forbidden, userResponse.StatusCode);
    }

    [Fact]
    public async Task GetAuditLogs_WithAuditorRole_ShouldReturn200Ok()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "dev-auditor-token");

        var response = await client.GetAsync("/api/audit");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var logs = await response.Content.ReadFromJsonAsync<List<AuditLogDto>>();
        Assert.NotNull(logs);
    }

    [Fact]
    public async Task GetAuditLogs_WithAdminRole_ShouldReturn200Ok()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "dev-admin-token");

        var response = await client.GetAsync("/api/audit");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var logs = await response.Content.ReadFromJsonAsync<List<AuditLogDto>>();
        Assert.NotNull(logs);
    }

    [Fact]
    public async Task EntityAndUserAuditLogs_WithAuditorRole_ShouldReturn200Ok()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "dev-auditor-token");
        var testId = Guid.NewGuid();

        var entityResponse = await client.GetAsync($"/api/audit/entities/KnowledgeArticle/{testId}");
        Assert.Equal(HttpStatusCode.OK, entityResponse.StatusCode);

        var userResponse = await client.GetAsync($"/api/audit/users/{testId}");
        Assert.Equal(HttpStatusCode.OK, userResponse.StatusCode);
    }

    [Fact]
    public async Task AuditEndpoints_WhenAuthorized_WithInvalidParameters_ShouldReturn400BadRequest()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "dev-auditor-token");

        // 1. Empty GUID for entity -> 400
        var response1 = await client.GetAsync($"/api/audit/entities/KnowledgeArticle/{Guid.Empty}");
        Assert.Equal(HttpStatusCode.BadRequest, response1.StatusCode);

        // 2. Empty GUID for user -> 400
        var response2 = await client.GetAsync($"/api/audit/users/{Guid.Empty}");
        Assert.Equal(HttpStatusCode.BadRequest, response2.StatusCode);

        // 3. Whitespace entity name -> 400
        var response3 = await client.GetAsync($"/api/audit/entities/%20/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.BadRequest, response3.StatusCode);
    }

    [Fact]
    public async Task HealthEndpoint_WithoutAuthentication_ShouldReturn200Ok()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
