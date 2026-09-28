namespace EngineeringKnowledgeHub.Api.Models;

using System.ComponentModel.DataAnnotations;
using EngineeringKnowledgeHub.Application.ProductsAndTechnologies;

/// <summary>
/// HTTP request contract for registering a new engineering product.
/// </summary>
public sealed record CreateProductRequest(
    [Required(AllowEmptyStrings = false, ErrorMessage = "Product name is required.")]
    string Name,
    [Required(AllowEmptyStrings = false, ErrorMessage = "Product code is required.")]
    string Code,
    string? Description = null,
    bool IsActive = true) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            yield return new ValidationResult("Product name is required.", [nameof(Name)]);
        }

        if (string.IsNullOrWhiteSpace(Code))
        {
            yield return new ValidationResult("Product code is required.", [nameof(Code)]);
        }
    }

    public CreateProductCommand ToCommand() =>
        new(Name, Code, Description, IsActive);
}

/// <summary>
/// HTTP request contract for updating an existing engineering product.
/// </summary>
public sealed record UpdateProductRequest(
    [Required(AllowEmptyStrings = false, ErrorMessage = "Product name is required.")]
    string Name,
    [Required(AllowEmptyStrings = false, ErrorMessage = "Product code is required.")]
    string Code,
    string? Description = null) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            yield return new ValidationResult("Product name is required.", [nameof(Name)]);
        }

        if (string.IsNullOrWhiteSpace(Code))
        {
            yield return new ValidationResult("Product code is required.", [nameof(Code)]);
        }
    }

    public UpdateProductCommand ToCommand(Guid id) =>
        new(id, Name, Code, Description);
}
