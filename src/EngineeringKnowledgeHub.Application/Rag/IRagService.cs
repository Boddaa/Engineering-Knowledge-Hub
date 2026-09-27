namespace EngineeringKnowledgeHub.Application.Rag;

using EngineeringKnowledgeHub.Application.DTOs;

/// <summary>
/// High-level application service contract for Retrieval-Augmented Generation (RAG).
/// Orchestrates knowledge retrieval, evidence ranking, context construction, and answer generation.
/// </summary>
public interface IRagService
{
    Task<RagResponseDto> AskAsync(
        RagQuestionRequest request,
        CancellationToken cancellationToken = default);
}
