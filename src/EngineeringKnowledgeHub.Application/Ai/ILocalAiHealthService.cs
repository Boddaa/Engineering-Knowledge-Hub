namespace EngineeringKnowledgeHub.Application.Ai;

/// <summary>
/// Operational health and diagnostics contract for the local AI subsystem.
/// Determines whether the configured runtime is reachable and whether the configured models are loaded.
/// </summary>
public interface ILocalAiHealthService
{
    Task<LocalAiHealthDto> CheckHealthAsync(CancellationToken cancellationToken = default);
}
