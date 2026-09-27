namespace EngineeringKnowledgeHub.Application.Search;

using EngineeringKnowledgeHub.Domain.Enums;

/// <summary>
/// Search request contract combining keyword query terms, structured metadata filters,
/// and hybrid ranking configurations.
/// </summary>
public sealed record HybridSearchArticleRequest
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
    public HybridSearchOptions Options { get; init; } = HybridSearchOptions.Default;
}
