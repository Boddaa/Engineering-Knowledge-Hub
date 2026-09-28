namespace EngineeringKnowledgeHub.Api.Models;

using System.ComponentModel.DataAnnotations;
using EngineeringKnowledgeHub.Application.Sources;
using EngineeringKnowledgeHub.Domain.Enums;

/// <summary>
/// HTTP request contract for registering a new knowledge source.
/// </summary>
public sealed record CreateKnowledgeSourceRequest(
    [Required(AllowEmptyStrings = false, ErrorMessage = "Source name is required.")]
    string Name,
    SourceType SourceType,
    string? Url = null,
    string? Description = null,
    TrustLevel TrustLevel = TrustLevel.Untrusted) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            yield return new ValidationResult("Source name is required.", [nameof(Name)]);
        }
    }

    public CreateKnowledgeSourceCommand ToCommand() =>
        new(Name, SourceType, Url, Description, TrustLevel);
}

/// <summary>
/// HTTP request contract for updating an existing knowledge source metadata.
/// </summary>
public sealed record UpdateKnowledgeSourceRequest(
    [Required(AllowEmptyStrings = false, ErrorMessage = "Source name is required.")]
    string Name,
    string? Url = null,
    string? Description = null) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            yield return new ValidationResult("Source name is required.", [nameof(Name)]);
        }
    }

    public UpdateKnowledgeSourceCommand ToCommand(Guid id) =>
        new(id, Name, Url, Description);
}

/// <summary>
/// HTTP request contract for updating the trust level of a knowledge source.
/// </summary>
public sealed record UpdateTrustLevelRequest(TrustLevel TrustLevel)
{
    public UpdateTrustLevelCommand ToCommand(Guid id) =>
        new(id, TrustLevel);
}

/// <summary>
/// HTTP request contract for attaching a source to a knowledge article.
/// </summary>
public sealed record AttachSourceRequest(string? ReferenceText = null)
{
    public AttachArticleSourceCommand ToCommand(Guid articleId, Guid sourceId) =>
        new(articleId, sourceId, ReferenceText);
}
