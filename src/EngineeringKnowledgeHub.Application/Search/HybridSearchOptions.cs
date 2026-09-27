namespace EngineeringKnowledgeHub.Application.Search;

using EngineeringKnowledgeHub.Application.Common.Exceptions;

/// <summary>
/// Explicit, configurable weighting and scoring parameters for hybrid search ranking.
/// </summary>
public sealed record HybridSearchOptions
{
    public double KeywordWeight { get; init; } = 0.5;
    public double SemanticWeight { get; init; } = 0.5;
    public double VerifiedBoost { get; init; } = 1.1;

    public static HybridSearchOptions Default { get; } = new();

    public void Validate()
    {
        if (KeywordWeight < 0.0)
        {
            throw new ValidationException("Keyword weight cannot be negative.");
        }

        if (SemanticWeight < 0.0)
        {
            throw new ValidationException("Semantic weight cannot be negative.");
        }

        if (KeywordWeight == 0.0 && SemanticWeight == 0.0)
        {
            throw new ValidationException("Keyword weight and semantic weight cannot both be zero.");
        }

        if (VerifiedBoost < 1.0)
        {
            throw new ValidationException("Verified boost cannot be less than 1.0.");
        }
    }
}
