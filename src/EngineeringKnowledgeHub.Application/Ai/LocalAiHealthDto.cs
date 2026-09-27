namespace EngineeringKnowledgeHub.Application.Ai;

/// <summary>
/// Diagnostic information about the operational health of the local AI subsystem.
/// Exposes sufficient diagnostic metadata without leaking secrets, internal paths, or sensitive keys.
/// </summary>
public sealed record LocalAiHealthDto(
    bool Enabled,
    bool RuntimeReachable,
    string? GenerationModelConfigured,
    bool GenerationModelAvailable,
    string? EmbeddingModelConfigured,
    bool EmbeddingModelAvailable,
    LocalAiHealthStatus Status,
    string DiagnosticMessage);
