namespace EngineeringKnowledgeHub.Application.Taxonomy;

using EngineeringKnowledgeHub.Application.Common.Exceptions;
using EngineeringKnowledgeHub.Application.Common.Interfaces;
using EngineeringKnowledgeHub.Application.DTOs;
using EngineeringKnowledgeHub.Domain.Entities;

public sealed class KnowledgeTaxonomyService(
    IKnowledgeCategoryRepository categoryRepository,
    IKnowledgeTypeRepository typeRepository,
    IKnowledgeTagRepository tagRepository) : IKnowledgeTaxonomyService
{
    // ================= Category Operations =================

    public async Task<KnowledgeCategoryDto> CreateCategoryAsync(
        CreateKnowledgeCategoryCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.Name))
        {
            throw new ValidationException("Category name is required.");
        }

        if (command.ParentCategoryId.HasValue)
        {
            var parent = await categoryRepository.GetByIdAsync(command.ParentCategoryId.Value, cancellationToken);
            if (parent is null)
            {
                throw new NotFoundException("KnowledgeCategory", command.ParentCategoryId.Value);
            }
        }

        var category = new KnowledgeCategory(
            Guid.NewGuid(),
            command.Name,
            command.Description,
            command.ParentCategoryId);

        await categoryRepository.AddAsync(category, cancellationToken);

        return KnowledgeCategoryDto.FromDomain(category);
    }

    public async Task<KnowledgeCategoryDto> UpdateCategoryAsync(
        UpdateKnowledgeCategoryCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.Id == Guid.Empty) throw new ValidationException("Category Id cannot be empty.");
        if (string.IsNullOrWhiteSpace(command.Name)) throw new ValidationException("Category name is required.");

        var category = await categoryRepository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new NotFoundException("KnowledgeCategory", command.Id);

        if (command.ParentCategoryId.HasValue && command.ParentCategoryId.Value != command.Id)
        {
            var parent = await categoryRepository.GetByIdAsync(command.ParentCategoryId.Value, cancellationToken);
            if (parent is null)
            {
                throw new NotFoundException("KnowledgeCategory", command.ParentCategoryId.Value);
            }
        }

        category.Update(command.Name, command.Description, command.ParentCategoryId);

        await categoryRepository.UpdateAsync(category, cancellationToken);

        return KnowledgeCategoryDto.FromDomain(category);
    }

    public async Task<KnowledgeCategoryDto> GetCategoryByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty) throw new ValidationException("Category Id cannot be empty.");

        var category = await categoryRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("KnowledgeCategory", id);

        return KnowledgeCategoryDto.FromDomain(category);
    }

    // ================= KnowledgeType Operations =================

    public async Task<KnowledgeTypeDto> CreateTypeAsync(
        CreateKnowledgeTypeCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.Name))
        {
            throw new ValidationException("KnowledgeType name is required.");
        }

        var type = new KnowledgeType(
            Guid.NewGuid(),
            command.Name,
            command.Description);

        await typeRepository.AddAsync(type, cancellationToken);

        return KnowledgeTypeDto.FromDomain(type);
    }

    public async Task<KnowledgeTypeDto> UpdateTypeAsync(
        UpdateKnowledgeTypeCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.Id == Guid.Empty) throw new ValidationException("KnowledgeType Id cannot be empty.");
        if (string.IsNullOrWhiteSpace(command.Name)) throw new ValidationException("KnowledgeType name is required.");

        var type = await typeRepository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new NotFoundException("KnowledgeType", command.Id);

        type.Update(command.Name, command.Description);

        await typeRepository.UpdateAsync(type, cancellationToken);

        return KnowledgeTypeDto.FromDomain(type);
    }

    public async Task<KnowledgeTypeDto> GetTypeByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty) throw new ValidationException("KnowledgeType Id cannot be empty.");

        var type = await typeRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("KnowledgeType", id);

        return KnowledgeTypeDto.FromDomain(type);
    }

    // ================= KnowledgeTag Operations =================

    public async Task<KnowledgeTagDto> CreateTagAsync(
        CreateKnowledgeTagCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.Name))
        {
            throw new ValidationException("Tag name is required.");
        }

        var normalizedName = command.Name.Trim().ToLowerInvariant();
        var existing = await tagRepository.GetByNameAsync(normalizedName, cancellationToken);
        if (existing is not null)
        {
            throw new ConflictException($"Tag '{normalizedName}' already exists.");
        }

        var tag = new KnowledgeTag(
            Guid.NewGuid(),
            command.Name);

        await tagRepository.AddAsync(tag, cancellationToken);

        return KnowledgeTagDto.FromDomain(tag);
    }

    public async Task<KnowledgeTagDto> UpdateTagAsync(
        UpdateKnowledgeTagCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.Id == Guid.Empty) throw new ValidationException("Tag Id cannot be empty.");
        if (string.IsNullOrWhiteSpace(command.Name)) throw new ValidationException("Tag name is required.");

        var tag = await tagRepository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new NotFoundException("KnowledgeTag", command.Id);

        var normalizedName = command.Name.Trim().ToLowerInvariant();
        if (!string.Equals(tag.Name, normalizedName, StringComparison.OrdinalIgnoreCase))
        {
            var existing = await tagRepository.GetByNameAsync(normalizedName, cancellationToken);
            if (existing is not null)
            {
                throw new ConflictException($"Tag '{normalizedName}' already exists.");
            }
        }

        tag.UpdateName(command.Name);

        await tagRepository.UpdateAsync(tag, cancellationToken);

        return KnowledgeTagDto.FromDomain(tag);
    }

    public async Task<KnowledgeTagDto> GetTagByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty) throw new ValidationException("Tag Id cannot be empty.");

        var tag = await tagRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("KnowledgeTag", id);

        return KnowledgeTagDto.FromDomain(tag);
    }
}
