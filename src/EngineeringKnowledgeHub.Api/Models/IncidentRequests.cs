namespace EngineeringKnowledgeHub.Api.Models;

using System.ComponentModel.DataAnnotations;
using EngineeringKnowledgeHub.Application.Incidents;
using EngineeringKnowledgeHub.Domain.Enums;

/// <summary>
/// HTTP request contract for reporting a new operational incident.
/// </summary>
public sealed record CreateIncidentRequest(
    [Required(AllowEmptyStrings = false, ErrorMessage = "Incident title is required.")]
    string Title,
    [Required(AllowEmptyStrings = false, ErrorMessage = "Problem description is required.")]
    string ProblemDescription,
    IncidentSeverity Severity,
    Guid CreatedById,
    string? Symptoms = null) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Title))
        {
            yield return new ValidationResult("Incident title is required.", [nameof(Title)]);
        }

        if (string.IsNullOrWhiteSpace(ProblemDescription))
        {
            yield return new ValidationResult("Problem description is required.", [nameof(ProblemDescription)]);
        }
    }

    public CreateIncidentCommand ToCommand() =>
        new(Title, ProblemDescription, Severity, CreatedById, Symptoms);
}

/// <summary>
/// HTTP request contract for resolving an ongoing incident.
/// </summary>
public sealed record ResolveIncidentRequest(
    Guid ResolvedById,
    [Required(AllowEmptyStrings = false, ErrorMessage = "Resolution is required.")]
    string Resolution,
    string? RootCause = null) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Resolution))
        {
            yield return new ValidationResult("Resolution is required.", [nameof(Resolution)]);
        }
    }

    public ResolveIncidentCommand ToCommand(Guid incidentId) =>
        new(incidentId, ResolvedById, Resolution, RootCause);
}

/// <summary>
/// HTTP request contract for linking a knowledge article to an incident.
/// </summary>
public sealed record LinkArticleRequest(
    Guid ArticleId,
    ArticleRelationshipType RelationshipType)
{
    public LinkArticleToIncidentCommand ToCommand(Guid incidentId) =>
        new(incidentId, ArticleId, RelationshipType);
}

/// <summary>
/// HTTP request contract for proposing a draft knowledge article from a resolved incident.
/// </summary>
public sealed record CreateKnowledgeFromIncidentRequest(
    Guid AuthorId,
    Guid KnowledgeTypeId,
    string? Title = null,
    string? Summary = null,
    string? CustomContent = null,
    string? Slug = null)
{
    public CreateKnowledgeFromIncidentCommand ToCommand(Guid incidentId) =>
        new(incidentId, AuthorId, KnowledgeTypeId, Title, Summary, CustomContent, Slug);
}
