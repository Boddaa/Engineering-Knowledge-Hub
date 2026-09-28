namespace EngineeringKnowledgeHub.Api.Models;

using System.ComponentModel.DataAnnotations;
using EngineeringKnowledgeHub.Application.ProductsAndTechnologies;

/// <summary>
/// HTTP request contract for creating a new technology entry.
/// </summary>
public sealed record CreateTechnologyRequest(
    [Required(AllowEmptyStrings = false, ErrorMessage = "Technology name is required.")]
    string Name,
    string? Description = null) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            yield return new ValidationResult("Technology name is required.", [nameof(Name)]);
        }
    }

    public CreateTechnologyCommand ToCommand() =>
        new(Name, Description);
}

/// <summary>
/// HTTP request contract for updating an existing technology entry.
/// </summary>
public sealed record UpdateTechnologyRequest(
    [Required(AllowEmptyStrings = false, ErrorMessage = "Technology name is required.")]
    string Name,
    string? Description = null) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            yield return new ValidationResult("Technology name is required.", [nameof(Name)]);
        }
    }

    public UpdateTechnologyCommand ToCommand(Guid id) =>
        new(id, Name, Description);
}
