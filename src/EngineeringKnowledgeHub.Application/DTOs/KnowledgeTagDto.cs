namespace EngineeringKnowledgeHub.Application.DTOs;

using EngineeringKnowledgeHub.Domain.Entities;

public sealed record KnowledgeTagDto(
    Guid Id,
    string Name)
{
    public static KnowledgeTagDto FromDomain(KnowledgeTag tag) =>
        new(tag.Id, tag.Name);
}
