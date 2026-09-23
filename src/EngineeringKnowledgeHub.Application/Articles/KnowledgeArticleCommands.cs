namespace EngineeringKnowledgeHub.Application.Articles;

public sealed record CreateKnowledgeArticleCommand(
    string Title,
    string Summary,
    string Content,
    Guid KnowledgeTypeId,
    Guid AuthorId,
    DateTimeOffset CreatedAt,
    string? Slug = null,
    Guid? Id = null);

public sealed record UpdateKnowledgeArticleCommand(
    Guid Id,
    string Title,
    string Summary,
    string Content,
    DateTimeOffset UpdatedAt,
    string? Slug = null);

public sealed record ApproveKnowledgeArticleCommand(
    Guid ArticleId,
    Guid ReviewerId,
    DateTimeOffset ReviewedAt);

public sealed record RejectKnowledgeArticleCommand(
    Guid ArticleId,
    Guid ReviewerId,
    DateTimeOffset ReviewedAt);
