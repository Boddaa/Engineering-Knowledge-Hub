namespace EngineeringKnowledgeHub.Api.Models;

using System.ComponentModel.DataAnnotations;
using System.Text;
using EngineeringKnowledgeHub.Application.Documents;
using EngineeringKnowledgeHub.Application.Enrichment;
using EngineeringKnowledgeHub.Application.SemanticSearch;

/// <summary>
/// HTTP request contract for uploading/registering a document.
/// </summary>
public sealed record UploadDocumentRequest(
    [Required(AllowEmptyStrings = false, ErrorMessage = "File name is required.")]
    string FileName,
    [Required(AllowEmptyStrings = false, ErrorMessage = "Content type is required.")]
    string ContentType,
    Guid SourceId,
    Guid UploadedById,
    [Required(AllowEmptyStrings = false, ErrorMessage = "Document content is required.")]
    string Content,
    string? StoragePath = null) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(FileName))
        {
            yield return new ValidationResult("File name is required.", [nameof(FileName)]);
        }

        if (string.IsNullOrWhiteSpace(ContentType))
        {
            yield return new ValidationResult("Content type is required.", [nameof(ContentType)]);
        }

        if (string.IsNullOrWhiteSpace(Content))
        {
            yield return new ValidationResult("Document content is required.", [nameof(Content)]);
        }
    }

    public UploadDocumentCommand ToCommand()
    {
        byte[] bytes;
        try
        {
            bytes = Convert.FromBase64String(Content);
        }
        catch
        {
            bytes = Encoding.UTF8.GetBytes(Content);
        }

        return new UploadDocumentCommand(FileName, ContentType, SourceId, UploadedById, bytes, StoragePath);
    }
}

/// <summary>
/// HTTP request contract for processing an uploaded document.
/// </summary>
public sealed record ProcessDocumentRequest(string? Content = null)
{
    public ProcessDocumentCommand ToCommand(Guid documentId)
    {
        byte[]? bytes = null;
        if (!string.IsNullOrWhiteSpace(Content))
        {
            try
            {
                bytes = Convert.FromBase64String(Content);
            }
            catch
            {
                bytes = Encoding.UTF8.GetBytes(Content);
            }
        }

        return new ProcessDocumentCommand(documentId, bytes);
    }
}

/// <summary>
/// HTTP request contract for chunking a processed document.
/// </summary>
public sealed record ChunkDocumentRequest(int? TargetChunkSize = null, int? OverlapTokens = null)
{
    public ChunkingOptions? ToOptions() =>
        TargetChunkSize.HasValue || OverlapTokens.HasValue
            ? new ChunkingOptions(TargetChunkSize ?? 500, OverlapTokens ?? 50)
            : null;
}

/// <summary>
/// HTTP request contract for proposing a draft knowledge article synthesized from an imported document.
/// Proposing creates an Unverified Draft article, strictly requiring human review before publication.
/// </summary>
public sealed record ProposeArticleFromDocumentRequest(
    [Required(AllowEmptyStrings = false, ErrorMessage = "Title is required.")]
    string Title,
    Guid AuthorId,
    Guid KnowledgeTypeId,
    string? Summary = null,
    string? Content = null,
    string? ReferenceText = null,
    IReadOnlyCollection<Guid>? ProductIds = null,
    IReadOnlyCollection<Guid>? TechnologyIds = null,
    IReadOnlyCollection<Guid>? TagIds = null) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Title))
        {
            yield return new ValidationResult("Title is required when proposing an article.", [nameof(Title)]);
        }

        if (AuthorId == Guid.Empty)
        {
            yield return new ValidationResult("AuthorId cannot be empty.", [nameof(AuthorId)]);
        }

        if (KnowledgeTypeId == Guid.Empty)
        {
            yield return new ValidationResult("KnowledgeTypeId cannot be empty.", [nameof(KnowledgeTypeId)]);
        }
    }

    public ProposeArticleFromDocumentCommand ToCommand(Guid documentId) =>
        new(documentId, AuthorId, KnowledgeTypeId, Title, Summary, Content, ReferenceText, ProductIds, TechnologyIds, TagIds);
}
