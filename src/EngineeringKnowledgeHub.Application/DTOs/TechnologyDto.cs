namespace EngineeringKnowledgeHub.Application.DTOs;

using EngineeringKnowledgeHub.Domain.Entities;

public sealed record TechnologyDto(
    Guid Id,
    string Name,
    string Description)
{
    public static TechnologyDto FromDomain(Technology technology) =>
        new(technology.Id, technology.Name, technology.Description);
}
