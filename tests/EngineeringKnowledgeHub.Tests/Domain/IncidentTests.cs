namespace EngineeringKnowledgeHub.Tests.Domain;

using EngineeringKnowledgeHub.Domain.Entities;
using EngineeringKnowledgeHub.Domain.Enums;
using EngineeringKnowledgeHub.Domain.Exceptions;
using Xunit;

public class IncidentTests
{
    private readonly Guid _createdById = Guid.NewGuid();
    private readonly Guid _resolvedById = Guid.NewGuid();
    private readonly DateTimeOffset _now = DateTimeOffset.UtcNow;

    private Incident CreateValidIncident()
    {
        return new Incident(
            Guid.NewGuid(),
            "Camera Barrier Controller Timeout",
            "Barrier controller did not respond within 500ms on lane 3.",
            IncidentSeverity.High,
            _createdById,
            _now,
            "Timeout logs in Event Viewer, gate arm remained closed.");
    }

    [Fact]
    public void Constructor_With_Valid_Parameters_Should_Initialize_In_Open_State()
    {
        var incident = CreateValidIncident();

        Assert.Equal(IncidentStatus.Open, incident.Status);
        Assert.Equal(IncidentSeverity.High, incident.Severity);
        Assert.Equal(_createdById, incident.CreatedById);
        Assert.Null(incident.ResolvedById);
        Assert.Null(incident.ResolvedAt);
        Assert.Empty(incident.Resolution);
        Assert.Empty(incident.ArticleLinks);
    }

    [Fact]
    public void Investigate_Should_Transition_To_Investigating()
    {
        var incident = CreateValidIncident();

        incident.Investigate();

        Assert.Equal(IncidentStatus.Investigating, incident.Status);
    }

    [Fact]
    public void Resolve_Should_Set_Resolved_Status_And_Resolution_Details()
    {
        var incident = CreateValidIncident();
        var resolvedTime = _now.AddHours(3);

        incident.Resolve(_resolvedById, "Replaced faulty RS485 converter and updated baud rate to 115200.", "Hardware defect in serial module.", resolvedTime);

        Assert.Equal(IncidentStatus.Resolved, incident.Status);
        Assert.Equal(_resolvedById, incident.ResolvedById);
        Assert.Equal(resolvedTime, incident.ResolvedAt);
        Assert.Contains("RS485 converter", incident.Resolution);
        Assert.Contains("Hardware defect", incident.RootCause);
    }

    [Fact]
    public void Resolve_With_Empty_Resolution_Should_Throw_DomainRuleViolationException()
    {
        var incident = CreateValidIncident();

        Assert.Throws<DomainRuleViolationException>(() =>
            incident.Resolve(_resolvedById, "   ", "Root cause", _now.AddHours(1)));
    }

    [Fact]
    public void Close_When_Resolved_Should_Transition_To_Closed()
    {
        var incident = CreateValidIncident();
        incident.Resolve(_resolvedById, "Fixed", "Defect", _now.AddHours(1));

        incident.Close();

        Assert.Equal(IncidentStatus.Closed, incident.Status);
    }

    [Fact]
    public void Close_When_Open_Should_Throw_DomainRuleViolationException()
    {
        var incident = CreateValidIncident();

        Assert.Throws<DomainRuleViolationException>(() => incident.Close());
    }

    [Fact]
    public void LinkArticle_Should_Add_Relationship_To_Knowledge()
    {
        var incident = CreateValidIncident();
        var articleId = Guid.NewGuid();

        incident.LinkArticle(articleId, ArticleRelationshipType.Resolves);

        Assert.Single(incident.ArticleLinks);
        var link = incident.ArticleLinks.First();
        Assert.Equal(articleId, link.ArticleId);
        Assert.Equal(ArticleRelationshipType.Resolves, link.RelationshipType);
    }

    [Fact]
    public void UnlinkArticle_Should_Remove_Relationship()
    {
        var incident = CreateValidIncident();
        var articleId = Guid.NewGuid();
        incident.LinkArticle(articleId, ArticleRelationshipType.Supports);

        incident.UnlinkArticle(articleId);

        Assert.Empty(incident.ArticleLinks);
    }
}
