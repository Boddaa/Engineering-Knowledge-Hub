namespace EngineeringKnowledgeHub.Application.Enrichment;

/// <summary>
/// Command to import content from an approved external knowledge source.
/// </summary>
public sealed record ExternalImportCommand(
    Guid? ImportedById = null,
    string? CustomFileName = null,
    bool? ChunkAndIndex = null);

/// <summary>
/// Command to propose a draft knowledge article from an imported document.
/// Proposing creates an Unverified Draft article, strictly requiring human review before publication.
/// </summary>
public sealed record ProposeArticleFromDocumentCommand(
    Guid DocumentId,
    Guid AuthorId,
    Guid KnowledgeTypeId,
    string Title,
    string? Summary = null,
    string? Content = null,
    string? ReferenceText = null,
    IReadOnlyCollection<Guid>? ProductIds = null,
    IReadOnlyCollection<Guid>? TechnologyIds = null,
    IReadOnlyCollection<Guid>? TagIds = null);
