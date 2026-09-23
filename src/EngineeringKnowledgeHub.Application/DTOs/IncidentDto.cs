namespace EngineeringKnowledgeHub.Application.DTOs;

using EngineeringKnowledgeHub.Domain.Entities;
using EngineeringKnowledgeHub.Domain.Enums;

/// <summary>
/// Relationship link between an incident and a knowledge article.
/// </summary>
public sealed record IncidentArticleLinkDto(Guid ArticleId, ArticleRelationshipType RelationshipType);

/// <summary>
/// Data transfer object projecting operational incident state.
/// </summary>
public sealed record IncidentDto(
    Guid Id,
    string Title,
    string ProblemDescription,
    string Symptoms,
    string RootCause,
    string Resolution,
    IncidentSeverity Severity,
    IncidentStatus Status,
    Guid CreatedById,
    Guid? ResolvedById,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ResolvedAt,
    IReadOnlyCollection<IncidentArticleLinkDto> ArticleLinks)
{
    public static IncidentDto FromDomain(Incident incident) =>
        new(
            incident.Id,
            incident.Title,
            incident.ProblemDescription,
            incident.Symptoms,
            incident.RootCause,
            incident.Resolution,
            incident.Severity,
            incident.Status,
            incident.CreatedById,
            incident.ResolvedById,
            incident.CreatedAt,
            incident.ResolvedAt,
            incident.ArticleLinks.Select(l => new IncidentArticleLinkDto(l.ArticleId, l.RelationshipType)).ToList());
}
