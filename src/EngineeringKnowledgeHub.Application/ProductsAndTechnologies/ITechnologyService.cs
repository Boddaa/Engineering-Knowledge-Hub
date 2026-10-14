namespace EngineeringKnowledgeHub.Application.ProductsAndTechnologies;

using EngineeringKnowledgeHub.Application.DTOs;

public interface ITechnologyService
{
    Task<TechnologyDto> CreateTechnologyAsync(CreateTechnologyCommand command, CancellationToken cancellationToken = default);
    Task<TechnologyDto> UpdateTechnologyAsync(UpdateTechnologyCommand command, CancellationToken cancellationToken = default);
    Task<TechnologyDto> GetTechnologyByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TechnologyDto>> GetTechnologiesAsync(CancellationToken cancellationToken = default);
}
