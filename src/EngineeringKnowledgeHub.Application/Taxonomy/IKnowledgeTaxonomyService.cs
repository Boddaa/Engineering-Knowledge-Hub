namespace EngineeringKnowledgeHub.Application.Taxonomy;

using EngineeringKnowledgeHub.Application.DTOs;

public interface IKnowledgeTaxonomyService
{
    // Categories
    Task<KnowledgeCategoryDto> CreateCategoryAsync(CreateKnowledgeCategoryCommand command, CancellationToken cancellationToken = default);
    Task<KnowledgeCategoryDto> UpdateCategoryAsync(UpdateKnowledgeCategoryCommand command, CancellationToken cancellationToken = default);
    Task<KnowledgeCategoryDto> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken = default);

    // Knowledge Types
    Task<KnowledgeTypeDto> CreateTypeAsync(CreateKnowledgeTypeCommand command, CancellationToken cancellationToken = default);
    Task<KnowledgeTypeDto> UpdateTypeAsync(UpdateKnowledgeTypeCommand command, CancellationToken cancellationToken = default);
    Task<KnowledgeTypeDto> GetTypeByIdAsync(Guid id, CancellationToken cancellationToken = default);

    // Knowledge Tags
    Task<KnowledgeTagDto> CreateTagAsync(CreateKnowledgeTagCommand command, CancellationToken cancellationToken = default);
    Task<KnowledgeTagDto> UpdateTagAsync(UpdateKnowledgeTagCommand command, CancellationToken cancellationToken = default);
    Task<KnowledgeTagDto> GetTagByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
