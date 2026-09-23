namespace EngineeringKnowledgeHub.Domain.Enums;

/// <summary>
/// Defines how a knowledge article relates to an incident or another article.
/// </summary>
public enum ArticleRelationshipType
{
    Supports = 1,
    Resolves = 2,
    RelatesTo = 3,
    CausedBy = 4
}
