namespace EngineeringKnowledgeHub.Application.Ai;

/// <summary>
/// Represents the operational health status of the local AI subsystem.
/// </summary>
public enum LocalAiHealthStatus
{
    /// <summary>
    /// Local AI is disabled in configuration. Deterministic provider is active.
    /// </summary>
    Disabled,

    /// <summary>
    /// Local AI runtime is reachable and all configured models are available.
    /// </summary>
    Healthy,

    /// <summary>
    /// Local AI runtime is unreachable or not responding.
    /// </summary>
    Unavailable,

    /// <summary>
    /// Local AI configuration is invalid or missing required parameters.
    /// </summary>
    Misconfigured,

    /// <summary>
    /// Local AI runtime is reachable, but one or more configured models are not installed/available.
    /// </summary>
    ModelUnavailable
}
