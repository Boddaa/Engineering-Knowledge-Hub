namespace EngineeringKnowledgeHub.Tests.Application;

using EngineeringKnowledgeHub.Application.Common.Exceptions;
using EngineeringKnowledgeHub.Application.SemanticSearch;
using Xunit;

public class EmbeddingProviderTests
{
    private readonly DeterministicEmbeddingProvider _provider = new(dimensions: 64);

    [Fact]
    public async Task GenerateEmbedding_WithValidText_ShouldReturnNormalizedVectorWithCorrectDimensions()
    {
        var text = "Troubleshooting barrier gate connection timeout in toll collection system.";

        var vector = await _provider.GenerateEmbeddingAsync(text);

        Assert.NotNull(vector);
        Assert.Equal(64, vector.Dimensions);
        Assert.Equal(1.0, vector.ComputeMagnitude(), precision: 5);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GenerateEmbedding_WithNullOrWhitespaceText_ShouldThrowValidationException(string? invalidText)
    {
        await Assert.ThrowsAsync<ValidationException>(() =>
            _provider.GenerateEmbeddingAsync(invalidText!));
    }

    [Fact]
    public async Task GenerateEmbedding_DeterministicBehavior_ShouldProduceIdenticalVectorsForIdenticalText()
    {
        var text = "ANPR camera plate capture failure on lane 3.";

        var v1 = await _provider.GenerateEmbeddingAsync(text);
        var v2 = await _provider.GenerateEmbeddingAsync(text);

        Assert.Equal(v1, v2);
        Assert.Equal(1.0, v1.CosineSimilarityTo(v2), precision: 5);
    }

    [Fact]
    public async Task GenerateEmbedding_SemanticallySimilarTexts_ShouldHaveHighSimilarity()
    {
        var text1 = "Camera barrier gate connection timeout.";
        var text2 = "Barrier gate camera connection timeout error.";
        var text3 = "PostgreSQL index maintenance and query optimization.";

        var v1 = await _provider.GenerateEmbeddingAsync(text1);
        var v2 = await _provider.GenerateEmbeddingAsync(text2);
        var v3 = await _provider.GenerateEmbeddingAsync(text3);

        var sim12 = v1.CosineSimilarityTo(v2);
        var sim13 = v1.CosineSimilarityTo(v3);

        Assert.True(sim12 > 0.65, $"Expected high similarity for related texts, got {sim12}");
        Assert.True(sim12 > sim13, $"Expected sim12 ({sim12}) > sim13 ({sim13})");
    }

    [Fact]
    public void Constructor_WithDimensionsLessThanFour_ShouldThrowValidationException()
    {
        Assert.Throws<ValidationException>(() => new DeterministicEmbeddingProvider(dimensions: 2));
    }

    [Fact]
    public async Task GenerateBatchEmbeddings_WithValidTexts_ShouldReturnOrderedListOfVectors()
    {
        var texts = new List<string>
        {
            "First technical text",
            "Second procedural text",
            "Third troubleshooting text"
        };

        var batch = await _provider.GenerateBatchEmbeddingsAsync(texts);

        Assert.Equal(3, batch.Count);
        for (int i = 0; i < batch.Count; i++)
        {
            Assert.Equal(64, batch[i].Dimensions);
            Assert.Equal(1.0, batch[i].ComputeMagnitude(), precision: 5);
        }
    }

    [Fact]
    public async Task GenerateBatchEmbeddings_WithNullCollectionOrInvalidElement_ShouldThrowValidationException()
    {
        await Assert.ThrowsAsync<ValidationException>(() =>
            _provider.GenerateBatchEmbeddingsAsync(null!));

        await Assert.ThrowsAsync<ValidationException>(() =>
            _provider.GenerateBatchEmbeddingsAsync(["Valid text", ""]));
    }
}
