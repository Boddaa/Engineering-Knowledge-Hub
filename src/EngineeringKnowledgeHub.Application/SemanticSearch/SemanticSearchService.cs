namespace EngineeringKnowledgeHub.Application.SemanticSearch;

using EngineeringKnowledgeHub.Application.Common.Exceptions;
using EngineeringKnowledgeHub.Application.DTOs;
using EngineeringKnowledgeHub.Domain.Enums;

/// <summary>
/// Application-level semantic search service.
/// Uses IEmbeddingProvider to convert user queries to dense vectors and retrieves nearest candidates from IVectorStore.
/// Agnostic of underlying vector storage engine or AI provider.
/// </summary>
public sealed class SemanticSearchService : ISemanticSearchService
{
    private readonly IEmbeddingProvider _embeddingProvider;
    private readonly IVectorStore _vectorStore;

    public SemanticSearchService(
        IEmbeddingProvider embeddingProvider,
        IVectorStore vectorStore)
    {
        _embeddingProvider = embeddingProvider ?? throw new ArgumentNullException(nameof(embeddingProvider));
        _vectorStore = vectorStore ?? throw new ArgumentNullException(nameof(vectorStore));
    }

    public async Task<IReadOnlyList<SemanticSearchResultDto>> SearchAsync(
        SemanticSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        var queryVector = await _embeddingProvider.GenerateEmbeddingAsync(request.Query.Trim(), cancellationToken);

        var filter = new VectorSearchFilter(
            ArticleId: request.ArticleId,
            DocumentId: request.DocumentId,
            ProductId: request.ProductId,
            TechnologyId: request.TechnologyId,
            KnowledgeTypeId: request.KnowledgeTypeId,
            TagId: request.TagId,
            Status: request.Status,
            VerificationStatus: request.VerificationStatus);

        var candidates = await _vectorStore.SearchNearestAsync(
            queryVector,
            request.TopK,
            filter,
            cancellationToken);

        var filteredCandidates = request.MinSimilarity > 0.0
            ? candidates.Where(c => c.SimilarityScore >= request.MinSimilarity)
            : candidates;

        var dtos = filteredCandidates.Select(c => new SemanticSearchResultDto(
            c.Record.Id,
            c.Record.ArticleId,
            c.Record.DocumentId,
            c.Record.ChunkId,
            c.Record.Text,
            c.SimilarityScore,
            c.Record.Metadata)).ToList();

        return dtos;
    }

    private static void ValidateRequest(SemanticSearchRequest request)
    {
        if (request is null)
        {
            throw new ValidationException("Semantic search request cannot be null.");
        }

        if (string.IsNullOrWhiteSpace(request.Query))
        {
            throw new ValidationException("Search query cannot be null or whitespace.");
        }

        if (request.TopK < 1)
        {
            throw new ValidationException("TopK must be greater than 0.");
        }

        if (request.ArticleId == Guid.Empty)
        {
            throw new ValidationException("ArticleId cannot be an empty GUID.");
        }

        if (request.DocumentId == Guid.Empty)
        {
            throw new ValidationException("DocumentId cannot be an empty GUID.");
        }

        if (request.ProductId == Guid.Empty)
        {
            throw new ValidationException("ProductId cannot be an empty GUID.");
        }

        if (request.TechnologyId == Guid.Empty)
        {
            throw new ValidationException("TechnologyId cannot be an empty GUID.");
        }

        if (request.KnowledgeTypeId == Guid.Empty)
        {
            throw new ValidationException("KnowledgeTypeId cannot be an empty GUID.");
        }

        if (request.TagId == Guid.Empty)
        {
            throw new ValidationException("TagId cannot be an empty GUID.");
        }

        if (request.Status.HasValue && !Enum.IsDefined(typeof(KnowledgeStatus), request.Status.Value))
        {
            throw new ValidationException("Invalid knowledge status filter.");
        }

        if (request.VerificationStatus.HasValue && !Enum.IsDefined(typeof(VerificationStatus), request.VerificationStatus.Value))
        {
            throw new ValidationException("Invalid verification status filter.");
        }

        if (double.IsNaN(request.MinSimilarity) || double.IsInfinity(request.MinSimilarity) || request.MinSimilarity < 0.0 || request.MinSimilarity > 1.0)
        {
            throw new ValidationException("MinSimilarity must be between 0.0 and 1.0.");
        }
    }
}
