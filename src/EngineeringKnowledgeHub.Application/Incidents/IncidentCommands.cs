namespace EngineeringKnowledgeHub.Application.Incidents;

using EngineeringKnowledgeHub.Domain.Enums;

public sealed record CreateIncidentCommand(
    string Title,
    string ProblemDescription,
    IncidentSeverity Severity,
    Guid CreatedById,
    string? Symptoms = null);

public sealed record ResolveIncidentCommand(
    Guid IncidentId,
    Guid ResolvedById,
    string Resolution,
    string? RootCause = null);

public sealed record LinkArticleToIncidentCommand(
    Guid IncidentId,
    Guid ArticleId,
    ArticleRelationshipType RelationshipType);

public sealed record CreateKnowledgeFromIncidentCommand(
    Guid IncidentId,
    Guid AuthorId,
    Guid KnowledgeTypeId,
    string? Title = null,
    string? Summary = null,
    string? CustomContent = null,
    string? Slug = null);
