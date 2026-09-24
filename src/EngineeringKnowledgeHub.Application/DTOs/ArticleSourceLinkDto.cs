namespace EngineeringKnowledgeHub.Application.DTOs;

using EngineeringKnowledgeHub.Domain.Entities;

public sealed record ArticleSourceLinkDto(
    Guid SourceId,
    string? ReferenceText)
{
    public static ArticleSourceLinkDto FromDomain(ArticleSourceLink link) =>
        new(link.SourceId, link.ReferenceText);
}
