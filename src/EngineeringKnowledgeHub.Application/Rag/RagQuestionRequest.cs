namespace EngineeringKnowledgeHub.Application.Rag;

using EngineeringKnowledgeHub.Domain.Enums;

/// <summary>
/// Request parameters for querying the RAG knowledge assistant.
/// </summary>
public sealed class RagQuestionRequest
{
    public string Question { get; set; } = string.Empty;

    public int MaxEvidence { get; set; } = 5;

    public double MinRelevanceScore { get; set; } = 0.25;

    public Guid? ProductId { get; set; }

    public Guid? TechnologyId { get; set; }

    public Guid? KnowledgeTypeId { get; set; }

    public Guid? TagId { get; set; }

    public VerificationStatus? VerificationStatus { get; set; }

    public bool PreferVerified { get; set; } = true;
}
