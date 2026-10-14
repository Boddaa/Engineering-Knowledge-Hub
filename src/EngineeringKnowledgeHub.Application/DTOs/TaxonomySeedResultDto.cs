namespace EngineeringKnowledgeHub.Application.DTOs;

public sealed record TaxonomySeedResultDto(
    int TypesAdded,
    int CategoriesAdded,
    int ProductsAdded,
    int TechnologiesAdded,
    int TagsAdded);
