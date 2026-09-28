namespace EngineeringKnowledgeHub.Api.Models;

using System.ComponentModel.DataAnnotations;
using EngineeringKnowledgeHub.Application.SemanticSearch;
using EngineeringKnowledgeHub.Domain.Enums;

/// <summary>
/// HTTP request contract for executing semantic vector similarity search queries.
/// </summary>
public sealed record SemanticSearchApiRequest(
    [Required(AllowEmptyStrings = false, ErrorMessage = "Search query cannot be null or whitespace.")]
    string Query,
    int TopK = 10,
    Guid? ArticleId = null,
    Guid? DocumentId = null,
    Guid? ProductId = null,
    Guid? TechnologyId = null,
    Guid? KnowledgeTypeId = null,
    Guid? TagId = null,
    KnowledgeStatus? Status = null,
    VerificationStatus? VerificationStatus = null,
    double MinSimilarity = 0.0) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Query))
        {
            yield return new ValidationResult("Search query cannot be null or whitespace.", [nameof(Query)]);
        }

        if (TopK < 1)
        {
            yield return new ValidationResult("TopK must be greater than 0.", [nameof(TopK)]);
        }

        if (ArticleId.HasValue && ArticleId.Value == Guid.Empty)
        {
            yield return new ValidationResult("ArticleId cannot be an empty GUID.", [nameof(ArticleId)]);
        }

        if (DocumentId.HasValue && DocumentId.Value == Guid.Empty)
        {
            yield return new ValidationResult("DocumentId cannot be an empty GUID.", [nameof(DocumentId)]);
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

        if (Status.HasValue && !Enum.IsDefined(typeof(KnowledgeStatus), Status.Value))
        {
            yield return new ValidationResult("Invalid knowledge status filter.", [nameof(Status)]);
        }

        if (VerificationStatus.HasValue && !Enum.IsDefined(typeof(VerificationStatus), VerificationStatus.Value))
        {
            yield return new ValidationResult("Invalid verification status filter.", [nameof(VerificationStatus)]);
        }

        if (double.IsNaN(MinSimilarity) || double.IsInfinity(MinSimilarity) || MinSimilarity < 0.0 || MinSimilarity > 1.0)
        {
            yield return new ValidationResult("MinSimilarity must be between 0.0 and 1.0.", [nameof(MinSimilarity)]);
        }
    }

    public SemanticSearchRequest ToApplicationRequest() =>
        new()
        {
            Query = Query,
            TopK = TopK,
            ArticleId = ArticleId,
            DocumentId = DocumentId,
            ProductId = ProductId,
            TechnologyId = TechnologyId,
            KnowledgeTypeId = KnowledgeTypeId,
            TagId = TagId,
            Status = Status,
            VerificationStatus = VerificationStatus,
            MinSimilarity = MinSimilarity
        };
}
