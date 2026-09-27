namespace EngineeringKnowledgeHub.Api.Controllers;

using EngineeringKnowledgeHub.Application.Ai;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Operational endpoints for Local AI runtime health and diagnostics.
/// </summary>
[ApiController]
[Route("api/ai/local")]
public sealed class LocalAiController(ILocalAiHealthService? healthService = null) : ControllerBase
{
    /// <summary>
    /// Checks operational health and model readiness of the local AI subsystem.
    /// </summary>
    [HttpGet("health")]
    [ProducesResponseType<LocalAiHealthDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<LocalAiHealthDto>(StatusCodes.Status503ServiceUnavailable)]
    [ProducesResponseType<LocalAiHealthDto>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetHealth(CancellationToken cancellationToken = default)
    {
        if (healthService == null)
        {
            var disabledDto = new LocalAiHealthDto(
                Enabled: false,
                RuntimeReachable: false,
                GenerationModelConfigured: null,
                GenerationModelAvailable: false,
                EmbeddingModelConfigured: null,
                EmbeddingModelAvailable: false,
                Status: LocalAiHealthStatus.Disabled,
                DiagnosticMessage: "Local AI health service is not registered.");

            return Ok(disabledDto);
        }

        var health = await healthService.CheckHealthAsync(cancellationToken);

        return health.Status switch
        {
            LocalAiHealthStatus.Healthy or LocalAiHealthStatus.Disabled => Ok(health),
            LocalAiHealthStatus.Misconfigured => StatusCode(StatusCodes.Status500InternalServerError, health),
            _ => StatusCode(StatusCodes.Status503ServiceUnavailable, health)
        };
    }
}
