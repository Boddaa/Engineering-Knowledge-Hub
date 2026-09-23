namespace EngineeringKnowledgeHub.Application.Common.Services;

using EngineeringKnowledgeHub.Application.Common.Interfaces;

/// <summary>
/// Default system clock provider using DateTimeOffset.UtcNow.
/// </summary>
public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
