namespace EngineeringKnowledgeHub.Application.Incidents;

using System.Text;
using EngineeringKnowledgeHub.Application.Articles;
using EngineeringKnowledgeHub.Application.Common.Exceptions;
using EngineeringKnowledgeHub.Application.Common.Interfaces;
using EngineeringKnowledgeHub.Application.DTOs;
using EngineeringKnowledgeHub.Domain.Entities;
using EngineeringKnowledgeHub.Domain.Enums;

/// <summary>
/// Orchestrates the lifecycle of operational incidents, links with knowledge articles,
/// and the workflow for converting resolved incidents into draft knowledge articles.
/// </summary>
public sealed class IncidentService : IIncidentService
{
    private readonly IIncidentRepository _incidentRepository;
    private readonly IKnowledgeArticleRepository _articleRepository;
    private readonly IKnowledgeArticleService _articleService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public IncidentService(
        IIncidentRepository incidentRepository,
        IKnowledgeArticleRepository articleRepository,
        IKnowledgeArticleService articleService,
        IDateTimeProvider dateTimeProvider)
    {
        _incidentRepository = incidentRepository ?? throw new ArgumentNullException(nameof(incidentRepository));
        _articleRepository = articleRepository ?? throw new ArgumentNullException(nameof(articleRepository));
        _articleService = articleService ?? throw new ArgumentNullException(nameof(articleService));
        _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
    }

    public async Task<IncidentDto> CreateIncidentAsync(
        CreateIncidentCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command is null)
        {
            throw new ValidationException("Create incident command cannot be null.");
        }

        if (string.IsNullOrWhiteSpace(command.Title))
        {
            throw new ValidationException("Incident title cannot be null or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(command.ProblemDescription))
        {
            throw new ValidationException("Problem description cannot be null or whitespace.");
        }

        if (command.CreatedById == Guid.Empty)
        {
            throw new ValidationException("CreatedById cannot be empty.");
        }

        if (!Enum.IsDefined(typeof(IncidentSeverity), command.Severity))
        {
            throw new ValidationException("Invalid incident severity.");
        }

        var incident = new Incident(
            Guid.NewGuid(),
            command.Title,
            command.ProblemDescription,
            command.Severity,
            command.CreatedById,
            _dateTimeProvider.UtcNow,
            command.Symptoms);

        await _incidentRepository.AddAsync(incident, cancellationToken);

        return IncidentDto.FromDomain(incident);
    }

    public async Task<IncidentDto> GetIncidentByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var incident = await _incidentRepository.GetByIdAsync(id, cancellationToken);
        if (incident is null)
        {
            throw new NotFoundException("Incident", id);
        }

        return IncidentDto.FromDomain(incident);
    }

    public async Task<IReadOnlyList<IncidentDto>> GetIncidentsAsync(
        IncidentStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var incidents = await _incidentRepository.GetAllAsync(cancellationToken);

        if (status.HasValue)
        {
            incidents = incidents.Where(i => i.Status == status.Value).ToList();
        }

        return incidents.Select(IncidentDto.FromDomain).ToList();
    }

    public async Task<IReadOnlyList<IncidentDto>> GetIncidentsByArticleIdAsync(
        Guid articleId,
        CancellationToken cancellationToken = default)
    {
        if (articleId == Guid.Empty)
        {
            throw new ValidationException("ArticleId cannot be empty.");
        }

        var incidents = await _incidentRepository.GetByArticleIdAsync(articleId, cancellationToken);
        return incidents.Select(IncidentDto.FromDomain).ToList();
    }

    public async Task<IncidentDto> StartInvestigationAsync(
        Guid incidentId,
        CancellationToken cancellationToken = default)
    {
        var incident = await _incidentRepository.GetByIdAsync(incidentId, cancellationToken);
        if (incident is null)
        {
            throw new NotFoundException("Incident", incidentId);
        }

        incident.Investigate();
        await _incidentRepository.UpdateAsync(incident, cancellationToken);

        return IncidentDto.FromDomain(incident);
    }

    public async Task<IncidentDto> ResolveIncidentAsync(
        ResolveIncidentCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command is null)
        {
            throw new ValidationException("Resolve incident command cannot be null.");
        }

        if (command.IncidentId == Guid.Empty)
        {
            throw new ValidationException("IncidentId cannot be empty.");
        }

        if (command.ResolvedById == Guid.Empty)
        {
            throw new ValidationException("ResolvedById cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(command.Resolution))
        {
            throw new ValidationException("Resolution cannot be null or whitespace.");
        }

        var incident = await _incidentRepository.GetByIdAsync(command.IncidentId, cancellationToken);
        if (incident is null)
        {
            throw new NotFoundException("Incident", command.IncidentId);
        }

        incident.Resolve(command.ResolvedById, command.Resolution, command.RootCause, _dateTimeProvider.UtcNow);
        await _incidentRepository.UpdateAsync(incident, cancellationToken);

        return IncidentDto.FromDomain(incident);
    }

    public async Task<IncidentDto> CloseIncidentAsync(
        Guid incidentId,
        CancellationToken cancellationToken = default)
    {
        var incident = await _incidentRepository.GetByIdAsync(incidentId, cancellationToken);
        if (incident is null)
        {
            throw new NotFoundException("Incident", incidentId);
        }

        incident.Close();
        await _incidentRepository.UpdateAsync(incident, cancellationToken);

        return IncidentDto.FromDomain(incident);
    }

    public async Task<IncidentDto> LinkArticleToIncidentAsync(
        LinkArticleToIncidentCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command is null)
        {
            throw new ValidationException("Link article command cannot be null.");
        }

        if (command.IncidentId == Guid.Empty)
        {
            throw new ValidationException("IncidentId cannot be empty.");
        }

        if (command.ArticleId == Guid.Empty)
        {
            throw new ValidationException("ArticleId cannot be empty.");
        }

        if (!Enum.IsDefined(typeof(ArticleRelationshipType), command.RelationshipType))
        {
            throw new ValidationException("Invalid article relationship type.");
        }

        var article = await _articleRepository.GetByIdAsync(command.ArticleId, cancellationToken);
        if (article is null)
        {
            throw new NotFoundException("KnowledgeArticle", command.ArticleId);
        }

        var incident = await _incidentRepository.GetByIdAsync(command.IncidentId, cancellationToken);
        if (incident is null)
        {
            throw new NotFoundException("Incident", command.IncidentId);
        }

        incident.LinkArticle(command.ArticleId, command.RelationshipType);
        await _incidentRepository.UpdateAsync(incident, cancellationToken);

        return IncidentDto.FromDomain(incident);
    }

    public async Task<IncidentDto> UnlinkArticleFromIncidentAsync(
        Guid incidentId,
        Guid articleId,
        CancellationToken cancellationToken = default)
    {
        var incident = await _incidentRepository.GetByIdAsync(incidentId, cancellationToken);
        if (incident is null)
        {
            throw new NotFoundException("Incident", incidentId);
        }

        incident.UnlinkArticle(articleId);
        await _incidentRepository.UpdateAsync(incident, cancellationToken);

        return IncidentDto.FromDomain(incident);
    }

    public async Task<KnowledgeArticleDto> CreateKnowledgeFromIncidentAsync(
        CreateKnowledgeFromIncidentCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command is null)
        {
            throw new ValidationException("Create knowledge from incident command cannot be null.");
        }

        if (command.IncidentId == Guid.Empty)
        {
            throw new ValidationException("IncidentId cannot be empty.");
        }

        if (command.AuthorId == Guid.Empty)
        {
            throw new ValidationException("AuthorId cannot be empty.");
        }

        if (command.KnowledgeTypeId == Guid.Empty)
        {
            throw new ValidationException("KnowledgeTypeId cannot be empty.");
        }

        var incident = await _incidentRepository.GetByIdAsync(command.IncidentId, cancellationToken);
        if (incident is null)
        {
            throw new NotFoundException("Incident", command.IncidentId);
        }

        if (incident.Status is not (IncidentStatus.Resolved or IncidentStatus.Closed) ||
            string.IsNullOrWhiteSpace(incident.Resolution))
        {
            throw new ValidationException(
                "Cannot create knowledge article from an incident without a resolution. The incident must be resolved or closed.");
        }

        var title = !string.IsNullOrWhiteSpace(command.Title)
            ? command.Title.Trim()
            : incident.Title;

        if (title.Length > 200)
        {
            title = title[..200].Trim();
        }

        var summary = !string.IsNullOrWhiteSpace(command.Summary)
            ? command.Summary.Trim()
            : (!string.IsNullOrWhiteSpace(incident.Symptoms) ? incident.Symptoms : incident.ProblemDescription);

        string content;
        if (!string.IsNullOrWhiteSpace(command.CustomContent))
        {
            content = command.CustomContent.Trim();
        }
        else
        {
            var sb = new StringBuilder();
            sb.AppendLine($"# {title}");
            sb.AppendLine();
            sb.AppendLine("## Problem Description");
            sb.AppendLine(incident.ProblemDescription);

            if (!string.IsNullOrWhiteSpace(incident.Symptoms))
            {
                sb.AppendLine();
                sb.AppendLine("## Symptoms");
                sb.AppendLine(incident.Symptoms);
            }

            if (!string.IsNullOrWhiteSpace(incident.RootCause))
            {
                sb.AppendLine();
                sb.AppendLine("## Root Cause");
                sb.AppendLine(incident.RootCause);
            }

            sb.AppendLine();
            sb.AppendLine("## Resolution");
            sb.AppendLine(incident.Resolution);

            content = sb.ToString();
        }

        // Creates a new article in Draft status (respecting human peer-review lifecycle)
        var articleDto = await _articleService.CreateArticleAsync(
            new CreateKnowledgeArticleCommand(
                title,
                summary,
                content,
                command.KnowledgeTypeId,
                command.AuthorId,
                command.Slug),
            cancellationToken);

        // Link incident to the proposed knowledge article
        incident.LinkArticle(articleDto.Id, ArticleRelationshipType.Resolves);
        await _incidentRepository.UpdateAsync(incident, cancellationToken);

        return articleDto;
    }
}
