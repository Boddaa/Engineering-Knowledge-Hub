namespace EngineeringKnowledgeHub.Application.Articles;

using EngineeringKnowledgeHub.Application.Common.Exceptions;
using EngineeringKnowledgeHub.Application.Common.Interfaces;
using EngineeringKnowledgeHub.Application.DTOs;
using EngineeringKnowledgeHub.Domain.Constants;
using EngineeringKnowledgeHub.Domain.Entities;
using EngineeringKnowledgeHub.Domain.ValueObjects;

public sealed class KnowledgeArticleService(
    IKnowledgeArticleRepository articleRepository,
    IKnowledgeTypeRepository typeRepository,
    IKnowledgeTagRepository tagRepository,
    IProductRepository productRepository,
    ITechnologyRepository technologyRepository,
    IDateTimeProvider dateTimeProvider,
    IAuditLogRepository auditLogRepository,
    IKnowledgeVersionRepository versionRepository,
    IUnitOfWork? unitOfWork = null) : IKnowledgeArticleService
{
    public async Task<KnowledgeArticleDto> CreateArticleAsync(
        CreateKnowledgeArticleCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.Title))
        {
            throw new ValidationException("Article title is required.");
        }

        if (command.KnowledgeTypeId == Guid.Empty)
        {
            throw new ValidationException("KnowledgeTypeId is required.");
        }

        if (command.AuthorId == Guid.Empty)
        {
            throw new ValidationException("AuthorId is required.");
        }

        var type = await typeRepository.GetByIdAsync(command.KnowledgeTypeId, cancellationToken);
        if (type is null)
        {
            throw new NotFoundException("KnowledgeType", command.KnowledgeTypeId);
        }

        var slug = string.IsNullOrWhiteSpace(command.Slug)
            ? Slug.FromTitle(command.Title)
            : Slug.Create(command.Slug);

        if (await articleRepository.ExistsBySlugAsync(slug, cancellationToken))
        {
            throw new ConflictException($"An article with slug '{slug.Value}' already exists.");
        }

        var now = dateTimeProvider.UtcNow;
        var article = new KnowledgeArticle(
            Guid.NewGuid(),
            command.Title,
            slug,
            command.Summary,
            command.Content,
            command.KnowledgeTypeId,
            command.AuthorId,
            now);

        await articleRepository.AddAsync(article, cancellationToken);

        return KnowledgeArticleDto.FromDomain(article);
    }

    public async Task<KnowledgeArticleDto> UpdateArticleAsync(
        UpdateKnowledgeArticleCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.Id == Guid.Empty)
        {
            throw new ValidationException("Article Id cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(command.Title))
        {
            throw new ValidationException("Article title is required.");
        }

        var article = await articleRepository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new NotFoundException("KnowledgeArticle", command.Id);

        var slug = string.IsNullOrWhiteSpace(command.Slug)
            ? Slug.FromTitle(command.Title)
            : Slug.Create(command.Slug);

        if (slug != article.Slug && await articleRepository.ExistsBySlugAsync(slug, cancellationToken))
        {
            throw new ConflictException($"An article with slug '{slug.Value}' already exists.");
        }

        var now = dateTimeProvider.UtcNow;
        article.UpdateContent(command.Title, slug, command.Summary, command.Content, now);

        await articleRepository.UpdateAsync(article, cancellationToken);

        return KnowledgeArticleDto.FromDomain(article);
    }

    public async Task<KnowledgeArticleDto> GetArticleByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty) throw new ValidationException("Article Id cannot be empty.");

        var article = await articleRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("KnowledgeArticle", id);

        return KnowledgeArticleDto.FromDomain(article);
    }

    public Task<KnowledgeArticleDto> SubmitForReviewAsync(
        Guid id,
        CancellationToken cancellationToken = default) =>
        SubmitForReviewAsync(id, Guid.Empty, cancellationToken);

    public async Task<KnowledgeArticleDto> SubmitForReviewAsync(
        Guid id,
        Guid submitterId,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty) throw new ValidationException("Article Id cannot be empty.");

        var article = await articleRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("KnowledgeArticle", id);

        article.SubmitForReview();

        await articleRepository.UpdateAsync(article, cancellationToken);

        var actor = submitterId != Guid.Empty ? submitterId : article.AuthorId;
        var now = dateTimeProvider.UtcNow;
        var audit = new AuditLog(
            Guid.NewGuid(),
            actor,
            GovernanceAuditActions.ArticleSubmittedForReview,
            nameof(KnowledgeArticle),
            article.Id,
            "Article submitted for review.",
            now);

        await auditLogRepository.AddAsync(audit, cancellationToken);

        return KnowledgeArticleDto.FromDomain(article);
    }

    public async Task<KnowledgeArticleDto> ApproveArticleAsync(
        ApproveKnowledgeArticleCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.ArticleId == Guid.Empty) throw new ValidationException("ArticleId is required.");
        if (command.ReviewerId == Guid.Empty) throw new ValidationException("ReviewerId is required.");

        if (unitOfWork is not null)
        {
            return await unitOfWork.ExecuteTransactionAsync(ct => ExecuteApproveCoreAsync(command, ct), cancellationToken);
        }

        return await ExecuteApproveCoreAsync(command, cancellationToken);
    }

    private async Task<KnowledgeArticleDto> ExecuteApproveCoreAsync(
        ApproveKnowledgeArticleCommand command,
        CancellationToken cancellationToken)
    {
        var article = await articleRepository.GetByIdAsync(command.ArticleId, cancellationToken)
            ?? throw new NotFoundException("KnowledgeArticle", command.ArticleId);

        var now = dateTimeProvider.UtcNow;
        article.Approve(command.ReviewerId, now);

        await articleRepository.UpdateAsync(article, cancellationToken);

        var audit = new AuditLog(
            Guid.NewGuid(),
            command.ReviewerId,
            GovernanceAuditActions.ArticleApproved,
            nameof(KnowledgeArticle),
            article.Id,
            $"Article approved by reviewer '{command.ReviewerId}'.",
            now);

        await auditLogRepository.AddAsync(audit, cancellationToken);

        var latest = await versionRepository.GetLatestVersionNumberAsync(article.Id, cancellationToken);
        var nextVersion = latest + 1;

        if (await versionRepository.ExistsAsync(article.Id, nextVersion, cancellationToken))
        {
            throw new ConflictException($"Version {nextVersion} already exists for article '{article.Id}'.");
        }

        var changeSummary = string.IsNullOrWhiteSpace(command.ChangeSummary) ? "Approved and published." : command.ChangeSummary.Trim();
        var version = new KnowledgeVersion(
            Guid.NewGuid(),
            article.Id,
            nextVersion,
            article.Title,
            article.Content,
            command.ReviewerId,
            changeSummary,
            now);

        await versionRepository.AddAsync(version, cancellationToken);

        var versionAudit = new AuditLog(
            Guid.NewGuid(),
            command.ReviewerId,
            GovernanceAuditActions.KnowledgeVersionCreated,
            nameof(KnowledgeVersion),
            version.Id,
            $"Version {version.VersionNumber} created for article '{article.Id}'.",
            now);

        await auditLogRepository.AddAsync(versionAudit, cancellationToken);

        return KnowledgeArticleDto.FromDomain(article);
    }

    public async Task<KnowledgeArticleDto> RejectArticleAsync(
        RejectKnowledgeArticleCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.ArticleId == Guid.Empty) throw new ValidationException("ArticleId is required.");
        if (command.ReviewerId == Guid.Empty) throw new ValidationException("ReviewerId is required.");

        var article = await articleRepository.GetByIdAsync(command.ArticleId, cancellationToken)
            ?? throw new NotFoundException("KnowledgeArticle", command.ArticleId);

        var now = dateTimeProvider.UtcNow;
        article.Reject(command.ReviewerId, now);

        await articleRepository.UpdateAsync(article, cancellationToken);

        var audit = new AuditLog(
            Guid.NewGuid(),
            command.ReviewerId,
            GovernanceAuditActions.ArticleRejected,
            nameof(KnowledgeArticle),
            article.Id,
            $"Article rejected by reviewer '{command.ReviewerId}'.",
            now);

        await auditLogRepository.AddAsync(audit, cancellationToken);

        return KnowledgeArticleDto.FromDomain(article);
    }

    public Task<KnowledgeArticleDto> DeprecateArticleAsync(
        Guid id,
        CancellationToken cancellationToken = default) =>
        DeprecateArticleAsync(id, Guid.Empty, cancellationToken);

    public async Task<KnowledgeArticleDto> DeprecateArticleAsync(
        Guid id,
        Guid actorId,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty) throw new ValidationException("Article Id cannot be empty.");

        var article = await articleRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("KnowledgeArticle", id);

        article.Deprecate();

        await articleRepository.UpdateAsync(article, cancellationToken);

        var actor = actorId != Guid.Empty ? actorId : (article.ReviewerId ?? article.AuthorId);
        var now = dateTimeProvider.UtcNow;
        var audit = new AuditLog(
            Guid.NewGuid(),
            actor,
            GovernanceAuditActions.ArticleDeprecated,
            nameof(KnowledgeArticle),
            article.Id,
            $"Article deprecated by actor '{actor}'.",
            now);

        await auditLogRepository.AddAsync(audit, cancellationToken);

        return KnowledgeArticleDto.FromDomain(article);
    }

    public Task<KnowledgeArticleDto> ArchiveArticleAsync(
        Guid id,
        CancellationToken cancellationToken = default) =>
        ArchiveArticleAsync(id, Guid.Empty, cancellationToken);

    public async Task<KnowledgeArticleDto> ArchiveArticleAsync(
        Guid id,
        Guid actorId,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty) throw new ValidationException("Article Id cannot be empty.");

        var article = await articleRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("KnowledgeArticle", id);

        article.Archive();

        await articleRepository.UpdateAsync(article, cancellationToken);

        var actor = actorId != Guid.Empty ? actorId : article.AuthorId;
        var now = dateTimeProvider.UtcNow;
        var audit = new AuditLog(
            Guid.NewGuid(),
            actor,
            GovernanceAuditActions.ArticleArchived,
            nameof(KnowledgeArticle),
            article.Id,
            $"Article archived by actor '{actor}'.",
            now);

        await auditLogRepository.AddAsync(audit, cancellationToken);

        return KnowledgeArticleDto.FromDomain(article);
    }

    public async Task<KnowledgeArticleDto> AddTagAsync(
        Guid articleId,
        Guid tagId,
        CancellationToken cancellationToken = default)
    {
        if (articleId == Guid.Empty) throw new ValidationException("ArticleId is required.");
        if (tagId == Guid.Empty) throw new ValidationException("TagId is required.");

        var article = await articleRepository.GetByIdAsync(articleId, cancellationToken)
            ?? throw new NotFoundException("KnowledgeArticle", articleId);

        var tag = await tagRepository.GetByIdAsync(tagId, cancellationToken)
            ?? throw new NotFoundException("KnowledgeTag", tagId);

        article.AddTag(tag.Id);

        await articleRepository.UpdateAsync(article, cancellationToken);

        return KnowledgeArticleDto.FromDomain(article);
    }

    public async Task<KnowledgeArticleDto> RemoveTagAsync(
        Guid articleId,
        Guid tagId,
        CancellationToken cancellationToken = default)
    {
        if (articleId == Guid.Empty) throw new ValidationException("ArticleId is required.");
        if (tagId == Guid.Empty) throw new ValidationException("TagId is required.");

        var article = await articleRepository.GetByIdAsync(articleId, cancellationToken)
            ?? throw new NotFoundException("KnowledgeArticle", articleId);

        article.RemoveTag(tagId);

        await articleRepository.UpdateAsync(article, cancellationToken);

        return KnowledgeArticleDto.FromDomain(article);
    }

    public async Task<KnowledgeArticleDto> AddProductAsync(
        Guid articleId,
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        if (articleId == Guid.Empty) throw new ValidationException("ArticleId is required.");
        if (productId == Guid.Empty) throw new ValidationException("ProductId is required.");

        var article = await articleRepository.GetByIdAsync(articleId, cancellationToken)
            ?? throw new NotFoundException("KnowledgeArticle", articleId);

        var product = await productRepository.GetByIdAsync(productId, cancellationToken)
            ?? throw new NotFoundException("Product", productId);

        article.AddProduct(product.Id);

        await articleRepository.UpdateAsync(article, cancellationToken);

        return KnowledgeArticleDto.FromDomain(article);
    }

    public async Task<KnowledgeArticleDto> RemoveProductAsync(
        Guid articleId,
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        if (articleId == Guid.Empty) throw new ValidationException("ArticleId is required.");
        if (productId == Guid.Empty) throw new ValidationException("ProductId is required.");

        var article = await articleRepository.GetByIdAsync(articleId, cancellationToken)
            ?? throw new NotFoundException("KnowledgeArticle", articleId);

        article.RemoveProduct(productId);

        await articleRepository.UpdateAsync(article, cancellationToken);

        return KnowledgeArticleDto.FromDomain(article);
    }

    public async Task<KnowledgeArticleDto> AddTechnologyAsync(
        Guid articleId,
        Guid technologyId,
        CancellationToken cancellationToken = default)
    {
        if (articleId == Guid.Empty) throw new ValidationException("ArticleId is required.");
        if (technologyId == Guid.Empty) throw new ValidationException("TechnologyId is required.");

        var article = await articleRepository.GetByIdAsync(articleId, cancellationToken)
            ?? throw new NotFoundException("KnowledgeArticle", articleId);

        var technology = await technologyRepository.GetByIdAsync(technologyId, cancellationToken)
            ?? throw new NotFoundException("Technology", technologyId);

        article.AddTechnology(technology.Id);

        await articleRepository.UpdateAsync(article, cancellationToken);

        return KnowledgeArticleDto.FromDomain(article);
    }

    public async Task<KnowledgeArticleDto> RemoveTechnologyAsync(
        Guid articleId,
        Guid technologyId,
        CancellationToken cancellationToken = default)
    {
        if (articleId == Guid.Empty) throw new ValidationException("ArticleId is required.");
        if (technologyId == Guid.Empty) throw new ValidationException("TechnologyId is required.");

        var article = await articleRepository.GetByIdAsync(articleId, cancellationToken)
            ?? throw new NotFoundException("KnowledgeArticle", articleId);

        article.RemoveTechnology(technologyId);

        await articleRepository.UpdateAsync(article, cancellationToken);

        return KnowledgeArticleDto.FromDomain(article);
    }
}
