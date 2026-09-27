namespace EngineeringKnowledgeHub.Application.Rag;

/// <summary>
/// Contract for multi-source knowledge and evidence retrieval.
/// Orchestrates local search, semantic vector candidate lookup, and incident inspection
/// to build ranked, traceable evidence candidates.
/// </summary>
public interface IKnowledgeRetriever
{
    Task<IReadOnlyList<RagEvidenceItem>> RetrieveEvidenceAsync(
        RagRetrievalRequest request,
        CancellationToken cancellationToken = default);
}
