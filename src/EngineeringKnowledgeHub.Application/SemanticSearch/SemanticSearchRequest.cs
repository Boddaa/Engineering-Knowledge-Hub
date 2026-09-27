namespace EngineeringKnowledgeHub.Application.SemanticSearch;

using EngineeringKnowledgeHub.Domain.Enums;

/// <summary>
/// Request contract for semantic search queries across indexed vector content.
/// </summary>
public sealed record SemanticSearchRequest
{
    public string Query { get; init; } = string.Empty;
    public int TopK { get; init; } = 10;
    public Guid? ArticleId { get; init; }
    public Guid? DocumentId { get; init; }
    public Guid? ProductId { get; init; }
    public Guid? TechnologyId { get; init; }
    public Guid? KnowledgeTypeId { get; init; }
    public Guid? TagId { get; init; }
    public KnowledgeStatus? Status { get; init; }
    public VerificationStatus? VerificationStatus { get; init; }
    public double MinSimilarity { get; init; } = 0.0;
}
