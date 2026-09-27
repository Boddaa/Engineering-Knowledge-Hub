namespace EngineeringKnowledgeHub.Application.Rag;

/// <summary>
/// Contract for constructing structured prompts and bounded context from retrieved engineering evidence.
/// Enforces token budgets, structured citation formatting, and evidence sufficiency validation.
/// </summary>
public interface IContextBuilder
{
    RagContext BuildContext(
        string question,
        IReadOnlyList<RagEvidenceItem> evidence,
        RagContextOptions? options = null);
}
