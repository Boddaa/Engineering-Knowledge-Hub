namespace EngineeringKnowledgeHub.Application.Feedback;

using EngineeringKnowledgeHub.Application.DTOs;

/// <summary>
/// Application service contract for article helpfulness feedback and rating.
/// </summary>
public interface IKnowledgeFeedbackService
{
    Task<KnowledgeFeedbackDto> SubmitFeedbackAsync(SubmitFeedbackCommand command, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<KnowledgeFeedbackDto>> GetFeedbackForArticleAsync(Guid articleId, CancellationToken cancellationToken = default);
}
