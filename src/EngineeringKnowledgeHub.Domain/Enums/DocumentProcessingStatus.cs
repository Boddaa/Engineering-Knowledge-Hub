namespace EngineeringKnowledgeHub.Domain.Enums;

/// <summary>
/// Status of an imported document through its ingestion and extraction lifecycle.
/// </summary>
public enum DocumentProcessingStatus
{
    Pending = 1,
    Processing = 2,
    Processed = 3,
    Failed = 4
}
