namespace EngineeringKnowledgeHub.Application.Search;

/// <summary>
/// Explicit, bounded pagination container for search and query results.
/// </summary>
/// <typeparam name="T">Result item type.</typeparam>
public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount)
{
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
    public bool HasPreviousPage => Page > 1 && TotalPages > 0;
    public bool HasNextPage => Page < TotalPages;

    public static PagedResult<T> Empty(int page, int pageSize) =>
        new([], page, pageSize, 0);
}
