namespace EngineeringKnowledgeHub.Domain.Enums;

/// <summary>
/// Origin type for a knowledge source or supporting reference.
/// </summary>
public enum SourceType
{
    Internal = 1,
    External = 2,
    Incident = 3,
    EngineerExperience = 4,
    VendorDocumentation = 5,
    SystemGenerated = 6,
    AISuggested = 7
}
