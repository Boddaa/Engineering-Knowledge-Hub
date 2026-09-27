namespace EngineeringKnowledgeHub.Application.Enrichment;

/// <summary>
/// Abstraction for retrieving remote technical knowledge from approved external sources.
/// Independent of HttpClient, vendor SDKs, or cloud services.
/// </summary>
public interface IExternalKnowledgeProvider
{
    Task<ExternalContentResult> FetchContentAsync(
        ExternalContentRequest request,
        CancellationToken cancellationToken = default);
}
