namespace EngineeringKnowledgeHub.Api.Models;

using System.ComponentModel.DataAnnotations;
using EngineeringKnowledgeHub.Application.Rag;

/// <summary>
/// HTTP request contract for direct AI prompt execution via the configured AI provider.
/// </summary>
public sealed record AiGenerateRequest(
    [Required(AllowEmptyStrings = false, ErrorMessage = "User prompt cannot be null or whitespace.")]
    string UserPrompt,
    string? SystemPrompt = null,
    double? Temperature = null,
    int? MaxTokens = null) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(UserPrompt))
        {
            yield return new ValidationResult("User prompt cannot be null or whitespace.", [nameof(UserPrompt)]);
        }

        if (Temperature.HasValue && (Temperature.Value < 0.0 || Temperature.Value > 2.0))
        {
            yield return new ValidationResult("Temperature must be between 0.0 and 2.0.", [nameof(Temperature)]);
        }

        if (MaxTokens.HasValue && MaxTokens.Value <= 0)
        {
            yield return new ValidationResult("MaxTokens must be greater than zero.", [nameof(MaxTokens)]);
        }
    }

    public AiPromptRequest ToPromptRequest() =>
        new(
            SystemPrompt: SystemPrompt ?? string.Empty,
            UserPrompt: UserPrompt,
            MaxTokens: MaxTokens,
            Temperature: Temperature);
}
