namespace EngineeringKnowledgeHub.Tests.Api;

using System.Net;
using System.Net.Http.Json;
using EngineeringKnowledgeHub.Api.Models;
using EngineeringKnowledgeHub.Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using Xunit;

public class TaxonomyApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public TaxonomyApiTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    // =========================================================================
    // Category Tests
    // =========================================================================

    [Fact]
    public async Task CreateCategory_And_GetCategory_ShouldPersist_AndReturn201Created()
    {
        var request = new CreateCategoryRequest(
            Name: $"Integration Category {Guid.NewGuid():N}",
            Description: "Infrastructure and cloud computing category");

        var response = await _client.PostAsJsonAsync("/api/taxonomy/categories", request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var created = await response.Content.ReadFromJsonAsync<KnowledgeCategoryDto>();
        Assert.NotNull(created);
        Assert.NotEqual(Guid.Empty, created.Id);
        Assert.Equal(request.Name, created.Name);
        Assert.Equal(request.Description, created.Description);
        Assert.Null(created.ParentCategoryId);

        var getResponse = await _client.GetAsync($"/api/taxonomy/categories/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var fetched = await getResponse.Content.ReadFromJsonAsync<KnowledgeCategoryDto>();
        Assert.NotNull(fetched);
        Assert.Equal(created.Id, fetched.Id);
        Assert.Equal(created.Name, fetched.Name);
        Assert.Equal(created.Description, fetched.Description);
    }

    [Fact]
    public async Task UpdateCategory_ShouldUpdateMetadata_AndReturn200Ok()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/taxonomy/categories", new CreateCategoryRequest(
            Name: $"Initial Category {Guid.NewGuid():N}",
            Description: "Initial description"));
        var created = await createResponse.Content.ReadFromJsonAsync<KnowledgeCategoryDto>();
        Assert.NotNull(created);

        var updateRequest = new UpdateCategoryRequest(
            Name: $"Updated Category {Guid.NewGuid():N}",
            Description: "Updated description");

        var updateResponse = await _client.PutAsJsonAsync($"/api/taxonomy/categories/{created.Id}", updateRequest);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var updated = await updateResponse.Content.ReadFromJsonAsync<KnowledgeCategoryDto>();
        Assert.NotNull(updated);
        Assert.Equal(created.Id, updated.Id);
        Assert.Equal(updateRequest.Name, updated.Name);
        Assert.Equal(updateRequest.Description, updated.Description);
    }

    [Fact]
    public async Task GetCategory_WhenNotFound_ShouldReturn404NotFound()
    {
        var response = await _client.GetAsync($"/api/taxonomy/categories/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(404, problem.Status);
    }

    [Fact]
    public async Task UpdateCategory_WhenNotFound_ShouldReturn404NotFound()
    {
        var updateRequest = new UpdateCategoryRequest(
            Name: "Non-existent Category",
            Description: "Description");

        var response = await _client.PutAsJsonAsync($"/api/taxonomy/categories/{Guid.NewGuid()}", updateRequest);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(404, problem.Status);
    }

    [Fact]
    public async Task CreateCategory_WithParentCategory_ShouldPersistParentRelationship()
    {
        var parentResponse = await _client.PostAsJsonAsync("/api/taxonomy/categories", new CreateCategoryRequest(
            Name: $"Parent Category {Guid.NewGuid():N}",
            Description: "Top-level category"));
        var parent = await parentResponse.Content.ReadFromJsonAsync<KnowledgeCategoryDto>();
        Assert.NotNull(parent);

        var childResponse = await _client.PostAsJsonAsync("/api/taxonomy/categories", new CreateCategoryRequest(
            Name: $"Child Category {Guid.NewGuid():N}",
            Description: "Sub-category",
            ParentCategoryId: parent.Id));
        Assert.Equal(HttpStatusCode.Created, childResponse.StatusCode);

        var child = await childResponse.Content.ReadFromJsonAsync<KnowledgeCategoryDto>();
        Assert.NotNull(child);
        Assert.Equal(parent.Id, child.ParentCategoryId);
    }

    [Fact]
    public async Task CreateCategory_WithNonExistentParentCategory_ShouldReturn404NotFound()
    {
        var nonExistentParentId = Guid.NewGuid();
        var request = new CreateCategoryRequest(
            Name: $"Orphan Category {Guid.NewGuid():N}",
            Description: "Category pointing to nonexistent parent",
            ParentCategoryId: nonExistentParentId);

        var response = await _client.PostAsJsonAsync("/api/taxonomy/categories", request);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(404, problem.Status);
    }

    [Fact]
    public async Task UpdateCategory_WithSelfAsParent_ShouldReturn409Conflict()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/taxonomy/categories", new CreateCategoryRequest(
            Name: $"Self-Parent Category {Guid.NewGuid():N}",
            Description: "Category to test self-referencing"));
        var created = await createResponse.Content.ReadFromJsonAsync<KnowledgeCategoryDto>();
        Assert.NotNull(created);

        var updateRequest = new UpdateCategoryRequest(
            Name: created.Name,
            Description: created.Description,
            ParentCategoryId: created.Id);

        var response = await _client.PutAsJsonAsync($"/api/taxonomy/categories/{created.Id}", updateRequest);
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(409, problem.Status);
    }

    [Fact]
    public async Task UpdateCategory_WithNonExistentParent_ShouldReturn404NotFound()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/taxonomy/categories", new CreateCategoryRequest(
            Name: $"Category For Parent Update {Guid.NewGuid():N}",
            Description: "Category"));
        var created = await createResponse.Content.ReadFromJsonAsync<KnowledgeCategoryDto>();
        Assert.NotNull(created);

        var updateRequest = new UpdateCategoryRequest(
            Name: created.Name,
            Description: created.Description,
            ParentCategoryId: Guid.NewGuid());

        var response = await _client.PutAsJsonAsync($"/api/taxonomy/categories/{created.Id}", updateRequest);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(404, problem.Status);
    }

    [Theory]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\r\n")]
    public async Task CreateCategory_WithWhitespaceName_ShouldReturn400BadRequest(string whitespace)
    {
        var request = new { Name = whitespace, Description = "Valid Description" };
        var response = await _client.PostAsJsonAsync("/api/taxonomy/categories", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
    }

    [Theory]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\r\n")]
    public async Task UpdateCategory_WithWhitespaceName_ShouldReturn400BadRequest(string whitespace)
    {
        var createResponse = await _client.PostAsJsonAsync("/api/taxonomy/categories", new CreateCategoryRequest(
            Name: $"Category For Whitespace Update {Guid.NewGuid():N}"));
        var created = await createResponse.Content.ReadFromJsonAsync<KnowledgeCategoryDto>();
        Assert.NotNull(created);

        var updateRequest = new { Name = whitespace, Description = "Valid Description" };
        var response = await _client.PutAsJsonAsync($"/api/taxonomy/categories/{created.Id}", updateRequest);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
    }

    [Fact]
    public async Task Category_WithEmptyGuid_ShouldReturn400BadRequest()
    {
        var getResponse = await _client.GetAsync($"/api/taxonomy/categories/{Guid.Empty}");
        Assert.Equal(HttpStatusCode.BadRequest, getResponse.StatusCode);

        var putResponse = await _client.PutAsJsonAsync($"/api/taxonomy/categories/{Guid.Empty}", new UpdateCategoryRequest("Name"));
        Assert.Equal(HttpStatusCode.BadRequest, putResponse.StatusCode);
    }

    [Fact]
    public async Task CreateCategory_WithEmptyParentGuid_ShouldReturn400BadRequest()
    {
        var request = new CreateCategoryRequest(
            Name: "Category With Empty Parent Guid",
            ParentCategoryId: Guid.Empty);

        var response = await _client.PostAsJsonAsync("/api/taxonomy/categories", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
    }

    // =========================================================================
    // Knowledge Type Tests
    // =========================================================================

    [Fact]
    public async Task CreateType_And_GetType_ShouldPersist_AndReturn201Created()
    {
        var request = new CreateKnowledgeTypeRequest(
            Name: $"Integration Type {Guid.NewGuid():N}",
            Description: "Runbook and troubleshooting guide");

        var response = await _client.PostAsJsonAsync("/api/taxonomy/types", request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var created = await response.Content.ReadFromJsonAsync<KnowledgeTypeDto>();
        Assert.NotNull(created);
        Assert.NotEqual(Guid.Empty, created.Id);
        Assert.Equal(request.Name, created.Name);
        Assert.Equal(request.Description, created.Description);

        var getResponse = await _client.GetAsync($"/api/taxonomy/types/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var fetched = await getResponse.Content.ReadFromJsonAsync<KnowledgeTypeDto>();
        Assert.NotNull(fetched);
        Assert.Equal(created.Id, fetched.Id);
        Assert.Equal(created.Name, fetched.Name);
        Assert.Equal(created.Description, fetched.Description);
    }

    [Fact]
    public async Task UpdateType_ShouldUpdateMetadata_AndReturn200Ok()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/taxonomy/types", new CreateKnowledgeTypeRequest(
            Name: $"Initial Type {Guid.NewGuid():N}",
            Description: "Initial type description"));
        var created = await createResponse.Content.ReadFromJsonAsync<KnowledgeTypeDto>();
        Assert.NotNull(created);

        var updateRequest = new UpdateKnowledgeTypeRequest(
            Name: $"Updated Type {Guid.NewGuid():N}",
            Description: "Updated type description");

        var updateResponse = await _client.PutAsJsonAsync($"/api/taxonomy/types/{created.Id}", updateRequest);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var updated = await updateResponse.Content.ReadFromJsonAsync<KnowledgeTypeDto>();
        Assert.NotNull(updated);
        Assert.Equal(created.Id, updated.Id);
        Assert.Equal(updateRequest.Name, updated.Name);
        Assert.Equal(updateRequest.Description, updated.Description);
    }

    [Fact]
    public async Task GetType_WhenNotFound_ShouldReturn404NotFound()
    {
        var response = await _client.GetAsync($"/api/taxonomy/types/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(404, problem.Status);
    }

    [Fact]
    public async Task UpdateType_WhenNotFound_ShouldReturn404NotFound()
    {
        var updateRequest = new UpdateKnowledgeTypeRequest(
            Name: "Non-existent Type",
            Description: "Description");

        var response = await _client.PutAsJsonAsync($"/api/taxonomy/types/{Guid.NewGuid()}", updateRequest);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(404, problem.Status);
    }

    [Theory]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\r\n")]
    public async Task CreateType_WithWhitespaceName_ShouldReturn400BadRequest(string whitespace)
    {
        var request = new { Name = whitespace, Description = "Valid Description" };
        var response = await _client.PostAsJsonAsync("/api/taxonomy/types", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
    }

    [Theory]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\r\n")]
    public async Task UpdateType_WithWhitespaceName_ShouldReturn400BadRequest(string whitespace)
    {
        var createResponse = await _client.PostAsJsonAsync("/api/taxonomy/types", new CreateKnowledgeTypeRequest(
            Name: $"Type For Whitespace Update {Guid.NewGuid():N}"));
        var created = await createResponse.Content.ReadFromJsonAsync<KnowledgeTypeDto>();
        Assert.NotNull(created);

        var updateRequest = new { Name = whitespace, Description = "Valid Description" };
        var response = await _client.PutAsJsonAsync($"/api/taxonomy/types/{created.Id}", updateRequest);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
    }

    [Fact]
    public async Task Type_WithEmptyGuid_ShouldReturn400BadRequest()
    {
        var getResponse = await _client.GetAsync($"/api/taxonomy/types/{Guid.Empty}");
        Assert.Equal(HttpStatusCode.BadRequest, getResponse.StatusCode);

        var putResponse = await _client.PutAsJsonAsync($"/api/taxonomy/types/{Guid.Empty}", new UpdateKnowledgeTypeRequest("Name"));
        Assert.Equal(HttpStatusCode.BadRequest, putResponse.StatusCode);
    }

    // =========================================================================
    // Knowledge Tag Tests
    // =========================================================================

    [Fact]
    public async Task CreateTag_And_GetTag_ShouldPersist_NormalizeName_AndReturn201Created()
    {
        var tagName = $"IntegrationTag_{Guid.NewGuid():N}"[..16];
        var request = new CreateKnowledgeTagRequest(Name: tagName.ToUpperInvariant());

        var response = await _client.PostAsJsonAsync("/api/taxonomy/tags", request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var created = await response.Content.ReadFromJsonAsync<KnowledgeTagDto>();
        Assert.NotNull(created);
        Assert.NotEqual(Guid.Empty, created.Id);
        // Domain normalizes tags to lowercase
        Assert.Equal(tagName.ToLowerInvariant(), created.Name);

        var getResponse = await _client.GetAsync($"/api/taxonomy/tags/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var fetched = await getResponse.Content.ReadFromJsonAsync<KnowledgeTagDto>();
        Assert.NotNull(fetched);
        Assert.Equal(created.Id, fetched.Id);
        Assert.Equal(tagName.ToLowerInvariant(), fetched.Name);
    }

    [Fact]
    public async Task UpdateTag_ShouldUpdateName_AndReturn200Ok()
    {
        var tagName = $"TagInit_{Guid.NewGuid():N}"[..14];
        var createResponse = await _client.PostAsJsonAsync("/api/taxonomy/tags", new CreateKnowledgeTagRequest(Name: tagName));
        var created = await createResponse.Content.ReadFromJsonAsync<KnowledgeTagDto>();
        Assert.NotNull(created);

        var updatedTagName = $"TagUpd_{Guid.NewGuid():N}"[..14];
        var updateRequest = new UpdateKnowledgeTagRequest(Name: updatedTagName);

        var updateResponse = await _client.PutAsJsonAsync($"/api/taxonomy/tags/{created.Id}", updateRequest);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var updated = await updateResponse.Content.ReadFromJsonAsync<KnowledgeTagDto>();
        Assert.NotNull(updated);
        Assert.Equal(created.Id, updated.Id);
        Assert.Equal(updatedTagName.ToLowerInvariant(), updated.Name);
    }

    [Fact]
    public async Task GetTag_WhenNotFound_ShouldReturn404NotFound()
    {
        var response = await _client.GetAsync($"/api/taxonomy/tags/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(404, problem.Status);
    }

    [Fact]
    public async Task UpdateTag_WhenNotFound_ShouldReturn404NotFound()
    {
        var updateRequest = new UpdateKnowledgeTagRequest(Name: "nonexistent-tag");

        var response = await _client.PutAsJsonAsync($"/api/taxonomy/tags/{Guid.NewGuid()}", updateRequest);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(404, problem.Status);
    }

    [Fact]
    public async Task CreateTag_WithDuplicateName_ShouldReturn409Conflict()
    {
        var duplicateName = $"dup-tag-{Guid.NewGuid():N}"[..16];
        var first = await _client.PostAsJsonAsync("/api/taxonomy/tags", new CreateKnowledgeTagRequest(Name: duplicateName));
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        var second = await _client.PostAsJsonAsync("/api/taxonomy/tags", new CreateKnowledgeTagRequest(Name: duplicateName.ToUpperInvariant()));
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);

        var problem = await second.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(409, problem.Status);
    }

    [Fact]
    public async Task UpdateTag_WithDuplicateExistingName_ShouldReturn409Conflict()
    {
        var firstTag = $"tag-alpha-{Guid.NewGuid():N}"[..16];
        var secondTag = $"tag-beta-{Guid.NewGuid():N}"[..16];

        await _client.PostAsJsonAsync("/api/taxonomy/tags", new CreateKnowledgeTagRequest(Name: firstTag));
        var secondResponse = await _client.PostAsJsonAsync("/api/taxonomy/tags", new CreateKnowledgeTagRequest(Name: secondTag));
        var createdSecond = await secondResponse.Content.ReadFromJsonAsync<KnowledgeTagDto>();
        Assert.NotNull(createdSecond);

        // Update second tag to have first tag's name
        var updateResponse = await _client.PutAsJsonAsync($"/api/taxonomy/tags/{createdSecond.Id}", new UpdateKnowledgeTagRequest(Name: firstTag));
        Assert.Equal(HttpStatusCode.Conflict, updateResponse.StatusCode);

        var problem = await updateResponse.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(409, problem.Status);
    }

    [Theory]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\r\n")]
    public async Task CreateTag_WithWhitespaceName_ShouldReturn400BadRequest(string whitespace)
    {
        var request = new { Name = whitespace };
        var response = await _client.PostAsJsonAsync("/api/taxonomy/tags", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
    }

    [Theory]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\r\n")]
    public async Task UpdateTag_WithWhitespaceName_ShouldReturn400BadRequest(string whitespace)
    {
        var tagName = $"tag-ws-{Guid.NewGuid():N}"[..14];
        var createResponse = await _client.PostAsJsonAsync("/api/taxonomy/tags", new CreateKnowledgeTagRequest(Name: tagName));
        var created = await createResponse.Content.ReadFromJsonAsync<KnowledgeTagDto>();
        Assert.NotNull(created);

        var updateRequest = new { Name = whitespace };
        var response = await _client.PutAsJsonAsync($"/api/taxonomy/tags/{created.Id}", updateRequest);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
    }

    [Fact]
    public async Task Tag_WithEmptyGuid_ShouldReturn400BadRequest()
    {
        var getResponse = await _client.GetAsync($"/api/taxonomy/tags/{Guid.Empty}");
        Assert.Equal(HttpStatusCode.BadRequest, getResponse.StatusCode);

        var putResponse = await _client.PutAsJsonAsync($"/api/taxonomy/tags/{Guid.Empty}", new UpdateKnowledgeTagRequest("valid-tag"));
        Assert.Equal(HttpStatusCode.BadRequest, putResponse.StatusCode);
    }

    // =========================================================================
    // Collection Listing and Seed Tests (Phase 11)
    // =========================================================================

    [Fact]
    public async Task GetCategories_ShouldReturnCollection_AndSupportBothRoutes()
    {
        var categoryName = $"Category For Listing {Guid.NewGuid():N}";
        var createResponse = await _client.PostAsJsonAsync("/api/taxonomy/categories", new CreateCategoryRequest(
            Name: categoryName,
            Description: "Listing test description"));
        var created = await createResponse.Content.ReadFromJsonAsync<KnowledgeCategoryDto>();
        Assert.NotNull(created);

        // 1. GET /api/taxonomy/categories
        var response1 = await _client.GetAsync("/api/taxonomy/categories");
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);
        var list1 = await response1.Content.ReadFromJsonAsync<IReadOnlyList<KnowledgeCategoryDto>>();
        Assert.NotNull(list1);
        Assert.Contains(list1, c => c.Id == created.Id);

        // 2. GET /api/categories (Architecture Section 23 route alias)
        var response2 = await _client.GetAsync("/api/categories");
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        var list2 = await response2.Content.ReadFromJsonAsync<IReadOnlyList<KnowledgeCategoryDto>>();
        Assert.NotNull(list2);
        Assert.Contains(list2, c => c.Id == created.Id);
    }

    [Fact]
    public async Task GetTypes_ShouldReturnCollection_AndReturn200Ok()
    {
        var typeName = $"Type For Listing {Guid.NewGuid():N}";
        var createResponse = await _client.PostAsJsonAsync("/api/taxonomy/types", new CreateKnowledgeTypeRequest(
            Name: typeName,
            Description: "Listing test type description"));
        var created = await createResponse.Content.ReadFromJsonAsync<KnowledgeTypeDto>();
        Assert.NotNull(created);

        var response = await _client.GetAsync("/api/taxonomy/types");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var list = await response.Content.ReadFromJsonAsync<IReadOnlyList<KnowledgeTypeDto>>();
        Assert.NotNull(list);
        Assert.Contains(list, t => t.Id == created.Id);
    }

    [Fact]
    public async Task GetTags_ShouldReturnCollection_AndReturn200Ok()
    {
        var tagName = $"tag-list-{Guid.NewGuid():N}"[..16];
        var createResponse = await _client.PostAsJsonAsync("/api/taxonomy/tags", new CreateKnowledgeTagRequest(
            Name: tagName));
        var created = await createResponse.Content.ReadFromJsonAsync<KnowledgeTagDto>();
        Assert.NotNull(created);

        var response = await _client.GetAsync("/api/taxonomy/tags");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var list = await response.Content.ReadFromJsonAsync<IReadOnlyList<KnowledgeTagDto>>();
        Assert.NotNull(list);
        Assert.Contains(list, t => t.Id == created.Id);
    }

    [Fact]
    public async Task PostSeed_ShouldSeedDefaultCatalog_AndBeIdempotentOnSubsequentCalls()
    {
        // 1. First seed invocation
        var firstResponse = await _client.PostAsync("/api/taxonomy/seed", null);
        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
        var firstResult = await firstResponse.Content.ReadFromJsonAsync<TaxonomySeedResultDto>();
        Assert.NotNull(firstResult);

        // Verify that seeded categories, types, products, technologies, and tags exist in listing APIs
        var typesResponse = await _client.GetAsync("/api/taxonomy/types");
        var types = await typesResponse.Content.ReadFromJsonAsync<IReadOnlyList<KnowledgeTypeDto>>();
        Assert.NotNull(types);
        Assert.Contains(types, t => t.Name == "Runbook");
        Assert.Contains(types, t => t.Name == "Troubleshooting Guide");
        Assert.Contains(types, t => t.Name == "Incident Postmortem");

        var categoriesResponse = await _client.GetAsync("/api/taxonomy/categories");
        var categories = await categoriesResponse.Content.ReadFromJsonAsync<IReadOnlyList<KnowledgeCategoryDto>>();
        Assert.NotNull(categories);
        Assert.Contains(categories, c => c.Name == "Backend & APIs");
        Assert.Contains(categories, c => c.Name == "Infrastructure & IIS");

        var productsResponse = await _client.GetAsync("/api/products");
        var products = await productsResponse.Content.ReadFromJsonAsync<IReadOnlyList<ProductDto>>();
        Assert.NotNull(products);
        Assert.Contains(products, p => p.Code == "SUEZ-TOLL");
        Assert.Contains(products, p => p.Code == "EKH");

        var techResponse = await _client.GetAsync("/api/technologies");
        var techs = await techResponse.Content.ReadFromJsonAsync<IReadOnlyList<TechnologyDto>>();
        Assert.NotNull(techs);
        Assert.Contains(techs, t => t.Name == "ASP.NET Core");
        Assert.Contains(techs, t => t.Name == "SignalR");

        var tagsResponse = await _client.GetAsync("/api/taxonomy/tags");
        var tags = await tagsResponse.Content.ReadFromJsonAsync<IReadOnlyList<KnowledgeTagDto>>();
        Assert.NotNull(tags);
        Assert.Contains(tags, t => t.Name == "signalr");
        Assert.Contains(tags, t => t.Name == "runbook");

        // 2. Second seed invocation (must be 100% idempotent, adding 0 items)
        var secondResponse = await _client.PostAsync("/api/taxonomy/seed", null);
        Assert.Equal(HttpStatusCode.OK, secondResponse.StatusCode);
        var secondResult = await secondResponse.Content.ReadFromJsonAsync<TaxonomySeedResultDto>();
        Assert.NotNull(secondResult);
        Assert.Equal(0, secondResult.TypesAdded);
        Assert.Equal(0, secondResult.CategoriesAdded);
        Assert.Equal(0, secondResult.ProductsAdded);
        Assert.Equal(0, secondResult.TechnologiesAdded);
        Assert.Equal(0, secondResult.TagsAdded);
    }
}
