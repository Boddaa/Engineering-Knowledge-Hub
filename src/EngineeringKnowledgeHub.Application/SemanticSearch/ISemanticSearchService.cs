namespace EngineeringKnowledgeHub.Application.SemanticSearch;

using EngineeringKnowledgeHub.Application.DTOs;

/// <summary>
/// Contract for high-level semantic retrieval.
/// Orchestrates query embedding, vector lookup, metadata filtering, and result projection.
/// </summary>
public interface ISemanticSearchService
{
    Task<IReadOnlyList<SemanticSearchResultDto>> SearchAsync(
        SemanticSearchRequest request,
        CancellationToken cancellationToken = default);
}
