namespace EngineeringKnowledgeHub.Application.Search;

using EngineeringKnowledgeHub.Domain.Enums;

/// <summary>
/// Explicit, validated search request contract for global knowledge article retrieval.
/// </summary>
public sealed record SearchArticleRequest
{
    public string? Query { get; init; }
    public Guid? ProductId { get; init; }
    public Guid? TechnologyId { get; init; }
    public Guid? KnowledgeTypeId { get; init; }
    public Guid? TagId { get; init; }
    public KnowledgeStatus? Status { get; init; }
    public VerificationStatus? VerificationStatus { get; init; }
    public SearchSortOption SortBy { get; init; } = SearchSortOption.Relevance;
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
