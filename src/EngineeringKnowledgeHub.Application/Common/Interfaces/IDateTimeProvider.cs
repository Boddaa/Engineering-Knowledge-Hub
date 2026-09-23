namespace EngineeringKnowledgeHub.Application.Common.Interfaces;

/// <summary>
/// Abstraction for system clock access to ensure testability and prevent callers from controlling timestamps.
/// </summary>
public interface IDateTimeProvider
{
    DateTimeOffset UtcNow { get; }
}
