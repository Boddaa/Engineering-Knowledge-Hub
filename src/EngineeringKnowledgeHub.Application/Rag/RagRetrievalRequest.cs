namespace EngineeringKnowledgeHub.Application.Rag;

using EngineeringKnowledgeHub.Domain.Enums;

/// <summary>
/// Parameters for multi-source knowledge retrieval.
/// </summary>
public sealed class RagRetrievalRequest
{
    public string Query { get; set; } = string.Empty;

    public int MaxEvidenceItems { get; set; } = 5;

    public double MinRelevanceScore { get; set; } = 0.25;

    public Guid? ProductId { get; set; }

    public Guid? TechnologyId { get; set; }

    public Guid? KnowledgeTypeId { get; set; }

    public Guid? TagId { get; set; }

    public VerificationStatus? VerificationStatus { get; set; }

    public bool PreferVerified { get; set; } = true;
}
