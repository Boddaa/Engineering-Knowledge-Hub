namespace EngineeringKnowledgeHub.Tests.Api;

using System.Net;
using System.Net.Http.Json;
using EngineeringKnowledgeHub.Api.Models;
using EngineeringKnowledgeHub.Application.DTOs;
using EngineeringKnowledgeHub.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Xunit;

public class IncidentApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public IncidentApiTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateIncident_ShouldPersist_AndReturn201Created()
    {
        var createdById = Guid.NewGuid();
        var request = new CreateIncidentRequest(
            Title: "Database Timeout on Checkout",
            ProblemDescription: "Connections to SQL Server timed out during peak load.",
            Severity: IncidentSeverity.High,
            CreatedById: createdById,
            Symptoms: "504 Gateway Timeout on /checkout endpoint");

        var response = await _client.PostAsJsonAsync("/api/incidents", request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var created = await response.Content.ReadFromJsonAsync<IncidentDto>();
        Assert.NotNull(created);
        Assert.NotEqual(Guid.Empty, created.Id);
        Assert.Equal(request.Title, created.Title);
        Assert.Equal(request.ProblemDescription, created.ProblemDescription);
        Assert.Equal(request.Symptoms, created.Symptoms);
        Assert.Equal(IncidentSeverity.High, created.Severity);
        Assert.Equal(IncidentStatus.Open, created.Status);
        Assert.Equal(createdById, created.CreatedById);

        // Verify lookup via GET /api/incidents/{id}
        var getResponse = await _client.GetAsync($"/api/incidents/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var fetched = await getResponse.Content.ReadFromJsonAsync<IncidentDto>();
        Assert.NotNull(fetched);
        Assert.Equal(created.Id, fetched.Id);
        Assert.Equal(created.Title, fetched.Title);
    }

    [Fact]
    public async Task GetIncidents_WithStatusFilter_ShouldReturnFilteredList()
    {
        var createdById = Guid.NewGuid();
        var request = new CreateIncidentRequest(
            Title: $"Incident Filter Test {Guid.NewGuid():N}",
            ProblemDescription: "Testing status filter query parameter",
            Severity: IncidentSeverity.Low,
            CreatedById: createdById);

        var createResponse = await _client.PostAsJsonAsync("/api/incidents", request);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var listResponse = await _client.GetAsync("/api/incidents?status=Open");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);

        var incidents = await listResponse.Content.ReadFromJsonAsync<List<IncidentDto>>();
        Assert.NotNull(incidents);
        Assert.NotEmpty(incidents);
        Assert.All(incidents, i => Assert.Equal(IncidentStatus.Open, i.Status));
    }

    [Fact]
    public async Task IncidentLifecycle_StartInvestigation_Resolve_And_Close_ShouldTransitionStates()
    {
        var createdById = Guid.NewGuid();
        var resolverId = Guid.NewGuid();

        // 1. Create
        var createResponse = await _client.PostAsJsonAsync("/api/incidents", new CreateIncidentRequest(
            Title: "Lifecycle Test Incident",
            ProblemDescription: "Testing incident lifecycle transitions",
            Severity: IncidentSeverity.Critical,
            CreatedById: createdById));
        var incident = await createResponse.Content.ReadFromJsonAsync<IncidentDto>();
        Assert.NotNull(incident);
        Assert.Equal(IncidentStatus.Open, incident.Status);

        // 2. Start Investigation
        var investigateResponse = await _client.PostAsync($"/api/incidents/{incident.Id}/investigate", null);
        Assert.Equal(HttpStatusCode.OK, investigateResponse.StatusCode);
        var investigated = await investigateResponse.Content.ReadFromJsonAsync<IncidentDto>();
        Assert.NotNull(investigated);
        Assert.Equal(IncidentStatus.Investigating, investigated.Status);

        // 3. Resolve
        var resolveRequest = new ResolveIncidentRequest(
            ResolvedById: resolverId,
            Resolution: "Increased connection pool size from 100 to 500.",
            RootCause: "Connection exhaustion under spike traffic.");
        var resolveResponse = await _client.PostAsJsonAsync($"/api/incidents/{incident.Id}/resolve", resolveRequest);
        Assert.Equal(HttpStatusCode.OK, resolveResponse.StatusCode);
        var resolved = await resolveResponse.Content.ReadFromJsonAsync<IncidentDto>();
        Assert.NotNull(resolved);
        Assert.Equal(IncidentStatus.Resolved, resolved.Status);
        Assert.Equal(resolverId, resolved.ResolvedById);
        Assert.Equal(resolveRequest.Resolution, resolved.Resolution);
        Assert.Equal(resolveRequest.RootCause, resolved.RootCause);

        // 4. Close
        var closeResponse = await _client.PostAsync($"/api/incidents/{incident.Id}/close", null);
        Assert.Equal(HttpStatusCode.OK, closeResponse.StatusCode);
        var closed = await closeResponse.Content.ReadFromJsonAsync<IncidentDto>();
        Assert.NotNull(closed);
        Assert.Equal(IncidentStatus.Closed, closed.Status);
    }

    [Fact]
    public async Task IncidentArticleLinks_LinkAndUnlinkArticle_ShouldManageAssociations()
    {
        // 1. Create Article
        var articleResponse = await _client.PostAsJsonAsync("/api/knowledge/articles", new CreateArticleRequest(
            Title: "Troubleshooting Connection Pools",
            Summary: "Guide to diagnosing pool starvation",
            Content: "Check connection metrics and increase max pool size.",
            KnowledgeTypeId: _factory.DefaultKnowledgeTypeId,
            AuthorId: Guid.NewGuid()));
        var article = await articleResponse.Content.ReadFromJsonAsync<KnowledgeArticleDto>();
        Assert.NotNull(article);

        // 2. Create Incident
        var incidentResponse = await _client.PostAsJsonAsync("/api/incidents", new CreateIncidentRequest(
            Title: "Connection Starvation Incident",
            ProblemDescription: "Pool ran out of connections",
            Severity: IncidentSeverity.High,
            CreatedById: Guid.NewGuid()));
        var incident = await incidentResponse.Content.ReadFromJsonAsync<IncidentDto>();
        Assert.NotNull(incident);

        // 3. Link Article
        var linkRequest = new LinkArticleRequest(article.Id, ArticleRelationshipType.Resolves);
        var linkResponse = await _client.PostAsJsonAsync($"/api/incidents/{incident.Id}/articles", linkRequest);
        Assert.Equal(HttpStatusCode.OK, linkResponse.StatusCode);
        var linkedIncident = await linkResponse.Content.ReadFromJsonAsync<IncidentDto>();
        Assert.NotNull(linkedIncident);
        Assert.Contains(linkedIncident.ArticleLinks, l => l.ArticleId == article.Id);

        // 4. Query incidents by article
        var getByArticleResponse = await _client.GetAsync($"/api/incidents/article/{article.Id}");
        Assert.Equal(HttpStatusCode.OK, getByArticleResponse.StatusCode);
        var incidentsForArticle = await getByArticleResponse.Content.ReadFromJsonAsync<List<IncidentDto>>();
        Assert.NotNull(incidentsForArticle);
        Assert.Contains(incidentsForArticle, i => i.Id == incident.Id);

        // 5. Unlink Article
        var unlinkResponse = await _client.DeleteAsync($"/api/incidents/{incident.Id}/articles/{article.Id}");
        Assert.Equal(HttpStatusCode.OK, unlinkResponse.StatusCode);
        var unlinkedIncident = await unlinkResponse.Content.ReadFromJsonAsync<IncidentDto>();
        Assert.NotNull(unlinkedIncident);
        Assert.DoesNotContain(unlinkedIncident.ArticleLinks, l => l.ArticleId == article.Id);
    }

    [Fact]
    public async Task CreateKnowledgeFromIncident_ShouldCreateDraftArticle_AndLinkIncident()
    {
        var authorId = Guid.NewGuid();

        // 1. Create and Resolve Incident
        var incidentResponse = await _client.PostAsJsonAsync("/api/incidents", new CreateIncidentRequest(
            Title: "Deadlock in Payment Processing",
            ProblemDescription: "Circular lock dependency between order and invoice tables.",
            Severity: IncidentSeverity.Critical,
            CreatedById: authorId,
            Symptoms: "Error 1205: Transaction was deadlocked"));
        var incident = await incidentResponse.Content.ReadFromJsonAsync<IncidentDto>();
        Assert.NotNull(incident);

        await _client.PostAsJsonAsync($"/api/incidents/{incident.Id}/resolve", new ResolveIncidentRequest(
            ResolvedById: authorId,
            Resolution: "Standardized table access order to always lock Orders before Invoices.",
            RootCause: "Inconsistent lock ordering across two microservices."));

        // 2. Propose draft article from resolved incident
        var proposeRequest = new CreateKnowledgeFromIncidentRequest(
            AuthorId: authorId,
            KnowledgeTypeId: _factory.DefaultKnowledgeTypeId,
            Title: "Standard Lock Ordering for Payment Processing");

        var proposeResponse = await _client.PostAsJsonAsync($"/api/incidents/{incident.Id}/propose-article", proposeRequest);
        Assert.Equal(HttpStatusCode.OK, proposeResponse.StatusCode);

        var article = await proposeResponse.Content.ReadFromJsonAsync<KnowledgeArticleDto>();
        Assert.NotNull(article);
        Assert.NotEqual(Guid.Empty, article.Id);
        Assert.Equal(KnowledgeStatus.Draft, article.Status);
        Assert.Equal(proposeRequest.Title, article.Title);
        Assert.Contains("Circular lock dependency", article.Content);
        Assert.Contains("Standardized table access order", article.Content);
    }

    [Fact]
    public async Task GetIncident_WhenNotFound_ShouldReturn404NotFound()
    {
        var response = await _client.GetAsync($"/api/incidents/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(404, problem.Status);
    }

    [Fact]
    public async Task CreateIncident_WithEmptyCreatedById_ShouldReturn400BadRequest()
    {
        var invalidRequest = new CreateIncidentRequest(
            Title: "Valid Title",
            ProblemDescription: "Valid Description",
            Severity: IncidentSeverity.Low,
            CreatedById: Guid.Empty);

        var response = await _client.PostAsJsonAsync("/api/incidents", invalidRequest);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
        Assert.Contains("CreatedById cannot be empty", problem.Detail);
    }

    [Fact]
    public async Task ResolveIncident_WhenIncidentNotFound_ShouldReturn404NotFound()
    {
        var request = new ResolveIncidentRequest(
            ResolvedById: Guid.NewGuid(),
            Resolution: "Fix applied.");

        var response = await _client.PostAsJsonAsync($"/api/incidents/{Guid.NewGuid()}/resolve", request);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\r\n")]
    public async Task CreateIncident_WithWhitespaceTitle_ShouldReturn400BadRequest(string whitespace)
    {
        var invalidRequest = new
        {
            Title = whitespace,
            ProblemDescription = "Valid Problem Description",
            Severity = IncidentSeverity.High,
            CreatedById = Guid.NewGuid()
        };

        var response = await _client.PostAsJsonAsync("/api/incidents", invalidRequest);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
    }

    [Theory]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\r\n")]
    public async Task CreateIncident_WithWhitespaceProblemDescription_ShouldReturn400BadRequest(string whitespace)
    {
        var invalidRequest = new
        {
            Title = "Valid Incident Title",
            ProblemDescription = whitespace,
            Severity = IncidentSeverity.High,
            CreatedById = Guid.NewGuid()
        };

        var response = await _client.PostAsJsonAsync("/api/incidents", invalidRequest);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
    }

    [Theory]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\r\n")]
    public async Task ResolveIncident_WithWhitespaceResolution_ShouldReturn400BadRequest(string whitespace)
    {
        var incidentResponse = await _client.PostAsJsonAsync("/api/incidents", new CreateIncidentRequest(
            Title: "Incident For Whitespace Resolve",
            ProblemDescription: "Description",
            Severity: IncidentSeverity.Low,
            CreatedById: Guid.NewGuid()));
        var incident = await incidentResponse.Content.ReadFromJsonAsync<IncidentDto>();
        Assert.NotNull(incident);

        var invalidRequest = new
        {
            ResolvedById = Guid.NewGuid(),
            Resolution = whitespace
        };

        var response = await _client.PostAsJsonAsync($"/api/incidents/{incident.Id}/resolve", invalidRequest);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
    }
}
