namespace EngineeringKnowledgeHub.Api.Models;

using System.ComponentModel.DataAnnotations;
using System.Text;
using EngineeringKnowledgeHub.Application.Documents;
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
