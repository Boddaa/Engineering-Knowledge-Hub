namespace EngineeringKnowledgeHub.Application.SemanticSearch;

using EngineeringKnowledgeHub.Application.Common.Exceptions;

/// <summary>
/// Configuration parameters for deterministic text chunking.
/// </summary>
public sealed record ChunkingOptions(
    int ChunkSize = 500,
    int ChunkOverlap = 50)
{
    public static ChunkingOptions Default { get; } = new(500, 50);

    public void Validate()
    {
        if (ChunkSize <= 0)
        {
            throw new ValidationException("Chunk size must be greater than 0.");
        }

        if (ChunkOverlap < 0)
        {
            throw new ValidationException("Chunk overlap cannot be negative.");
        }

        if (ChunkOverlap >= ChunkSize)
        {
            throw new ValidationException("Chunk overlap must be less than chunk size.");
        }
    }
}
