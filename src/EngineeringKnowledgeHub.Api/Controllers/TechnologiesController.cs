namespace EngineeringKnowledgeHub.Api.Controllers;

using EngineeringKnowledgeHub.Api.Models;
using EngineeringKnowledgeHub.Application.DTOs;
using EngineeringKnowledgeHub.Application.ProductsAndTechnologies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Endpoints for managing engineering technologies.
/// </summary>
[ApiController]
[Route("api/technologies")]
public sealed class TechnologiesController(ITechnologyService technologyService) : ControllerBase
{
    private readonly ITechnologyService _technologyService = technologyService ?? throw new ArgumentNullException(nameof(technologyService));

    /// <summary>
    /// Creates a new technology entry.
    /// </summary>
    [HttpPost]
    [ProducesResponseType<TechnologyDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateTechnologyRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _technologyService.CreateTechnologyAsync(request.ToCommand(), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Retrieves a technology by its unique identifier.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<TechnologyDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return EmptyGuidError("Technology ID");
        }

        var technology = await _technologyService.GetTechnologyByIdAsync(id, cancellationToken);
        return Ok(technology);
    }

    /// <summary>
    /// Updates the metadata of an existing technology.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<TechnologyDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateTechnologyRequest request,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return EmptyGuidError("Technology ID");
        }

        var result = await _technologyService.UpdateTechnologyAsync(request.ToCommand(id), cancellationToken);
        return Ok(result);
    }

    private static BadRequestObjectResult EmptyGuidError(string fieldName) =>
        new(new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Validation Error",
            Detail = $"{fieldName} cannot be empty."
        });
}
