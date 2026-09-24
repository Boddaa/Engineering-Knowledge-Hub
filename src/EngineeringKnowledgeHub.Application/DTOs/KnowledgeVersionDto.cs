namespace EngineeringKnowledgeHub.Application.DTOs;

using EngineeringKnowledgeHub.Domain.Entities;

public sealed record KnowledgeVersionDto(
    Guid Id,
    Guid ArticleId,
    int VersionNumber,
    string Title,
    string Content,
    Guid ChangedById,
    string ChangeSummary,
    DateTimeOffset CreatedAt)
{
    public static KnowledgeVersionDto FromDomain(KnowledgeVersion version) =>
        new(
            version.Id,
            version.ArticleId,
            version.VersionNumber,
            version.Title,
            version.Content,
            version.ChangedById,
            version.ChangeSummary,
            version.CreatedAt);
}
