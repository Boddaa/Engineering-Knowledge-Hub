namespace EngineeringKnowledgeHub.Api.Models;

using EngineeringKnowledgeHub.Application.Articles;

/// <summary>
/// HTTP request contract for creating a new knowledge article.
/// </summary>
public sealed record CreateArticleRequest(
    string Title,
    string Summary,
    string Content,
    Guid KnowledgeTypeId,
    Guid AuthorId,
    string? Slug = null)
{
    public CreateKnowledgeArticleCommand ToCommand() =>
        new(Title, Summary, Content, KnowledgeTypeId, AuthorId, Slug);
}

/// <summary>
/// HTTP request contract for updating an existing knowledge article.
/// </summary>
public sealed record UpdateArticleRequest(
    string Title,
    string Summary,
    string Content,
    string? Slug = null);

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
