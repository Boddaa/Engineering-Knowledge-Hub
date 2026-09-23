namespace EngineeringKnowledgeHub.Application.Search;

/// <summary>
/// Explicit, safe sorting dimensions for knowledge search.
/// </summary>
public enum SearchSortOption
{
    Relevance = 0,
    RecentlyUpdated = 1,
    RecentlyCreated = 2,
    TitleAsc = 3,
    TitleDesc = 4
}
