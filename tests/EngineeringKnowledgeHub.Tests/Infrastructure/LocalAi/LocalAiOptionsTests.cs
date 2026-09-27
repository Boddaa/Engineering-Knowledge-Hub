namespace EngineeringKnowledgeHub.Tests.Infrastructure.LocalAi;

using EngineeringKnowledgeHub.Application.Common.Exceptions;
using EngineeringKnowledgeHub.Infrastructure.LocalAi;
using Xunit;

public class LocalAiOptionsTests
{
    [Fact]
    public void Validate_WhenDisabled_ShouldNotThrowEvenIfModelsAreEmpty()
    {
        var options = new LocalAiOptions
        {
            Enabled = false,
            GenerationModel = null,
            EmbeddingModel = null,
            BaseUrl = ""
        };

        // Should not throw because Local AI is disabled
        options.Validate();
    }

    [Fact]
    public void Validate_WhenEnabledAndValid_ShouldPass()
    {
        var options = new LocalAiOptions
        {
            Enabled = true,
            BaseUrl = "http://127.0.0.1:11434",
            GenerationModel = "llama3",
            EmbeddingModel = "nomic-embed-text",
            TimeoutSeconds = 60,
            EmbeddingDimensions = 768
        };

        options.Validate();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-a-url")]
    [InlineData("ftp://127.0.0.1:11434")]
    [InlineData("/relative/path")]
    public void Validate_WithInvalidBaseUrl_ShouldThrowValidationException(string invalidBaseUrl)
    {
        var options = new LocalAiOptions
        {
            Enabled = true,
            BaseUrl = invalidBaseUrl,
            GenerationModel = "llama3",
            EmbeddingModel = "nomic-embed-text",
            TimeoutSeconds = 60
        };

        Assert.Throws<ValidationException>(() => options.Validate());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Validate_WithNonPositiveTimeout_ShouldThrowValidationException(int invalidTimeout)
    {
        var options = new LocalAiOptions
        {
            Enabled = true,
            BaseUrl = "http://127.0.0.1:11434",
            GenerationModel = "llama3",
            EmbeddingModel = "nomic-embed-text",
            TimeoutSeconds = invalidTimeout
        };

        Assert.Throws<ValidationException>(() => options.Validate());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithEmptyGenerationModel_ShouldThrowValidationException(string? invalidModel)
    {
        var options = new LocalAiOptions
        {
            Enabled = true,
            BaseUrl = "http://127.0.0.1:11434",
            GenerationModel = invalidModel,
            EmbeddingModel = "nomic-embed-text",
            TimeoutSeconds = 60
        };

        Assert.Throws<ValidationException>(() => options.Validate());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithEmptyEmbeddingModel_ShouldThrowValidationException(string? invalidModel)
    {
        var options = new LocalAiOptions
        {
            Enabled = true,
            BaseUrl = "http://127.0.0.1:11434",
            GenerationModel = "llama3",
            EmbeddingModel = invalidModel,
            TimeoutSeconds = 60
        };

        Assert.Throws<ValidationException>(() => options.Validate());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-768)]
    public void Validate_WithNonPositiveEmbeddingDimensions_ShouldThrowValidationException(int invalidDims)
    {
        var options = new LocalAiOptions
        {
            Enabled = true,
            BaseUrl = "http://127.0.0.1:11434",
            GenerationModel = "llama3",
            EmbeddingModel = "nomic-embed-text",
            TimeoutSeconds = 60,
            EmbeddingDimensions = invalidDims
        };

        Assert.Throws<ValidationException>(() => options.Validate());
    }
}
