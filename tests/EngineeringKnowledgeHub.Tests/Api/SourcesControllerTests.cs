namespace EngineeringKnowledgeHub.Tests.Api;

using EngineeringKnowledgeHub.Api.Controllers;
using EngineeringKnowledgeHub.Application.Common.Exceptions;
using EngineeringKnowledgeHub.Application.DTOs;
using EngineeringKnowledgeHub.Application.Enrichment;
using EngineeringKnowledgeHub.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

public class SourcesControllerTests
{
    private sealed class FakeEnrichmentService(
        Func<Guid, ExternalImportCommand?, CancellationToken, Task<ExternalImportResultDto>> importFunc)
        : IExternalEnrichmentService
    {
        public Task<ExternalImportResultDto> ImportFromSourceAsync(Guid sourceId, ExternalImportCommand? command = null, CancellationToken cancellationToken = default)
        {
            return importFunc(sourceId, command, cancellationToken);
        }

        public Task<KnowledgeArticleDto> ProposeArticleFromDocumentAsync(ProposeArticleFromDocumentCommand command, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }

    [Fact]
    public async Task ImportFromSource_WithEmptyId_ShouldReturnBadRequest()
    {
        var controller = new SourcesController();
        var result = await controller.ImportFromSource(Guid.Empty);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var details = Assert.IsType<ProblemDetails>(badRequest.Value);
        Assert.Equal(StatusCodes.Status400BadRequest, details.Status);
    }

    [Fact]
    public async Task ImportFromSource_WhenServiceUnconfigured_ShouldReturnServiceUnavailable()
    {
        var controller = new SourcesController(null);
        var result = await controller.ImportFromSource(Guid.NewGuid());

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, objectResult.StatusCode);
    }

    [Fact]
    public async Task ImportFromSource_WhenSourceNotFound_ShouldReturnNotFound()
    {
        var service = new FakeEnrichmentService((id, _, _) =>
            throw new NotFoundException("KnowledgeSource", id));

        var controller = new SourcesController(service);
        var result = await controller.ImportFromSource(Guid.NewGuid());

        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        var details = Assert.IsType<ProblemDetails>(notFoundResult.Value);
        Assert.Equal(StatusCodes.Status404NotFound, details.Status);
    }

    [Fact]
    public async Task ImportFromSource_WhenValidationFails_ShouldReturnBadRequest()
    {
        var service = new FakeEnrichmentService((_, _, _) =>
            throw new ValidationException("Source is not approved."));

        var controller = new SourcesController(service);
        var result = await controller.ImportFromSource(Guid.NewGuid());

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var details = Assert.IsType<ProblemDetails>(badRequestResult.Value);
        Assert.Equal(StatusCodes.Status400BadRequest, details.Status);
    }

    [Fact]
    public async Task ImportFromSource_WhenSuccessful_ShouldReturnOkWithResultDto()
    {
        var sourceId = Guid.NewGuid();
        var expectedDto = new ExternalImportResultDto(
            IsSuccess: true,
            SourceId: sourceId,
            SourceName: "Vendor Manual",
            SourceType: SourceType.VendorDocumentation,
            Url: "https://vendor.example.com",
            DocumentId: Guid.NewGuid(),
            FileName: "manual.txt",
            ContentType: "text/plain",
            FileHash: "abc123hash",
            ChunkCount: 3,
            IsDuplicate: false,
            ErrorCategory: null,
            ErrorMessage: null,
            Timestamp: DateTimeOffset.UtcNow);

        var service = new FakeEnrichmentService((_, _, _) => Task.FromResult(expectedDto));
        var controller = new SourcesController(service);

        var actionResult = await controller.ImportFromSource(sourceId);

        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        var returnedDto = Assert.IsType<ExternalImportResultDto>(okResult.Value);
        Assert.True(returnedDto.IsSuccess);
        Assert.Equal(sourceId, returnedDto.SourceId);
        Assert.Equal("manual.txt", returnedDto.FileName);
    }
}
