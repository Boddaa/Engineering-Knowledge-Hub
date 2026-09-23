namespace EngineeringKnowledgeHub.Application.Common.Interfaces;

using EngineeringKnowledgeHub.Domain.Entities;

/// <summary>
/// Repository abstraction for knowledge article feedback.
/// </summary>
public interface IKnowledgeFeedbackRepository
{
    Task AddAsync(KnowledgeFeedback feedback, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<KnowledgeFeedback>> GetByArticleIdAsync(Guid articleId, CancellationToken cancellationToken = default);
}
