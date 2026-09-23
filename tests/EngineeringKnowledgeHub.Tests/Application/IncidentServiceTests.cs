namespace EngineeringKnowledgeHub.Tests.Application;

using EngineeringKnowledgeHub.Application.Articles;
using EngineeringKnowledgeHub.Application.Common.Exceptions;
using EngineeringKnowledgeHub.Application.DTOs;
using EngineeringKnowledgeHub.Application.Incidents;
using EngineeringKnowledgeHub.Domain.Entities;
using EngineeringKnowledgeHub.Domain.Enums;
using EngineeringKnowledgeHub.Domain.Exceptions;
using EngineeringKnowledgeHub.Domain.ValueObjects;
using EngineeringKnowledgeHub.Tests.Common;
using Xunit;

public class IncidentServiceTests
{
    private readonly InMemoryIncidentRepository _incidentRepository = new();
    private readonly InMemoryKnowledgeArticleRepository _articleRepository = new();
    private readonly InMemoryKnowledgeTypeRepository _typeRepository = new();
    private readonly FakeDateTimeProvider _dateTimeProvider;
    private readonly KnowledgeArticleService _articleService;
    private readonly IncidentService _incidentService;

    private readonly Guid _creatorId = Guid.NewGuid();
    private readonly Guid _resolverId = Guid.NewGuid();
    private readonly Guid _reviewerId = Guid.NewGuid();
    private readonly Guid _knowledgeTypeId = Guid.NewGuid();
    private readonly DateTimeOffset _initialTime = new(2026, 4, 10, 8, 30, 0, TimeSpan.Zero);

    public IncidentServiceTests()
    {
        _dateTimeProvider = new FakeDateTimeProvider(_initialTime);
        _typeRepository.AddAsync(new KnowledgeType(_knowledgeTypeId, "Troubleshooting Guide", "Operational issues")).Wait();

        _articleService = new KnowledgeArticleService(
            _articleRepository,
            _typeRepository,
            new InMemoryKnowledgeTagRepository(),
            new InMemoryProductRepository(),
            new InMemoryTechnologyRepository(),
            _dateTimeProvider);

        _incidentService = new IncidentService(
            _incidentRepository,
            _articleRepository,
            _articleService,
            _dateTimeProvider);
    }

    private async Task<IncidentDto> CreateSampleIncidentAsync(
        string title = "Barrier Gate Loop Detector Failure",
        string problem = "Gate arm fails to open when vehicle arrives on loop detector A.",
        IncidentSeverity severity = IncidentSeverity.High,
        string? symptoms = "Loop frequency oscillating, relay clicking continuously.")
    {
        var command = new CreateIncidentCommand(title, problem, severity, _creatorId, symptoms);
        return await _incidentService.CreateIncidentAsync(command);
    }

    #region Creation Tests

    [Fact]
    public async Task CreateIncidentAsync_With_Valid_Parameters_Should_Initialize_Open_Incident_With_System_Timestamp()
    {
        var command = new CreateIncidentCommand(
            "Camera ANPR Image Strobe desync",
            "Images are captured without infrared strobe synchronization at night.",
            IncidentSeverity.Critical,
            _creatorId,
            "Black images saved in capture folder between 22:00 and 05:00.");

        var result = await _incidentService.CreateIncidentAsync(command);

        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("Camera ANPR Image Strobe desync", result.Title);
        Assert.Equal(command.ProblemDescription, result.ProblemDescription);
        Assert.Equal(IncidentSeverity.Critical, result.Severity);
        Assert.Equal(IncidentStatus.Open, result.Status);
        Assert.Equal(_creatorId, result.CreatedById);
        Assert.Null(result.ResolvedById);
        Assert.Equal(_initialTime, result.CreatedAt);
        Assert.Null(result.ResolvedAt);
        Assert.Empty(result.ArticleLinks);
    }

    [Fact]
    public async Task CreateIncidentAsync_With_Null_Command_Should_Throw_ValidationException()
    {
        await Assert.ThrowsAsync<ValidationException>(() =>
            _incidentService.CreateIncidentAsync(null!));
    }

    [Fact]
    public async Task CreateIncidentAsync_With_Empty_Title_Should_Throw_ValidationException()
    {
        var command = new CreateIncidentCommand("   ", "Problem", IncidentSeverity.Medium, _creatorId);

        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            _incidentService.CreateIncidentAsync(command));

        Assert.Contains("title", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateIncidentAsync_With_Empty_ProblemDescription_Should_Throw_ValidationException()
    {
        var command = new CreateIncidentCommand("Title", "   ", IncidentSeverity.Medium, _creatorId);

        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            _incidentService.CreateIncidentAsync(command));

        Assert.Contains("Problem description", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateIncidentAsync_With_Empty_CreatedById_Should_Throw_ValidationException()
    {
        var command = new CreateIncidentCommand("Title", "Problem", IncidentSeverity.Medium, Guid.Empty);

        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            _incidentService.CreateIncidentAsync(command));

        Assert.Contains("CreatedById", ex.Message);
    }

    [Fact]
    public async Task CreateIncidentAsync_With_Invalid_Severity_Should_Throw_ValidationException()
    {
        var command = new CreateIncidentCommand("Title", "Problem", (IncidentSeverity)999, _creatorId);

        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            _incidentService.CreateIncidentAsync(command));

        Assert.Contains("severity", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region Retrieval Tests

    [Fact]
    public async Task GetIncidentByIdAsync_With_Existing_Id_Should_Return_Dto()
    {
        var created = await CreateSampleIncidentAsync();

        var retrieved = await _incidentService.GetIncidentByIdAsync(created.Id);

        Assert.NotNull(retrieved);
        Assert.Equal(created.Id, retrieved.Id);
        Assert.Equal(created.Title, retrieved.Title);
    }

    [Fact]
    public async Task GetIncidentByIdAsync_With_NonExistent_Id_Should_Throw_NotFoundException()
    {
        var nonExistentId = Guid.NewGuid();

        var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            _incidentService.GetIncidentByIdAsync(nonExistentId));

        Assert.Contains(nonExistentId.ToString(), ex.Message);
    }

    [Fact]
    public async Task GetIncidentsAsync_Should_Return_All_Or_Filter_By_Status()
    {
        var incident1 = await CreateSampleIncidentAsync("Open Incident");
        var incident2 = await CreateSampleIncidentAsync("To Investigate");
        await _incidentService.StartInvestigationAsync(incident2.Id);

        var all = await _incidentService.GetIncidentsAsync();
        var investigatingOnly = await _incidentService.GetIncidentsAsync(IncidentStatus.Investigating);

        Assert.Equal(2, all.Count);
        Assert.Single(investigatingOnly);
        Assert.Equal(incident2.Id, investigatingOnly[0].Id);
    }

    #endregion

    #region Lifecycle State Transition Tests

    [Fact]
    public async Task StartInvestigationAsync_Should_Transition_Open_To_Investigating()
    {
        var created = await CreateSampleIncidentAsync();

        var updated = await _incidentService.StartInvestigationAsync(created.Id);

        Assert.Equal(IncidentStatus.Investigating, updated.Status);
    }

    [Fact]
    public async Task StartInvestigationAsync_When_Incident_Not_Found_Should_Throw_NotFoundException()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _incidentService.StartInvestigationAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task ResolveIncidentAsync_Should_Transition_To_Resolved_With_Details_And_Timestamp()
    {
        var created = await CreateSampleIncidentAsync();
        _dateTimeProvider.UtcNow = _initialTime.AddHours(2);

        var resolveCommand = new ResolveIncidentCommand(
            created.Id,
            _resolverId,
            "Replaced optocoupler on digital input board.",
            "Surge voltage damage during thunderstorm.");

        var resolved = await _incidentService.ResolveIncidentAsync(resolveCommand);

        Assert.Equal(IncidentStatus.Resolved, resolved.Status);
        Assert.Equal(_resolverId, resolved.ResolvedById);
        Assert.Equal(_initialTime.AddHours(2), resolved.ResolvedAt);
        Assert.Equal("Replaced optocoupler on digital input board.", resolved.Resolution);
        Assert.Equal("Surge voltage damage during thunderstorm.", resolved.RootCause);
    }

    [Fact]
    public async Task ResolveIncidentAsync_With_Empty_Resolution_Should_Throw_ValidationException()
    {
        var created = await CreateSampleIncidentAsync();
        var command = new ResolveIncidentCommand(created.Id, _resolverId, "   ", "Root cause");

        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            _incidentService.ResolveIncidentAsync(command));

        Assert.Contains("Resolution", ex.Message);
    }

    [Fact]
    public async Task ResolveIncidentAsync_With_Empty_ResolvedById_Should_Throw_ValidationException()
    {
        var created = await CreateSampleIncidentAsync();
        var command = new ResolveIncidentCommand(created.Id, Guid.Empty, "Fixed it");

        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            _incidentService.ResolveIncidentAsync(command));

        Assert.Contains("ResolvedById", ex.Message);
    }

    [Fact]
    public async Task CloseIncidentAsync_Should_Transition_Resolved_Incident_To_Closed()
    {
        var created = await CreateSampleIncidentAsync();
        await _incidentService.ResolveIncidentAsync(new ResolveIncidentCommand(created.Id, _resolverId, "Fixed"));

        var closed = await _incidentService.CloseIncidentAsync(created.Id);

        Assert.Equal(IncidentStatus.Closed, closed.Status);
    }

    [Fact]
    public async Task CloseIncidentAsync_When_Incident_Is_Open_Should_Throw_DomainRuleViolationException()
    {
        var created = await CreateSampleIncidentAsync();

        await Assert.ThrowsAsync<DomainRuleViolationException>(() =>
            _incidentService.CloseIncidentAsync(created.Id));
    }

    #endregion

    #region Relationship Linking Tests

    [Fact]
    public async Task LinkArticleToIncidentAsync_Should_Link_Existing_Article_To_Incident()
    {
        var incident = await CreateSampleIncidentAsync();
        var article = await _articleService.CreateArticleAsync(
            new CreateKnowledgeArticleCommand(
                "Barrier Controller Replacement Procedure",
                "Summary",
                "Content",
                _knowledgeTypeId,
                _creatorId));

        var updated = await _incidentService.LinkArticleToIncidentAsync(
            new LinkArticleToIncidentCommand(incident.Id, article.Id, ArticleRelationshipType.Resolves));

        Assert.Single(updated.ArticleLinks);
        Assert.Equal(article.Id, updated.ArticleLinks.First().ArticleId);
        Assert.Equal(ArticleRelationshipType.Resolves, updated.ArticleLinks.First().RelationshipType);

        var linkedIncidents = await _incidentService.GetIncidentsByArticleIdAsync(article.Id);
        Assert.Single(linkedIncidents);
        Assert.Equal(incident.Id, linkedIncidents[0].Id);
    }

    [Fact]
    public async Task LinkArticleToIncidentAsync_When_Article_Does_Not_Exist_Should_Throw_NotFoundException()
    {
        var incident = await CreateSampleIncidentAsync();
        var nonExistentArticleId = Guid.NewGuid();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _incidentService.LinkArticleToIncidentAsync(
                new LinkArticleToIncidentCommand(incident.Id, nonExistentArticleId, ArticleRelationshipType.Supports)));
    }

    [Fact]
    public async Task UnlinkArticleFromIncidentAsync_Should_Remove_Relationship()
    {
        var incident = await CreateSampleIncidentAsync();
        var article = await _articleService.CreateArticleAsync(
            new CreateKnowledgeArticleCommand("Test Article", "Summary", "Content", _knowledgeTypeId, _creatorId));

        await _incidentService.LinkArticleToIncidentAsync(
            new LinkArticleToIncidentCommand(incident.Id, article.Id, ArticleRelationshipType.Supports));

        var unlinked = await _incidentService.UnlinkArticleFromIncidentAsync(incident.Id, article.Id);

        Assert.Empty(unlinked.ArticleLinks);
    }

    #endregion

    #region Create Knowledge From Incident Workflow Tests

    [Fact]
    public async Task CreateKnowledgeFromIncidentAsync_When_Incident_Is_Open_Should_Throw_ValidationException()
    {
        var incident = await CreateSampleIncidentAsync();

        var command = new CreateKnowledgeFromIncidentCommand(
            incident.Id,
            _creatorId,
            _knowledgeTypeId);

        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            _incidentService.CreateKnowledgeFromIncidentAsync(command));

        Assert.Contains("resolved or closed", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateKnowledgeFromIncidentAsync_When_Resolved_Should_Create_Draft_Article_And_Preserve_Review_Lifecycle()
    {
        // 1. Create and resolve incident
        var incident = await CreateSampleIncidentAsync(
            "RS485 Comm Error on Lane 4",
            "Lane 4 barrier controller dropped communication intermittently.",
            IncidentSeverity.High,
            "CRC errors logged in serial driver.");

        await _incidentService.ResolveIncidentAsync(
            new ResolveIncidentCommand(
                incident.Id,
                _resolverId,
                "Terminated RS485 bus with 120-ohm resistor at the end of line.",
                "Bus reflection caused signal degradation over 300m cable."));

        // 2. Propose knowledge from resolved incident
        var command = new CreateKnowledgeFromIncidentCommand(
            incident.Id,
            _creatorId,
            _knowledgeTypeId);

        var articleDto = await _incidentService.CreateKnowledgeFromIncidentAsync(command);

        // 3. Verify article was created strictly in DRAFT status (human review is required, NOT auto-published!)
        Assert.NotNull(articleDto);
        Assert.NotEqual(Guid.Empty, articleDto.Id);
        Assert.Equal(incident.Title, articleDto.Title);
        Assert.Equal(KnowledgeStatus.Draft, articleDto.Status);
        Assert.Equal(VerificationStatus.Unverified, articleDto.VerificationStatus);
        Assert.Equal(_creatorId, articleDto.AuthorId);
        Assert.Null(articleDto.PublishedAt);
        Assert.Contains("## Problem Description", articleDto.Content);
        Assert.Contains("## Symptoms", articleDto.Content);
        Assert.Contains("## Root Cause", articleDto.Content);
        Assert.Contains("## Resolution", articleDto.Content);

        // 4. Verify incident was linked to this new article
        var updatedIncident = await _incidentService.GetIncidentByIdAsync(incident.Id);
        Assert.Single(updatedIncident.ArticleLinks);
        Assert.Equal(articleDto.Id, updatedIncident.ArticleLinks.First().ArticleId);
        Assert.Equal(ArticleRelationshipType.Resolves, updatedIncident.ArticleLinks.First().RelationshipType);

        // 5. Verify the created draft article proceeds through the human review lifecycle
        await _articleService.SubmitForReviewAsync(articleDto.Id);
        var pendingArticle = await _articleService.GetArticleByIdAsync(articleDto.Id);
        Assert.Equal(KnowledgeStatus.PendingReview, pendingArticle.Status);

        var approvedArticle = await _articleService.ApproveArticleAsync(
            new ApproveKnowledgeArticleCommand(articleDto.Id, _reviewerId));
        Assert.Equal(KnowledgeStatus.Published, approvedArticle.Status);
        Assert.Equal(VerificationStatus.Verified, approvedArticle.VerificationStatus);
        Assert.Equal(_reviewerId, approvedArticle.ReviewerId);
        Assert.NotNull(approvedArticle.PublishedAt);
    }

    [Fact]
    public async Task CreateKnowledgeFromIncidentAsync_With_Custom_Content_And_Slug_Should_Use_Provided_Values()
    {
        var incident = await CreateSampleIncidentAsync("Incident With Custom Knowledge");
        await _incidentService.ResolveIncidentAsync(new ResolveIncidentCommand(incident.Id, _resolverId, "Resolved cleanly"));

        var command = new CreateKnowledgeFromIncidentCommand(
            incident.Id,
            _creatorId,
            _knowledgeTypeId,
            Title: "Custom Article Title",
            Summary: "Custom summary for the article",
            CustomContent: "Custom detailed Markdown content",
            Slug: "custom-article-slug");

        var article = await _incidentService.CreateKnowledgeFromIncidentAsync(command);

        Assert.Equal("Custom Article Title", article.Title);
        Assert.Equal("custom-article-slug", article.Slug);
        Assert.Equal("Custom summary for the article", article.Summary);
        Assert.Equal("Custom detailed Markdown content", article.Content);
        Assert.Equal(KnowledgeStatus.Draft, article.Status);
    }

    #endregion
}
