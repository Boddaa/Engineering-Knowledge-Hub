namespace EngineeringKnowledgeHub.Application.Articles;

public sealed record CreateKnowledgeArticleCommand(
    string Title,
    string Summary,
    string Content,
    Guid KnowledgeTypeId,
    Guid AuthorId,
    string? Slug = null);

public sealed record UpdateKnowledgeArticleCommand(
    Guid Id,
    string Title,
    string Summary,
    string Content,
    string? Slug = null);

public sealed record ApproveKnowledgeArticleCommand(
    Guid ArticleId,
    Guid ReviewerId,
    string? ChangeSummary = null,
    bool CreateVersion = false);

public sealed record RejectKnowledgeArticleCommand(
    Guid ArticleId,
    Guid ReviewerId);
