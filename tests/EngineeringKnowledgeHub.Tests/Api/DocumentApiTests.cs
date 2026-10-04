namespace EngineeringKnowledgeHub.Tests.Api;

using System.Net;
using System.Net.Http.Json;
using EngineeringKnowledgeHub.Api.Models;
using EngineeringKnowledgeHub.Application.DTOs;
using EngineeringKnowledgeHub.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Xunit;

public class DocumentApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public DocumentApiTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task UploadDocument_And_GetDocument_ShouldPersist_AndReturn201Created()
    {
        var uploaderId = Guid.NewGuid();
        var uniqueContent = $"Document content for upload test: {Guid.NewGuid():N}";
        var request = new UploadDocumentRequest(
            FileName: "architecture_spec.txt",
            ContentType: "text/plain",
            SourceId: _factory.DefaultSourceId,
            UploadedById: uploaderId,
            Content: uniqueContent);

        // 1. Upload via POST /api/documents
        var uploadResponse = await _client.PostAsJsonAsync("/api/documents", request);
        Assert.Equal(HttpStatusCode.Created, uploadResponse.StatusCode);
        Assert.NotNull(uploadResponse.Headers.Location);

        var document = await uploadResponse.Content.ReadFromJsonAsync<DocumentDto>();
        Assert.NotNull(document);
        Assert.NotEqual(Guid.Empty, document.Id);
        Assert.Equal(request.FileName, document.FileName);
        Assert.Equal(request.ContentType, document.ContentType);
        Assert.Equal(_factory.DefaultSourceId, document.SourceId);
        Assert.Equal(uploaderId, document.UploadedById);
        Assert.NotEmpty(document.FileHash);
        Assert.Equal(DocumentProcessingStatus.Pending, document.ProcessingStatus);

        // 2. Query via GET /api/documents/{id}
        var getResponse = await _client.GetAsync($"/api/documents/{document.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var fetched = await getResponse.Content.ReadFromJsonAsync<DocumentDto>();
        Assert.NotNull(fetched);
        Assert.Equal(document.Id, fetched.Id);
        Assert.Equal(document.FileHash, fetched.FileHash);
    }

    [Fact]
    public async Task GetDocuments_BySourceId_ShouldReturnDocumentList()
    {
        var uploaderId = Guid.NewGuid();
        var request = new UploadDocumentRequest(
            FileName: "source_doc.txt",
            ContentType: "text/plain",
            SourceId: _factory.DefaultSourceId,
            UploadedById: uploaderId,
            Content: $"Content for source list test: {Guid.NewGuid():N}");

        var uploadResponse = await _client.PostAsJsonAsync("/api/documents", request);
        Assert.Equal(HttpStatusCode.Created, uploadResponse.StatusCode);
        var document = await uploadResponse.Content.ReadFromJsonAsync<DocumentDto>();
        Assert.NotNull(document);

        var listResponse = await _client.GetAsync($"/api/documents/source/{_factory.DefaultSourceId}");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);

        var docs = await listResponse.Content.ReadFromJsonAsync<List<DocumentDto>>();
        Assert.NotNull(docs);
        Assert.Contains(docs, d => d.Id == document.Id);
    }

    [Fact]
    public async Task ProcessDocument_And_ChunkDocument_ShouldExtractTextAndChunks()
    {
        var rawContent = "This is the primary specification for the engineering architecture. It defines system interfaces and requirements.";
        var uploadResponse = await _client.PostAsJsonAsync("/api/documents", new UploadDocumentRequest(
            FileName: "processing_test.txt",
            ContentType: "text/plain",
            SourceId: _factory.DefaultSourceId,
            UploadedById: Guid.NewGuid(),
            Content: rawContent));
        var document = await uploadResponse.Content.ReadFromJsonAsync<DocumentDto>();
        Assert.NotNull(document);

        // 1. Process document
        var processResponse = await _client.PostAsJsonAsync($"/api/documents/{document.Id}/process", new ProcessDocumentRequest(rawContent));
        Assert.Equal(HttpStatusCode.OK, processResponse.StatusCode);

        var processed = await processResponse.Content.ReadFromJsonAsync<DocumentDto>();
        Assert.NotNull(processed);
        Assert.Equal(DocumentProcessingStatus.Processed, processed.ProcessingStatus);
        Assert.NotNull(processed.ExtractedText);
        Assert.Contains("engineering architecture", processed.ExtractedText);

        // 2. Chunk document
        var chunkResponse = await _client.PostAsJsonAsync($"/api/documents/{document.Id}/chunk", new ChunkDocumentRequest(TargetChunkSize: 50, OverlapTokens: 10));
        Assert.Equal(HttpStatusCode.OK, chunkResponse.StatusCode);

        var chunks = await chunkResponse.Content.ReadFromJsonAsync<List<DocumentChunkDto>>();
        Assert.NotNull(chunks);
        Assert.NotEmpty(chunks);
        Assert.All(chunks, c => Assert.Equal(document.Id, c.DocumentId));
    }

    [Fact]
    public async Task GetDocument_WhenNotFound_ShouldReturn404NotFound()
    {
        var response = await _client.GetAsync($"/api/documents/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(404, problem.Status);
    }

    [Fact]
    public async Task UploadDocument_WhenSourceNotFound_ShouldReturn404NotFound()
    {
        var request = new UploadDocumentRequest(
            FileName: "orphan_doc.txt",
            ContentType: "text/plain",
            SourceId: Guid.NewGuid(),
            UploadedById: Guid.NewGuid(),
            Content: "Some content");

        var response = await _client.PostAsJsonAsync("/api/documents", request);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(404, problem.Status);
    }

    [Fact]
    public async Task UploadDocument_WhenDuplicateContentHash_ShouldReturn409Conflict()
    {
        var content = $"Identical content for duplicate test: {Guid.NewGuid():N}";

        var request1 = new UploadDocumentRequest(
            FileName: "original.txt",
            ContentType: "text/plain",
            SourceId: _factory.DefaultSourceId,
            UploadedById: Guid.NewGuid(),
            Content: content);

        var response1 = await _client.PostAsJsonAsync("/api/documents", request1);
        Assert.Equal(HttpStatusCode.Created, response1.StatusCode);

        var request2 = new UploadDocumentRequest(
            FileName: "duplicate.txt",
            ContentType: "text/plain",
            SourceId: _factory.DefaultSourceId,
            UploadedById: Guid.NewGuid(),
            Content: content);

        var response2 = await _client.PostAsJsonAsync("/api/documents", request2);
        Assert.Equal(HttpStatusCode.Conflict, response2.StatusCode);

        var problem = await response2.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(409, problem.Status);
    }

    [Fact]
    public async Task UploadDocument_WithEmptySourceId_ShouldReturn400BadRequest()
    {
        var request = new UploadDocumentRequest(
            FileName: "file.txt",
            ContentType: "text/plain",
            SourceId: Guid.Empty,
            UploadedById: Guid.NewGuid(),
            Content: "content");

        var response = await _client.PostAsJsonAsync("/api/documents", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
        Assert.Contains("SourceId cannot be empty", problem.Detail);
    }

    [Theory]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\r\n")]
    public async Task UploadDocument_WithWhitespaceFileName_ShouldReturn400BadRequest(string whitespace)
    {
        var invalidRequest = new
        {
            FileName = whitespace,
            ContentType = "text/plain",
            SourceId = _factory.DefaultSourceId,
            UploadedById = Guid.NewGuid(),
            Content = "valid content"
        };

        var response = await _client.PostAsJsonAsync("/api/documents", invalidRequest);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
    }

    [Theory]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\r\n")]
    public async Task UploadDocument_WithWhitespaceContentType_ShouldReturn400BadRequest(string whitespace)
    {
        var invalidRequest = new
        {
            FileName = "valid.txt",
            ContentType = whitespace,
            SourceId = _factory.DefaultSourceId,
            UploadedById = Guid.NewGuid(),
            Content = "valid content"
        };

        var response = await _client.PostAsJsonAsync("/api/documents", invalidRequest);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
    }

    [Theory]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\r\n")]
    public async Task UploadDocument_WithWhitespaceContent_ShouldReturn400BadRequest(string whitespace)
    {
        var invalidRequest = new
        {
            FileName = "valid.txt",
            ContentType = "text/plain",
            SourceId = _factory.DefaultSourceId,
            UploadedById = Guid.NewGuid(),
            Content = whitespace
        };

        var response = await _client.PostAsJsonAsync("/api/documents", invalidRequest);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
    }

    [Fact]
    public async Task ProposeArticle_FromDocument_ShouldCreateDraftUnverifiedArticle_AndLinkToSource()
    {
        // 1. Upload and process a document
        var authorId = Guid.NewGuid();
        var uploadRequest = new UploadDocumentRequest(
            FileName: "vendor_manual.txt",
            ContentType: "text/plain",
            SourceId: _factory.DefaultSourceId,
            UploadedById: authorId,
            Content: "Vendor manual section 1.2: Database failover procedure.");

        var uploadResponse = await _client.PostAsJsonAsync("/api/documents", uploadRequest);
        Assert.Equal(HttpStatusCode.Created, uploadResponse.StatusCode);
        var document = await uploadResponse.Content.ReadFromJsonAsync<DocumentDto>();
        Assert.NotNull(document);

        var processResponse = await _client.PostAsJsonAsync($"/api/documents/{document.Id}/process", new ProcessDocumentRequest());
        Assert.Equal(HttpStatusCode.OK, processResponse.StatusCode);

        // 2. Propose draft article from document
        var proposeRequest = new ProposeArticleFromDocumentRequest(
            Title: $"Vendor Database Failover Guide {Guid.NewGuid():N}",
            AuthorId: authorId,
            KnowledgeTypeId: _factory.DefaultKnowledgeTypeId,
            Summary: "Database failover runbook from vendor documentation.",
            Content: "Step 1: Check primary health. Step 2: Trigger promotion.");

        var proposeResponse = await _client.PostAsJsonAsync($"/api/documents/{document.Id}/propose-article", proposeRequest);
        Assert.Equal(HttpStatusCode.OK, proposeResponse.StatusCode);

        var article = await proposeResponse.Content.ReadFromJsonAsync<KnowledgeArticleDto>();
        Assert.NotNull(article);
        Assert.NotEqual(Guid.Empty, article.Id);
        Assert.Equal(proposeRequest.Title, article.Title);
        Assert.Equal(KnowledgeStatus.Draft, article.Status);
        Assert.Equal(VerificationStatus.Unverified, article.VerificationStatus);
        Assert.Equal(authorId, article.AuthorId);
        Assert.Equal(_factory.DefaultKnowledgeTypeId, article.KnowledgeTypeId);

        // 3. Verify article is accessible via articles API
        var getArticleResponse = await _client.GetAsync($"/api/knowledge/articles/{article.Id}");
        Assert.Equal(HttpStatusCode.OK, getArticleResponse.StatusCode);
    }

    [Fact]
    public async Task ProposeArticle_WithEmptyDocumentId_ShouldReturn400BadRequest()
    {
        var request = new ProposeArticleFromDocumentRequest(
            Title: "Valid Title",
            AuthorId: Guid.NewGuid(),
            KnowledgeTypeId: _factory.DefaultKnowledgeTypeId);

        var response = await _client.PostAsJsonAsync($"/api/documents/{Guid.Empty}/propose-article", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
        Assert.Contains("Document ID cannot be empty", problem.Detail);
    }

    [Fact]
    public async Task ProposeArticle_WithEmptyAuthorId_ShouldReturn400BadRequest()
    {
        var request = new ProposeArticleFromDocumentRequest(
            Title: "Valid Title",
            AuthorId: Guid.Empty,
            KnowledgeTypeId: _factory.DefaultKnowledgeTypeId);

        var response = await _client.PostAsJsonAsync($"/api/documents/{Guid.NewGuid()}/propose-article", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
    }

    [Fact]
    public async Task ProposeArticle_WithEmptyKnowledgeTypeId_ShouldReturn400BadRequest()
    {
        var request = new ProposeArticleFromDocumentRequest(
            Title: "Valid Title",
            AuthorId: Guid.NewGuid(),
            KnowledgeTypeId: Guid.Empty);

        var response = await _client.PostAsJsonAsync($"/api/documents/{Guid.NewGuid()}/propose-article", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
    }

    [Fact]
    public async Task ProposeArticle_WithNonExistentDocument_ShouldReturn404NotFound()
    {
        var request = new ProposeArticleFromDocumentRequest(
            Title: "Valid Title",
            AuthorId: Guid.NewGuid(),
            KnowledgeTypeId: _factory.DefaultKnowledgeTypeId);

        var missingDocumentId = Guid.NewGuid();
        var response = await _client.PostAsJsonAsync($"/api/documents/{missingDocumentId}/propose-article", request);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(404, problem.Status);
        Assert.Equal("Not Found", problem.Title);
    }

    [Theory]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\r\n")]
    public async Task ProposeArticle_WithWhitespaceTitle_ShouldReturn400BadRequest(string whitespaceTitle)
    {
        var request = new ProposeArticleFromDocumentRequest(
            Title: whitespaceTitle,
            AuthorId: Guid.NewGuid(),
            KnowledgeTypeId: _factory.DefaultKnowledgeTypeId);

        var response = await _client.PostAsJsonAsync($"/api/documents/{Guid.NewGuid()}/propose-article", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
    }
}
