namespace EngineeringKnowledgeHub.Application.SemanticSearch;

using EngineeringKnowledgeHub.Application.Common.Exceptions;

/// <summary>
/// Provider-neutral, immutable vector representation for text embeddings.
/// Encapsulates numeric components, dimensionality, and similarity mathematics.
/// </summary>
public sealed class Vector : IEquatable<Vector>
{
    private readonly float[] _values;

    public IReadOnlyList<float> Values => _values;
    public int Dimensions => _values.Length;

    public Vector(float[] values)
    {
        if (values is null || values.Length == 0)
        {
            throw new ValidationException("Vector values cannot be null or empty.");
        }

        for (int i = 0; i < values.Length; i++)
        {
            if (float.IsNaN(values[i]) || float.IsInfinity(values[i]))
            {
                throw new ValidationException($"Vector value at index {i} is invalid (NaN or Infinity).");
            }
        }

        _values = (float[])values.Clone();
    }

    public Vector(IEnumerable<float> values)
        : this(values?.ToArray() ?? throw new ValidationException("Vector values cannot be null or empty."))
    {
    }

    public float this[int index] => _values[index];

    public double ComputeMagnitude()
    {
        double sum = 0.0;
        for (int i = 0; i < _values.Length; i++)
        {
            sum += (double)_values[i] * _values[i];
        }

        return Math.Sqrt(sum);
    }

    public double DotProduct(Vector other)
    {
        ArgumentNullException.ThrowIfNull(other);

        if (Dimensions != other.Dimensions)
        {
            throw new ValidationException($"Dimension mismatch: cannot compute dot product between vectors of dimensions {Dimensions} and {other.Dimensions}.");
        }

        double dot = 0.0;
        for (int i = 0; i < _values.Length; i++)
        {
            dot += (double)_values[i] * other._values[i];
        }

        return dot;
    }

    public double CosineSimilarityTo(Vector other)
    {
        ArgumentNullException.ThrowIfNull(other);

        if (Dimensions != other.Dimensions)
        {
            throw new ValidationException($"Dimension mismatch: cannot compute cosine similarity between vectors of dimensions {Dimensions} and {other.Dimensions}.");
        }

        var magA = ComputeMagnitude();
        var magB = other.ComputeMagnitude();

        // Safe zero-vector handling
        if (magA == 0.0 || magB == 0.0)
        {
            return 0.0;
        }

        var dot = DotProduct(other);
        var similarity = dot / (magA * magB);

        return Math.Clamp(similarity, -1.0, 1.0);
    }

    public bool Equals(Vector? other)
    {
        if (other is null || Dimensions != other.Dimensions) return false;
        if (ReferenceEquals(this, other)) return true;

        for (int i = 0; i < _values.Length; i++)
        {
            if (Math.Abs(_values[i] - other._values[i]) > 1e-6f)
            {
                return false;
            }
        }

        return true;
    }

    public override bool Equals(object? obj) => obj is Vector other && Equals(other);

    public override int GetHashCode()
    {
        HashCode hash = default;
        for (int i = 0; i < Math.Min(_values.Length, 8); i++)
        {
            hash.Add(_values[i]);
        }
        hash.Add(Dimensions);
        return hash.ToHashCode();
    }

    public override string ToString() => $"Vector[{Dimensions}]";
}
