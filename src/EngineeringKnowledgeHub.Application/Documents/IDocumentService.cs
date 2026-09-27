namespace EngineeringKnowledgeHub.Application.Documents;

using EngineeringKnowledgeHub.Application.DTOs;

public interface IDocumentService
{
    Task<DocumentDto> UploadDocumentAsync(UploadDocumentCommand command, CancellationToken cancellationToken = default);
    Task<DocumentDto> GetDocumentByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DocumentDto>> GetDocumentsBySourceIdAsync(Guid sourceId, CancellationToken cancellationToken = default);
    Task<DocumentDto> ProcessDocumentAsync(ProcessDocumentCommand command, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DocumentChunkDto>> ChunkDocumentAsync(
        Guid documentId,
        SemanticSearch.ChunkingOptions? options = null,
        CancellationToken cancellationToken = default);
}

