namespace EngineeringKnowledgeHub.Application.DTOs;

using EngineeringKnowledgeHub.Domain.Entities;

public sealed record KnowledgeTypeDto(
    Guid Id,
    string Name,
    string Description)
{
    public static KnowledgeTypeDto FromDomain(KnowledgeType type) =>
        new(type.Id, type.Name, type.Description);
}
