namespace EngineeringKnowledgeHub.Domain.Enums;

/// <summary>
/// Trust tier for knowledge sources to differentiate verified internal assets from unvetted external sources.
/// </summary>
public enum TrustLevel
{
    Untrusted = 1,
    Community = 2,
    Verified = 3,
    Official = 4
}
