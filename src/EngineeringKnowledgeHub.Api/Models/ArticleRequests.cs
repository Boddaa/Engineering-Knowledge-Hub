namespace EngineeringKnowledgeHub.Api.Models;

using System.ComponentModel.DataAnnotations;
using EngineeringKnowledgeHub.Application.Articles;

/// <summary>
/// HTTP request contract for creating a new knowledge article.
/// </summary>
public sealed record CreateArticleRequest(
    [Required(AllowEmptyStrings = false, ErrorMessage = "Article title is required.")]
    string Title,
    [Required(AllowEmptyStrings = false, ErrorMessage = "Article summary is required.")]
    string Summary,
    [Required(AllowEmptyStrings = false, ErrorMessage = "Article content is required.")]
    string Content,
    Guid KnowledgeTypeId,
    Guid AuthorId,
    string? Slug = null) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Title))
        {
            yield return new ValidationResult("Article title is required.", [nameof(Title)]);
        }

        if (string.IsNullOrWhiteSpace(Summary))
        {
            yield return new ValidationResult("Article summary is required.", [nameof(Summary)]);
        }

        if (string.IsNullOrWhiteSpace(Content))
        {
            yield return new ValidationResult("Article content is required.", [nameof(Content)]);
        }
    }

    public CreateKnowledgeArticleCommand ToCommand() =>
        new(Title, Summary, Content, KnowledgeTypeId, AuthorId, Slug);
}

/// <summary>
/// HTTP request contract for updating an existing knowledge article.
/// </summary>
public sealed record UpdateArticleRequest(
    [Required(AllowEmptyStrings = false, ErrorMessage = "Article title is required.")]
    string Title,
    [Required(AllowEmptyStrings = false, ErrorMessage = "Article summary is required.")]
    string Summary,
    [Required(AllowEmptyStrings = false, ErrorMessage = "Article content is required.")]
    string Content,
    string? Slug = null) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Title))
        {
            yield return new ValidationResult("Article title is required.", [nameof(Title)]);
        }

        if (string.IsNullOrWhiteSpace(Summary))
        {
            yield return new ValidationResult("Article summary is required.", [nameof(Summary)]);
        }

        if (string.IsNullOrWhiteSpace(Content))
        {
            yield return new ValidationResult("Article content is required.", [nameof(Content)]);
        }
    }
}

/// <summary>
/// HTTP request contract for submitting an article for peer review.
/// </summary>
public sealed record SubmitArticleReviewRequest(Guid? SubmitterId = null);

/// <summary>
/// HTTP request contract for approving an article publication.
/// </summary>
public sealed record ApproveArticleRequest(
    Guid ReviewerId,
    string? ChangeSummary = null);

/// <summary>
/// HTTP request contract for rejecting an article under review.
/// </summary>
public sealed record RejectArticleRequest(Guid ReviewerId);

/// <summary>
/// HTTP request contract for deprecating an active knowledge article.
/// </summary>
public sealed record DeprecateArticleRequest(Guid? ActorId = null);

/// <summary>
/// HTTP request contract for archiving an article.
/// </summary>
public sealed record ArchiveArticleRequest(Guid? ActorId = null);
