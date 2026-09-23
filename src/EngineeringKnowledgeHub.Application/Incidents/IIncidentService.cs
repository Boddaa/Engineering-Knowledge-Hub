namespace EngineeringKnowledgeHub.Application.Incidents;

using EngineeringKnowledgeHub.Application.DTOs;
using EngineeringKnowledgeHub.Domain.Enums;

/// <summary>
/// Application service contract orchestrating incident lifecycle, relationships,
/// and knowledge proposal workflows.
/// </summary>
public interface IIncidentService
{
    Task<IncidentDto> CreateIncidentAsync(CreateIncidentCommand command, CancellationToken cancellationToken = default);
    Task<IncidentDto> GetIncidentByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<IncidentDto>> GetIncidentsAsync(IncidentStatus? status = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<IncidentDto>> GetIncidentsByArticleIdAsync(Guid articleId, CancellationToken cancellationToken = default);
    Task<IncidentDto> StartInvestigationAsync(Guid incidentId, CancellationToken cancellationToken = default);
    Task<IncidentDto> ResolveIncidentAsync(ResolveIncidentCommand command, CancellationToken cancellationToken = default);
    Task<IncidentDto> CloseIncidentAsync(Guid incidentId, CancellationToken cancellationToken = default);
    Task<IncidentDto> LinkArticleToIncidentAsync(LinkArticleToIncidentCommand command, CancellationToken cancellationToken = default);
    Task<IncidentDto> UnlinkArticleFromIncidentAsync(Guid incidentId, Guid articleId, CancellationToken cancellationToken = default);
    Task<KnowledgeArticleDto> CreateKnowledgeFromIncidentAsync(CreateKnowledgeFromIncidentCommand command, CancellationToken cancellationToken = default);
}
