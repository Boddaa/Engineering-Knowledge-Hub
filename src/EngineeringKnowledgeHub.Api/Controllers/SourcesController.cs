namespace EngineeringKnowledgeHub.Api.Controllers;

using EngineeringKnowledgeHub.Application.Common.Exceptions;
using EngineeringKnowledgeHub.Application.Enrichment;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// API endpoint for external knowledge source online enrichment.
/// </summary>
[ApiController]
[Route("api/sources")]
public sealed class SourcesController(IExternalEnrichmentService? enrichmentService = null) : ControllerBase
{
    [HttpPost("{id:guid}/import")]
    [ProducesResponseType<ExternalImportResultDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> ImportFromSource(
        Guid id,
        [FromBody] ExternalImportCommand? command = null,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation Error",
                Detail = "Source ID cannot be empty."
            });
        }

        if (enrichmentService == null)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new ProblemDetails
            {
                Status = StatusCodes.Status503ServiceUnavailable,
                Title = "Service Unavailable",
                Detail = "External enrichment service is not configured."
            });
        }

        try
        {
            var result = await enrichmentService.ImportFromSourceAsync(id, command, cancellationToken);
            return Ok(result);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Source Not Found",
                Detail = ex.Message
            });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation Error",
                Detail = ex.Message
            });
        }
    }
}
