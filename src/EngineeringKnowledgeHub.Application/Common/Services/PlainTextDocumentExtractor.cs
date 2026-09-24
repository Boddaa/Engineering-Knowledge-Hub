namespace EngineeringKnowledgeHub.Application.Common.Services;

using System.Text;
using EngineeringKnowledgeHub.Application.Common.Interfaces;

public sealed class PlainTextDocumentExtractor : IDocumentTextExtractor
{
    private static readonly HashSet<string> SupportedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "text/plain",
        "text/markdown",
        "text/csv",
        "text/html",
        "application/json",
        "application/xml",
        "text/xml"
    };

    private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".txt",
        ".md",
        ".markdown",
        ".csv",
        ".log",
        ".json",
        ".xml",
        ".html",
        ".htm"
    };

    public bool CanExtract(string contentType, string fileName)
    {
        if (SupportedContentTypes.Contains(contentType?.Trim() ?? string.Empty))
        {
            return true;
        }

        var extension = Path.GetExtension(fileName);
        return !string.IsNullOrEmpty(extension) && SupportedExtensions.Contains(extension);
    }

    public async Task<string> ExtractTextAsync(
        Stream contentStream,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(contentStream);

        if (contentStream.CanSeek)
        {
            contentStream.Seek(0, SeekOrigin.Begin);
        }

        using var reader = new StreamReader(contentStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
        return await reader.ReadToEndAsync(cancellationToken);
    }
}
