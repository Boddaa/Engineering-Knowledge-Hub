namespace EngineeringKnowledgeHub.Tests.Application;

using EngineeringKnowledgeHub.Application.Common.Exceptions;
using EngineeringKnowledgeHub.Application.SemanticSearch;
using Xunit;

public class VectorAndSimilarityTests
{
    [Fact]
    public void Vector_Creation_WithValidValues_ShouldExposeDimensionsAndValues()
    {
        var values = new float[] { 1.0f, 2.0f, 3.0f };
        var vector = new Vector(values);

        Assert.Equal(3, vector.Dimensions);
        Assert.Equal(1.0f, vector[0]);
        Assert.Equal(2.0f, vector[1]);
        Assert.Equal(3.0f, vector[2]);
        Assert.Equal(Math.Sqrt(14.0), vector.ComputeMagnitude(), precision: 5);
    }

    [Fact]
    public void Vector_Creation_WithNullOrEmptyValues_ShouldThrowValidationException()
    {
        Assert.Throws<ValidationException>(() => new Vector((float[])null!));
        Assert.Throws<ValidationException>(() => new Vector(Array.Empty<float>()));
    }

    [Theory]
    [InlineData(float.NaN)]
    [InlineData(float.PositiveInfinity)]
    [InlineData(float.NegativeInfinity)]
    public void Vector_Creation_WithInvalidFloat_ShouldThrowValidationException(float invalidValue)
    {
        Assert.Throws<ValidationException>(() => new Vector([1.0f, invalidValue, 3.0f]));
    }

    [Fact]
    public void CosineSimilarity_WithIdenticalVectors_ShouldReturnOne()
    {
        var v1 = new Vector([1.0f, 2.0f, 3.0f]);
        var v2 = new Vector([1.0f, 2.0f, 3.0f]);

        var similarity = v1.CosineSimilarityTo(v2);

        Assert.Equal(1.0, similarity, precision: 5);
    }

    [Fact]
    public void CosineSimilarity_WithOrthogonalVectors_ShouldReturnZero()
    {
        var v1 = new Vector([1.0f, 0.0f, 0.0f]);
        var v2 = new Vector([0.0f, 1.0f, 0.0f]);

        var similarity = v1.CosineSimilarityTo(v2);

        Assert.Equal(0.0, similarity, precision: 5);
    }

    [Fact]
    public void CosineSimilarity_WithOppositeVectors_ShouldReturnMinusOne()
    {
        var v1 = new Vector([1.0f, 0.0f]);
        var v2 = new Vector([-1.0f, 0.0f]);

        var similarity = v1.CosineSimilarityTo(v2);

        Assert.Equal(-1.0, similarity, precision: 5);
    }

    [Fact]
    public void CosineSimilarity_WithZeroVector_ShouldReturnZeroSafelyWithoutDivideByZero()
    {
        var zeroVec = new Vector([0.0f, 0.0f, 0.0f]);
        var nonZeroVec = new Vector([1.0f, 2.0f, 3.0f]);

        var sim1 = zeroVec.CosineSimilarityTo(nonZeroVec);
        var sim2 = nonZeroVec.CosineSimilarityTo(zeroVec);
        var sim3 = zeroVec.CosineSimilarityTo(zeroVec);

        Assert.Equal(0.0, sim1);
        Assert.Equal(0.0, sim2);
        Assert.Equal(0.0, sim3);
    }

    [Fact]
    public void CosineSimilarity_WithDimensionMismatch_ShouldThrowValidationException()
    {
        var v3 = new Vector([1.0f, 2.0f, 3.0f]);
        var v4 = new Vector([1.0f, 2.0f, 3.0f, 4.0f]);

        Assert.Throws<ValidationException>(() => v3.CosineSimilarityTo(v4));
    }

    [Fact]
    public void DotProduct_WithDimensionMismatch_ShouldThrowValidationException()
    {
        var v2 = new Vector([1.0f, 2.0f]);
        var v3 = new Vector([1.0f, 2.0f, 3.0f]);

        Assert.Throws<ValidationException>(() => v2.DotProduct(v3));
    }

    [Fact]
    public void Vector_Equals_ShouldCompareValuesWithPrecision()
    {
        var v1 = new Vector([1.0f, 2.0f]);
        var v2 = new Vector([1.0f, 2.0f]);
        var v3 = new Vector([1.0f, 2.001f]);

        Assert.True(v1.Equals(v2));
        Assert.False(v1.Equals(v3));
        Assert.Equal(v1.GetHashCode(), v2.GetHashCode());
    }
}
