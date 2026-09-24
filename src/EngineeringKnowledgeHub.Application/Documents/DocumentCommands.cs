namespace EngineeringKnowledgeHub.Application.Documents;

public sealed record UploadDocumentCommand(
    string FileName,
    string ContentType,
    Guid SourceId,
    Guid UploadedById,
    byte[] Content,
    string? StoragePath = null);

public sealed record ProcessDocumentCommand(
    Guid DocumentId,
    byte[]? Content = null);
