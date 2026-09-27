namespace EngineeringKnowledgeHub.Application.Enrichment;

using EngineeringKnowledgeHub.Application.DTOs;

/// <summary>
/// Service orchestrating external knowledge enrichment, document ingestion, and human-governed proposals.
/// </summary>
public interface IExternalEnrichmentService
{
    Task<ExternalImportResultDto> ImportFromSourceAsync(
        Guid sourceId,
        ExternalImportCommand? command = null,
        CancellationToken cancellationToken = default);

    Task<KnowledgeArticleDto> ProposeArticleFromDocumentAsync(
        ProposeArticleFromDocumentCommand command,
        CancellationToken cancellationToken = default);
}
