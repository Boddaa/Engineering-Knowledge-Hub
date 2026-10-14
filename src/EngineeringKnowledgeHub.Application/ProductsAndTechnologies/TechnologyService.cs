namespace EngineeringKnowledgeHub.Application.ProductsAndTechnologies;

using EngineeringKnowledgeHub.Application.Common.Exceptions;
using EngineeringKnowledgeHub.Application.Common.Interfaces;
using EngineeringKnowledgeHub.Application.DTOs;
using EngineeringKnowledgeHub.Domain.Entities;

public sealed class TechnologyService(ITechnologyRepository technologyRepository) : ITechnologyService
{
    public async Task<TechnologyDto> CreateTechnologyAsync(
        CreateTechnologyCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.Name))
        {
            throw new ValidationException("Technology name is required.");
        }

        var technology = new Technology(
            Guid.NewGuid(),
            command.Name,
            command.Description);

        await technologyRepository.AddAsync(technology, cancellationToken);

        return TechnologyDto.FromDomain(technology);
    }

    public async Task<TechnologyDto> UpdateTechnologyAsync(
        UpdateTechnologyCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.Id == Guid.Empty) throw new ValidationException("Technology Id cannot be empty.");
        if (string.IsNullOrWhiteSpace(command.Name)) throw new ValidationException("Technology name is required.");

        var technology = await technologyRepository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new NotFoundException("Technology", command.Id);

        technology.Update(command.Name, command.Description);

        await technologyRepository.UpdateAsync(technology, cancellationToken);

        return TechnologyDto.FromDomain(technology);
    }

    public async Task<TechnologyDto> GetTechnologyByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty) throw new ValidationException("Technology Id cannot be empty.");

        var technology = await technologyRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Technology", id);

        return TechnologyDto.FromDomain(technology);
    }

    public async Task<IReadOnlyList<TechnologyDto>> GetTechnologiesAsync(CancellationToken cancellationToken = default)
    {
        var technologies = await technologyRepository.GetAllAsync(cancellationToken);
        return technologies.Select(TechnologyDto.FromDomain).ToList();
    }
}
