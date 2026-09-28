namespace EngineeringKnowledgeHub.Api.Controllers;

using EngineeringKnowledgeHub.Api.Models;
using EngineeringKnowledgeHub.Application.Ai;
using EngineeringKnowledgeHub.Application.Rag;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Operational endpoints for Local AI runtime health, diagnostics, and text generation.
/// </summary>
[ApiController]
[Route("api/ai")]
[Route("api/ai/local")]
public sealed class LocalAiController(
    ILocalAiHealthService healthService,
    IAiProvider aiProvider) : ControllerBase
{
    private readonly ILocalAiHealthService _healthService = healthService ?? throw new ArgumentNullException(nameof(healthService));
    private readonly IAiProvider _aiProvider = aiProvider ?? throw new ArgumentNullException(nameof(aiProvider));

    /// <summary>
    /// Checks operational health and model readiness of the local AI subsystem.
    /// </summary>
    [HttpGet("health")]
    [HttpGet("status")]
    [ProducesResponseType<LocalAiHealthDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<LocalAiHealthDto>(StatusCodes.Status503ServiceUnavailable)]
    [ProducesResponseType<LocalAiHealthDto>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetHealth(CancellationToken cancellationToken = default)
    {
        var health = await _healthService.CheckHealthAsync(cancellationToken);

        return health.Status switch
        {
            LocalAiHealthStatus.Healthy or LocalAiHealthStatus.Disabled => Ok(health),
            LocalAiHealthStatus.Misconfigured => StatusCode(StatusCodes.Status500InternalServerError, health),
            _ => StatusCode(StatusCodes.Status503ServiceUnavailable, health)
        };
    }

    /// <summary>
    /// Generates a response directly from the configured AI provider.
    /// </summary>
    [HttpPost("generate")]
    [ProducesResponseType<AiResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Generate(
        [FromBody] AiGenerateRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _aiProvider.GenerateAsync(request.ToPromptRequest(), cancellationToken);
        return Ok(response);
    }
}
