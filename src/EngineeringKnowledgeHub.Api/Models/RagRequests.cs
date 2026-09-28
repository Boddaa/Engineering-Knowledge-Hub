namespace EngineeringKnowledgeHub.Api.Models;

using System.ComponentModel.DataAnnotations;
using EngineeringKnowledgeHub.Application.Rag;
using EngineeringKnowledgeHub.Domain.Enums;

/// <summary>
/// HTTP request contract for submitting a question to the RAG knowledge assistant.
/// </summary>
public sealed record RagQueryRequest(
    [Required(AllowEmptyStrings = false, ErrorMessage = "Question cannot be null or whitespace.")]
    string Question,
    int? MaxEvidence = null,
    double? MinRelevanceScore = null,
    Guid? ProductId = null,
    Guid? TechnologyId = null,
    Guid? KnowledgeTypeId = null,
    Guid? TagId = null,
    VerificationStatus? VerificationStatus = null,
    bool? PreferVerified = null) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Question))
        {
            yield return new ValidationResult("Question cannot be null or whitespace.", [nameof(Question)]);
        }

        if (MaxEvidence.HasValue && (MaxEvidence.Value < 1 || MaxEvidence.Value > 50))
        {
            yield return new ValidationResult("MaxEvidence must be between 1 and 50.", [nameof(MaxEvidence)]);
        }

        if (MinRelevanceScore.HasValue && (double.IsNaN(MinRelevanceScore.Value) || double.IsInfinity(MinRelevanceScore.Value) ||
            MinRelevanceScore.Value < 0.0 || MinRelevanceScore.Value > 1.0))
        {
            yield return new ValidationResult("MinRelevanceScore must be between 0.0 and 1.0.", [nameof(MinRelevanceScore)]);
        }

        if (ProductId.HasValue && ProductId.Value == Guid.Empty)
        {
            yield return new ValidationResult("ProductId cannot be an empty GUID.", [nameof(ProductId)]);
        }

        if (TechnologyId.HasValue && TechnologyId.Value == Guid.Empty)
        {
            yield return new ValidationResult("TechnologyId cannot be an empty GUID.", [nameof(TechnologyId)]);
        }

        if (KnowledgeTypeId.HasValue && KnowledgeTypeId.Value == Guid.Empty)
        {
            yield return new ValidationResult("KnowledgeTypeId cannot be an empty GUID.", [nameof(KnowledgeTypeId)]);
        }

        if (TagId.HasValue && TagId.Value == Guid.Empty)
        {
            yield return new ValidationResult("TagId cannot be an empty GUID.", [nameof(TagId)]);
        }

        if (VerificationStatus.HasValue && !Enum.IsDefined(typeof(VerificationStatus), VerificationStatus.Value))
        {
            yield return new ValidationResult("Invalid verification status filter.", [nameof(VerificationStatus)]);
        }
    }

    public RagQuestionRequest ToApplicationRequest() =>
        new()
        {
            Question = Question,
            MaxEvidence = MaxEvidence ?? 5,
            MinRelevanceScore = MinRelevanceScore ?? 0.25,
            ProductId = ProductId,
            TechnologyId = TechnologyId,
            KnowledgeTypeId = KnowledgeTypeId,
            TagId = TagId,
            VerificationStatus = VerificationStatus,
            PreferVerified = PreferVerified ?? true
        };
}
