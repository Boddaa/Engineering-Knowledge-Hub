namespace EngineeringKnowledgeHub.Application.Articles;

using EngineeringKnowledgeHub.Application.DTOs;

public interface IKnowledgeArticleService
{
    Task<KnowledgeArticleDto> CreateArticleAsync(CreateKnowledgeArticleCommand command, CancellationToken cancellationToken = default);
    Task<KnowledgeArticleDto> UpdateArticleAsync(UpdateKnowledgeArticleCommand command, CancellationToken cancellationToken = default);
    Task<KnowledgeArticleDto> GetArticleByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<KnowledgeArticleDto> SubmitForReviewAsync(Guid id, CancellationToken cancellationToken = default);
    Task<KnowledgeArticleDto> SubmitForReviewAsync(Guid id, Guid submitterId, CancellationToken cancellationToken = default);
    Task<KnowledgeArticleDto> ApproveArticleAsync(ApproveKnowledgeArticleCommand command, CancellationToken cancellationToken = default);
    Task<KnowledgeArticleDto> RejectArticleAsync(RejectKnowledgeArticleCommand command, CancellationToken cancellationToken = default);
    Task<KnowledgeArticleDto> DeprecateArticleAsync(Guid id, CancellationToken cancellationToken = default);
    Task<KnowledgeArticleDto> DeprecateArticleAsync(Guid id, Guid actorId, CancellationToken cancellationToken = default);
    Task<KnowledgeArticleDto> ArchiveArticleAsync(Guid id, CancellationToken cancellationToken = default);
    Task<KnowledgeArticleDto> ArchiveArticleAsync(Guid id, Guid actorId, CancellationToken cancellationToken = default);
    Task<KnowledgeArticleDto> AddTagAsync(Guid articleId, Guid tagId, CancellationToken cancellationToken = default);
    Task<KnowledgeArticleDto> RemoveTagAsync(Guid articleId, Guid tagId, CancellationToken cancellationToken = default);
    Task<KnowledgeArticleDto> AddProductAsync(Guid articleId, Guid productId, CancellationToken cancellationToken = default);
    Task<KnowledgeArticleDto> RemoveProductAsync(Guid articleId, Guid productId, CancellationToken cancellationToken = default);
    Task<KnowledgeArticleDto> AddTechnologyAsync(Guid articleId, Guid technologyId, CancellationToken cancellationToken = default);
    Task<KnowledgeArticleDto> RemoveTechnologyAsync(Guid articleId, Guid technologyId, CancellationToken cancellationToken = default);
}
