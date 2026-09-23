namespace EngineeringKnowledgeHub.Application.Feedback;

using EngineeringKnowledgeHub.Application.Common.Exceptions;
using EngineeringKnowledgeHub.Application.Common.Interfaces;
using EngineeringKnowledgeHub.Application.DTOs;
using EngineeringKnowledgeHub.Domain.Entities;
using EngineeringKnowledgeHub.Domain.Enums;

/// <summary>
/// Service coordinating helpfulness ratings and feedback on knowledge articles.
/// </summary>
public sealed class KnowledgeFeedbackService : IKnowledgeFeedbackService
{
    private readonly IKnowledgeFeedbackRepository _feedbackRepository;
    private readonly IKnowledgeArticleRepository _articleRepository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public KnowledgeFeedbackService(
        IKnowledgeFeedbackRepository feedbackRepository,
        IKnowledgeArticleRepository articleRepository,
        IDateTimeProvider dateTimeProvider)
    {
        _feedbackRepository = feedbackRepository ?? throw new ArgumentNullException(nameof(feedbackRepository));
        _articleRepository = articleRepository ?? throw new ArgumentNullException(nameof(articleRepository));
        _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
    }

    public async Task<KnowledgeFeedbackDto> SubmitFeedbackAsync(
        SubmitFeedbackCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command is null)
        {
            throw new ValidationException("Submit feedback command cannot be null.");
        }

        if (command.ArticleId == Guid.Empty)
        {
            throw new ValidationException("ArticleId cannot be empty.");
        }

        if (command.UserId == Guid.Empty)
        {
            throw new ValidationException("UserId cannot be empty.");
        }

        if (!Enum.IsDefined(typeof(FeedbackType), command.FeedbackType))
        {
            throw new ValidationException("Invalid feedback type.");
        }

        var article = await _articleRepository.GetByIdAsync(command.ArticleId, cancellationToken);
        if (article is null)
        {
            throw new NotFoundException("KnowledgeArticle", command.ArticleId);
        }

        if (article.Status != KnowledgeStatus.Published)
        {
            throw new ValidationException(
                $"Cannot submit feedback for an article with status '{article.Status}'. Feedback can only be submitted for published articles.");
        }

        var feedback = new KnowledgeFeedback(
            Guid.NewGuid(),
            command.ArticleId,
            command.UserId,
            command.FeedbackType,
            _dateTimeProvider.UtcNow,
            command.Comment);

        await _feedbackRepository.AddAsync(feedback, cancellationToken);

        return KnowledgeFeedbackDto.FromDomain(feedback);
    }

    public async Task<IReadOnlyList<KnowledgeFeedbackDto>> GetFeedbackForArticleAsync(
        Guid articleId,
        CancellationToken cancellationToken = default)
    {
        if (articleId == Guid.Empty)
        {
            throw new ValidationException("ArticleId cannot be empty.");
        }

        var list = await _feedbackRepository.GetByArticleIdAsync(articleId, cancellationToken);
        return list.Select(KnowledgeFeedbackDto.FromDomain).ToList();
    }
}
