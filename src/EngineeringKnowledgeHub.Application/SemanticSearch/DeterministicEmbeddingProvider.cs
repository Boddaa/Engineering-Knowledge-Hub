namespace EngineeringKnowledgeHub.Application.SemanticSearch;

using EngineeringKnowledgeHub.Application.Common.Exceptions;

/// <summary>
/// Deterministic, local-first embedding provider using term and character n-gram hashing.
/// Generates reproducible, unit-normalized vectors without external network calls or AI services.
/// </summary>
public sealed class DeterministicEmbeddingProvider : IEmbeddingProvider
{
    public int Dimensions { get; }

    public DeterministicEmbeddingProvider(int dimensions = 64)
    {
        if (dimensions < 4)
        {
            throw new ValidationException("Embedding dimensions must be at least 4.");
        }

        Dimensions = dimensions;
    }

    public Task<Vector> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ValidationException("Text for embedding cannot be null or whitespace.");
        }

        var vector = ComputeHashedVector(text);
        return Task.FromResult(vector);
    }

    public Task<IReadOnlyList<Vector>> GenerateBatchEmbeddingsAsync(
        IReadOnlyList<string> texts,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (texts is null)
        {
            throw new ValidationException("Texts collection cannot be null.");
        }

        var results = new List<Vector>(texts.Count);
        foreach (var text in texts)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ValidationException("Text for embedding cannot be null or whitespace.");
            }

            results.Add(ComputeHashedVector(text));
        }

        return Task.FromResult<IReadOnlyList<Vector>>(results);
    }

    private Vector ComputeHashedVector(string text)
    {
        var buffer = new float[Dimensions];
        var words = text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var word in words)
        {
            var normalizedWord = word.ToLowerInvariant();
            var wordHash = GetStableHash(normalizedWord);
            var dim = Math.Abs(wordHash) % Dimensions;
            buffer[dim] += 1.0f;

            if (normalizedWord.Length >= 3)
            {
                for (int i = 0; i <= normalizedWord.Length - 3; i++)
                {
                    var trigram = normalizedWord.Substring(i, 3);
                    var trigramHash = GetStableHash(trigram);
                    var tDim = Math.Abs(trigramHash) % Dimensions;
                    buffer[tDim] += 0.25f;
                }
            }
        }

        double sumSq = 0.0;
        for (int i = 0; i < buffer.Length; i++)
        {
            sumSq += buffer[i] * buffer[i];
        }

        if (sumSq > 0.0)
        {
            float norm = (float)Math.Sqrt(sumSq);
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] /= norm;
            }
        }

        return new Vector(buffer);
    }

    private static int GetStableHash(string s)
    {
        unchecked
        {
            int hash = 5381;
            foreach (char c in s)
            {
                hash = ((hash << 5) + hash) ^ c;
            }
            return hash;
        }
    }
}
