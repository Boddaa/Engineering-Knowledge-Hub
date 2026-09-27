namespace EngineeringKnowledgeHub.Application.Rag;

using EngineeringKnowledgeHub.Application.Common.Exceptions;
using EngineeringKnowledgeHub.Application.Common.Interfaces;
using EngineeringKnowledgeHub.Application.DTOs;
using EngineeringKnowledgeHub.Domain.Enums;

/// <summary>
/// Orchestrates the Retrieval-Augmented Generation (RAG) workflow.
/// Coordinates multi-source evidence retrieval, context building, and provider-independent AI answer generation.
/// Guarantees that AI remains an assistant rather than an authoritative source of truth.
/// </summary>
public sealed class RagService : IRagService
{
    private const string InsufficientEvidenceFallbackAnswer =
        "Insufficient evidence was found in the local engineering repository to answer this question. " +
        "Please consult a senior engineer or submit a new knowledge article.";

    public const string GroundingFailureFallbackAnswer =
        "The generated response could not be verified against the retrieved evidence citations. " +
        "Please refer directly to the attached evidence sources.";

    private readonly IKnowledgeRetriever _retriever;
    private readonly IContextBuilder _contextBuilder;
    private readonly IAiProvider _aiProvider;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IGroundingValidator _groundingValidator;

    public RagService(
        IKnowledgeRetriever retriever,
        IContextBuilder contextBuilder,
        IAiProvider aiProvider,
        IDateTimeProvider dateTimeProvider,
        IGroundingValidator? groundingValidator = null)
    {
        _retriever = retriever ?? throw new ArgumentNullException(nameof(retriever));
        _contextBuilder = contextBuilder ?? throw new ArgumentNullException(nameof(contextBuilder));
        _aiProvider = aiProvider ?? throw new ArgumentNullException(nameof(aiProvider));
        _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
        _groundingValidator = groundingValidator ?? new GroundingValidator();
    }

    public async Task<RagResponseDto> AskAsync(
        RagQuestionRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        var trimmedQuestion = request.Question.Trim();

        // 1. Retrieve evidence
        var retrievalRequest = new RagRetrievalRequest
        {
            Query = trimmedQuestion,
            MaxEvidenceItems = request.MaxEvidence,
            MinRelevanceScore = request.MinRelevanceScore,
            ProductId = request.ProductId,
            TechnologyId = request.TechnologyId,
            KnowledgeTypeId = request.KnowledgeTypeId,
            TagId = request.TagId,
            VerificationStatus = request.VerificationStatus,
            PreferVerified = request.PreferVerified
        };

        var evidenceItems = await _retriever.RetrieveEvidenceAsync(retrievalRequest, cancellationToken);

        // 2. Build context
        var context = _contextBuilder.BuildContext(
            trimmedQuestion,
            evidenceItems,
            new RagContextOptions
            {
                InsufficientEvidenceThreshold = request.MinRelevanceScore
            });

        // 3. Handle insufficient evidence: never fabricate an answer
        if (!context.HasSufficientEvidence || context.EvidenceItems.Count == 0)
        {
            return new RagResponseDto(
                Question: trimmedQuestion,
                Answer: InsufficientEvidenceFallbackAnswer,
                IsSufficientEvidence: false,
                Evidence: Array.Empty<RagEvidenceDto>(),
                ModelUsed: "DeterministicLocalModel",
                AnsweredAt: _dateTimeProvider.UtcNow);
        }

        // 4. Generate answer via AI abstraction
        var aiResponse = await _aiProvider.GenerateAsync(
            new AiPromptRequest(
                SystemPrompt: context.SystemPrompt,
                UserPrompt: context.UserPrompt),
            cancellationToken);

        // 5. Project evidence citations for the caller (always preserved for traceability)
        var evidenceDtos = context.EvidenceItems.Select(e => new RagEvidenceDto(
            CitationNumber: e.CitationNumber,
            SourceType: e.SourceType,
            ReferenceId: e.ReferenceId,
            SecondaryId: e.SecondaryId,
            Title: e.Title,
            Excerpt: TruncateExcerpt(e.Content, 250),
            RelevanceScore: e.RelevanceScore,
            VerificationStatus: e.VerificationStatus,
            Metadata: e.Metadata)).ToList();

        // 6. Enforce Grounding Validation boundary
        var groundingResult = _groundingValidator.ValidateGrounding(aiResponse.Content, context.EvidenceItems);
        if (!groundingResult.IsGrounded)
        {
            return new RagResponseDto(
                Question: trimmedQuestion,
                Answer: GroundingFailureFallbackAnswer,
                IsSufficientEvidence: false,
                Evidence: evidenceDtos,
                ModelUsed: aiResponse.ModelName,
                AnsweredAt: _dateTimeProvider.UtcNow);
        }

        return new RagResponseDto(
            Question: trimmedQuestion,
            Answer: aiResponse.Content,
            IsSufficientEvidence: true,
            Evidence: evidenceDtos,
            ModelUsed: aiResponse.ModelName,
            AnsweredAt: _dateTimeProvider.UtcNow);
    }

    private static void ValidateRequest(RagQuestionRequest request)
    {
        if (request is null)
        {
            throw new ValidationException("RAG request cannot be null.");
        }

        if (string.IsNullOrWhiteSpace(request.Question))
        {
            throw new ValidationException("Question cannot be null or whitespace.");
        }

        if (request.MaxEvidence < 1 || request.MaxEvidence > 50)
        {
            throw new ValidationException("MaxEvidence must be between 1 and 50.");
        }

        if (double.IsNaN(request.MinRelevanceScore) || double.IsInfinity(request.MinRelevanceScore) ||
            request.MinRelevanceScore < 0.0 || request.MinRelevanceScore > 1.0)
        {
            throw new ValidationException("MinRelevanceScore must be between 0.0 and 1.0.");
        }

        if (request.ProductId == Guid.Empty)
        {
            throw new ValidationException("ProductId cannot be an empty GUID.");
        }

        if (request.TechnologyId == Guid.Empty)
        {
            throw new ValidationException("TechnologyId cannot be an empty GUID.");
        }

        if (request.KnowledgeTypeId == Guid.Empty)
        {
            throw new ValidationException("KnowledgeTypeId cannot be an empty GUID.");
        }

        if (request.TagId == Guid.Empty)
        {
            throw new ValidationException("TagId cannot be an empty GUID.");
        }

        if (request.VerificationStatus.HasValue && !Enum.IsDefined(typeof(VerificationStatus), request.VerificationStatus.Value))
        {
            throw new ValidationException("Invalid verification status filter.");
        }
    }

    private static string TruncateExcerpt(string text, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var normalized = text.Replace("\r", " ").Replace("\n", " ").Trim();
        return normalized.Length <= maxLength
            ? normalized
            : normalized[..maxLength].TrimEnd() + "...";
    }
}
