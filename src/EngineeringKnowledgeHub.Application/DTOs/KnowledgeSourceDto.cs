namespace EngineeringKnowledgeHub.Application.DTOs;

using EngineeringKnowledgeHub.Domain.Entities;
using EngineeringKnowledgeHub.Domain.Enums;

public sealed record KnowledgeSourceDto(
    Guid Id,
    string Name,
    SourceType SourceType,
    string Url,
    string Description,
    TrustLevel TrustLevel,
    bool IsApproved,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt)
{
    public static KnowledgeSourceDto FromDomain(KnowledgeSource source) =>
        new(
            source.Id,
            source.Name,
            source.SourceType,
            source.Url,
            source.Description,
            source.TrustLevel,
            source.IsApproved,
            source.CreatedAt,
            source.UpdatedAt);
}
