namespace EngineeringKnowledgeHub.Application.Common.Interfaces;

public interface IDocumentTextExtractor
{
    bool CanExtract(string contentType, string fileName);
    Task<string> ExtractTextAsync(Stream contentStream, string contentType, CancellationToken cancellationToken = default);
}
