namespace EngineeringKnowledgeHub.Api.Models;

using System.ComponentModel.DataAnnotations;
using EngineeringKnowledgeHub.Application.Taxonomy;

/// <summary>
/// HTTP request contract for creating a knowledge taxonomy category.
/// </summary>
public sealed record CreateCategoryRequest(
    [Required(AllowEmptyStrings = false, ErrorMessage = "Category name is required.")]
    string Name,
    string? Description = null,
    Guid? ParentCategoryId = null) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            yield return new ValidationResult("Category name is required.", [nameof(Name)]);
        }

        if (ParentCategoryId.HasValue && ParentCategoryId.Value == Guid.Empty)
        {
            yield return new ValidationResult("Parent category ID cannot be empty.", [nameof(ParentCategoryId)]);
        }
    }

    public CreateKnowledgeCategoryCommand ToCommand() =>
        new(Name, Description, ParentCategoryId);
}

/// <summary>
/// HTTP request contract for updating an existing knowledge taxonomy category.
/// </summary>
public sealed record UpdateCategoryRequest(
    [Required(AllowEmptyStrings = false, ErrorMessage = "Category name is required.")]
    string Name,
    string? Description = null,
    Guid? ParentCategoryId = null) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            yield return new ValidationResult("Category name is required.", [nameof(Name)]);
        }

        if (ParentCategoryId.HasValue && ParentCategoryId.Value == Guid.Empty)
        {
            yield return new ValidationResult("Parent category ID cannot be empty.", [nameof(ParentCategoryId)]);
        }
    }

    public UpdateKnowledgeCategoryCommand ToCommand(Guid id) =>
        new(id, Name, Description, ParentCategoryId);
}

/// <summary>
/// HTTP request contract for creating a knowledge taxonomy type.
/// </summary>
public sealed record CreateKnowledgeTypeRequest(
    [Required(AllowEmptyStrings = false, ErrorMessage = "Knowledge type name is required.")]
    string Name,
    string? Description = null) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            yield return new ValidationResult("Knowledge type name is required.", [nameof(Name)]);
        }
    }

    public CreateKnowledgeTypeCommand ToCommand() =>
        new(Name, Description);
}

/// <summary>
/// HTTP request contract for updating an existing knowledge taxonomy type.
/// </summary>
public sealed record UpdateKnowledgeTypeRequest(
    [Required(AllowEmptyStrings = false, ErrorMessage = "Knowledge type name is required.")]
    string Name,
    string? Description = null) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            yield return new ValidationResult("Knowledge type name is required.", [nameof(Name)]);
        }
    }

    public UpdateKnowledgeTypeCommand ToCommand(Guid id) =>
        new(id, Name, Description);
}

/// <summary>
/// HTTP request contract for creating a knowledge taxonomy tag.
/// </summary>
public sealed record CreateKnowledgeTagRequest(
    [Required(AllowEmptyStrings = false, ErrorMessage = "Tag name is required.")]
    string Name) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            yield return new ValidationResult("Tag name is required.", [nameof(Name)]);
        }
    }

    public CreateKnowledgeTagCommand ToCommand() =>
        new(Name);
}

/// <summary>
/// HTTP request contract for updating an existing knowledge taxonomy tag.
/// </summary>
public sealed record UpdateKnowledgeTagRequest(
    [Required(AllowEmptyStrings = false, ErrorMessage = "Tag name is required.")]
    string Name) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            yield return new ValidationResult("Tag name is required.", [nameof(Name)]);
        }
    }

    public UpdateKnowledgeTagCommand ToCommand(Guid id) =>
        new(id, Name);
}
