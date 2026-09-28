namespace EngineeringKnowledgeHub.Tests.Api;

using System.Net;
using System.Net.Http.Json;
using EngineeringKnowledgeHub.Api.Models;
using EngineeringKnowledgeHub.Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using Xunit;

public class ProductApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ProductApiTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateProduct_And_GetProduct_ShouldPersist_AndReturn201Created()
    {
        var uniqueCode = $"PROD_{Guid.NewGuid():N}"[..12];
        var request = new CreateProductRequest(
            Name: $"Integration Test Product {Guid.NewGuid():N}",
            Code: uniqueCode,
            Description: "Enterprise service bus gateway",
            IsActive: true);

        // 1. Create via POST /api/products
        var response = await _client.PostAsJsonAsync("/api/products", request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var created = await response.Content.ReadFromJsonAsync<ProductDto>();
        Assert.NotNull(created);
        Assert.NotEqual(Guid.Empty, created.Id);
        Assert.Equal(request.Name, created.Name);
        Assert.Equal(uniqueCode.ToUpperInvariant(), created.Code);
        Assert.Equal(request.Description, created.Description);
        Assert.True(created.IsActive);

        // 2. Query via GET /api/products/{id}
        var getResponse = await _client.GetAsync($"/api/products/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var fetched = await getResponse.Content.ReadFromJsonAsync<ProductDto>();
        Assert.NotNull(fetched);
        Assert.Equal(created.Id, fetched.Id);
        Assert.Equal(created.Name, fetched.Name);
        Assert.Equal(created.Code, fetched.Code);
    }

    [Fact]
    public async Task UpdateProduct_ShouldUpdateMetadata_AndReturn200Ok()
    {
        var uniqueCode = $"PRU_{Guid.NewGuid():N}"[..12];
        var createResponse = await _client.PostAsJsonAsync("/api/products", new CreateProductRequest(
            Name: $"Initial Product {Guid.NewGuid():N}",
            Code: uniqueCode,
            Description: "Initial description"));
        var created = await createResponse.Content.ReadFromJsonAsync<ProductDto>();
        Assert.NotNull(created);

        var updateRequest = new UpdateProductRequest(
            Name: "Updated Product Name",
            Code: $"{uniqueCode}_V2"[..12],
            Description: "Updated description");

        var updateResponse = await _client.PutAsJsonAsync($"/api/products/{created.Id}", updateRequest);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var updated = await updateResponse.Content.ReadFromJsonAsync<ProductDto>();
        Assert.NotNull(updated);
        Assert.Equal(updateRequest.Name, updated.Name);
        Assert.Equal(updateRequest.Code.ToUpperInvariant(), updated.Code);
        Assert.Equal(updateRequest.Description, updated.Description);
    }

    [Fact]
    public async Task ProductActivationLifecycle_Deactivate_And_Activate_ShouldTransitionStatus()
    {
        var uniqueCode = $"PLC_{Guid.NewGuid():N}"[..12];
        var createResponse = await _client.PostAsJsonAsync("/api/products", new CreateProductRequest(
            Name: $"Lifecycle Product {Guid.NewGuid():N}",
            Code: uniqueCode,
            IsActive: true));
        var created = await createResponse.Content.ReadFromJsonAsync<ProductDto>();
        Assert.NotNull(created);
        Assert.True(created.IsActive);

        // Deactivate
        var deactivateResponse = await _client.PostAsync($"/api/products/{created.Id}/deactivate", null);
        Assert.Equal(HttpStatusCode.OK, deactivateResponse.StatusCode);
        var deactivated = await deactivateResponse.Content.ReadFromJsonAsync<ProductDto>();
        Assert.NotNull(deactivated);
        Assert.False(deactivated.IsActive);

        // Activate
        var activateResponse = await _client.PostAsync($"/api/products/{created.Id}/activate", null);
        Assert.Equal(HttpStatusCode.OK, activateResponse.StatusCode);
        var activated = await activateResponse.Content.ReadFromJsonAsync<ProductDto>();
        Assert.NotNull(activated);
        Assert.True(activated.IsActive);
    }

    [Fact]
    public async Task GetProduct_WhenNotFound_ShouldReturn404NotFound()
    {
        var response = await _client.GetAsync($"/api/products/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(404, problem.Status);
    }

    [Fact]
    public async Task CreateProduct_WithDuplicateCode_ShouldReturn409Conflict()
    {
        var duplicateCode = $"DUP_{Guid.NewGuid():N}"[..12];
        var first = await _client.PostAsJsonAsync("/api/products", new CreateProductRequest(
            Name: "First Product",
            Code: duplicateCode));
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        var second = await _client.PostAsJsonAsync("/api/products", new CreateProductRequest(
            Name: "Second Product",
            Code: duplicateCode));
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);

        var problem = await second.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(409, problem.Status);
    }

    [Theory]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\r\n")]
    public async Task CreateProduct_WithWhitespaceName_ShouldReturn400BadRequest(string whitespace)
    {
        var request = new
        {
            Name = whitespace,
            Code = "VALID_CODE"
        };

        var response = await _client.PostAsJsonAsync("/api/products", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
    }

    [Theory]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\r\n")]
    public async Task CreateProduct_WithWhitespaceCode_ShouldReturn400BadRequest(string whitespace)
    {
        var request = new
        {
            Name = "Valid Product Name",
            Code = whitespace
        };

        var response = await _client.PostAsJsonAsync("/api/products", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
    }

    [Theory]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\r\n")]
    public async Task UpdateProduct_WithWhitespaceName_ShouldReturn400BadRequest(string whitespace)
    {
        var uniqueCode = $"WPU_{Guid.NewGuid():N}"[..12];
        var createResponse = await _client.PostAsJsonAsync("/api/products", new CreateProductRequest(
            Name: $"Product For Whitespace Update {Guid.NewGuid():N}",
            Code: uniqueCode));
        var created = await createResponse.Content.ReadFromJsonAsync<ProductDto>();
        Assert.NotNull(created);

        var invalidRequest = new
        {
            Name = whitespace,
            Code = "UPDATED_CODE"
        };

        var response = await _client.PutAsJsonAsync($"/api/products/{created.Id}", invalidRequest);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
    }
}
