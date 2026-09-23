namespace EngineeringKnowledgeHub.Application.DTOs;

using EngineeringKnowledgeHub.Domain.Entities;
using EngineeringKnowledgeHub.Domain.Enums;

public sealed record KnowledgeArticleDto(
    Guid Id,
    string Title,
    string Slug,
    string Summary,
    string Content,
    Guid KnowledgeTypeId,
    KnowledgeStatus Status,
    VerificationStatus VerificationStatus,
    Guid AuthorId,
    Guid? ReviewerId,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    DateTimeOffset? PublishedAt,
    DateTimeOffset? LastReviewedAt,
    IReadOnlyCollection<Guid> TagIds,
    IReadOnlyCollection<Guid> ProductIds,
    IReadOnlyCollection<Guid> TechnologyIds,
    IReadOnlyCollection<Guid> SourceIds)
{
    public static KnowledgeArticleDto FromDomain(KnowledgeArticle article) =>
        new(
            article.Id,
            article.Title,
            article.Slug.Value,
            article.Summary,
            article.Content,
            article.KnowledgeTypeId,
            article.Status,
            article.VerificationStatus,
            article.AuthorId,
            article.ReviewerId,
            article.CreatedAt,
            article.UpdatedAt,
            article.PublishedAt,
            article.LastReviewedAt,
            article.TagIds,
            article.ProductIds,
            article.TechnologyIds,
            article.SourceIds);
}
