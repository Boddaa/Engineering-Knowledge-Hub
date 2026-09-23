namespace EngineeringKnowledgeHub.Domain.Entities;

using EngineeringKnowledgeHub.Domain.Enums;
using EngineeringKnowledgeHub.Domain.Exceptions;

/// <summary>
/// Link between an incident and related knowledge articles.
/// </summary>
public sealed record IncidentArticleLink(Guid ArticleId, ArticleRelationshipType RelationshipType);

/// <summary>
/// Operational issue or troubleshooting case encountered by an engineer.
/// Represents a primary source for converted technical knowledge.
/// </summary>
public sealed class Incident
{
    private readonly List<IncidentArticleLink> _articleLinks = [];

    public Guid Id { get; private set; }
    public string Title { get; private set; }
    public string ProblemDescription { get; private set; }
    public string Symptoms { get; private set; }
    public string RootCause { get; private set; }
    public string Resolution { get; private set; }
    public IncidentSeverity Severity { get; private set; }
    public IncidentStatus Status { get; private set; }
    public Guid CreatedById { get; private set; }
    public Guid? ResolvedById { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? ResolvedAt { get; private set; }

    public IReadOnlyCollection<IncidentArticleLink> ArticleLinks => _articleLinks.AsReadOnly();

    public Incident(
        Guid id,
        string title,
        string problemDescription,
        IncidentSeverity severity,
        Guid createdById,
        DateTimeOffset createdAt,
        string? symptoms = null)
    {
        if (id == Guid.Empty) throw new DomainRuleViolationException("Incident Id cannot be empty.");
        if (createdById == Guid.Empty) throw new DomainRuleViolationException("CreatedById cannot be empty.");
        if (string.IsNullOrWhiteSpace(title)) throw new DomainRuleViolationException("Incident title cannot be null or whitespace.");
        if (string.IsNullOrWhiteSpace(problemDescription)) throw new DomainRuleViolationException("Problem description cannot be null or whitespace.");

        Id = id;
        Title = title.Trim();
        ProblemDescription = problemDescription.Trim();
        Symptoms = symptoms?.Trim() ?? string.Empty;
        RootCause = string.Empty;
        Resolution = string.Empty;
        Severity = severity;
        Status = IncidentStatus.Open;
        CreatedById = createdById;
        CreatedAt = createdAt;
    }

    public void Investigate()
    {
        if (Status is IncidentStatus.Resolved or IncidentStatus.Closed)
        {
            throw new DomainRuleViolationException($"Cannot investigate an incident that is already {Status}.");
        }

        Status = IncidentStatus.Investigating;
    }

    public void Resolve(Guid resolvedById, string resolution, string? rootCause, DateTimeOffset resolvedAt)
    {
        if (resolvedById == Guid.Empty)
        {
            throw new DomainRuleViolationException("ResolvedById cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(resolution))
        {
            throw new DomainRuleViolationException("Resolution description cannot be null or whitespace when resolving an incident.");
        }

        if (Status == IncidentStatus.Closed)
        {
            throw new DomainRuleViolationException("Cannot resolve an already closed incident.");
        }

        ResolvedById = resolvedById;
        Resolution = resolution.Trim();
        RootCause = rootCause?.Trim() ?? string.Empty;
        ResolvedAt = resolvedAt;
        Status = IncidentStatus.Resolved;
    }

    public void Close()
    {
        if (Status != IncidentStatus.Resolved)
        {
            throw new DomainRuleViolationException("Only resolved incidents can be closed.");
        }

        Status = IncidentStatus.Closed;
    }

    public void LinkArticle(Guid articleId, ArticleRelationshipType relationshipType)
    {
        if (articleId == Guid.Empty)
        {
            throw new DomainRuleViolationException("Article Id cannot be empty.");
        }

        _articleLinks.RemoveAll(l => l.ArticleId == articleId);
        _articleLinks.Add(new IncidentArticleLink(articleId, relationshipType));
    }

    public void UnlinkArticle(Guid articleId)
    {
        _articleLinks.RemoveAll(l => l.ArticleId == articleId);
    }
}
