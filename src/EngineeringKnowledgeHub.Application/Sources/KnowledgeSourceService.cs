namespace EngineeringKnowledgeHub.Application.Sources;

using EngineeringKnowledgeHub.Application.Common.Exceptions;
using EngineeringKnowledgeHub.Application.Common.Interfaces;
using EngineeringKnowledgeHub.Application.DTOs;
using EngineeringKnowledgeHub.Domain.Entities;
using EngineeringKnowledgeHub.Domain.Enums;

public sealed class KnowledgeSourceService(
    IKnowledgeSourceRepository sourceRepository,
    IKnowledgeArticleRepository articleRepository,
    IDateTimeProvider dateTimeProvider) : IKnowledgeSourceService
{
    public async Task<KnowledgeSourceDto> CreateSourceAsync(
        CreateKnowledgeSourceCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.Name))
        {
            throw new ValidationException("Source name is required.");
        }

        if (!Enum.IsDefined(command.SourceType))
        {
            throw new ValidationException($"Invalid SourceType: '{command.SourceType}'.");
        }

        if (!Enum.IsDefined(command.TrustLevel))
        {
            throw new ValidationException($"Invalid TrustLevel: '{command.TrustLevel}'.");
        }

        if (!string.IsNullOrWhiteSpace(command.Url) && !Uri.TryCreate(command.Url.Trim(), UriKind.Absolute, out _))
        {
            throw new ValidationException("Source Url must be a valid absolute URI if provided.");
        }

        if (await sourceRepository.ExistsByNameAsync(command.Name, cancellationToken))
        {
            throw new ConflictException($"A knowledge source with name '{command.Name.Trim()}' already exists.");
        }

        var now = dateTimeProvider.UtcNow;
        var source = new KnowledgeSource(
            Guid.NewGuid(),
            command.Name,
            command.SourceType,
            now,
            command.Url,
            command.Description,
            command.TrustLevel,
            isApproved: false);

        await sourceRepository.AddAsync(source, cancellationToken);

        return KnowledgeSourceDto.FromDomain(source);
    }

    public async Task<KnowledgeSourceDto> UpdateSourceAsync(
        UpdateKnowledgeSourceCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.Id == Guid.Empty)
        {
            throw new ValidationException("Source Id cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(command.Name))
        {
            throw new ValidationException("Source name is required.");
        }

        if (!string.IsNullOrWhiteSpace(command.Url) && !Uri.TryCreate(command.Url.Trim(), UriKind.Absolute, out _))
        {
            throw new ValidationException("Source Url must be a valid absolute URI if provided.");
        }

        var source = await sourceRepository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new NotFoundException("KnowledgeSource", command.Id);

        if (!string.Equals(source.Name, command.Name.Trim(), StringComparison.OrdinalIgnoreCase)
            && await sourceRepository.ExistsByNameAsync(command.Name, cancellationToken))
        {
            throw new ConflictException($"A knowledge source with name '{command.Name.Trim()}' already exists.");
        }

        var now = dateTimeProvider.UtcNow;
        source.Update(command.Name, command.Url, command.Description, now);

        await sourceRepository.UpdateAsync(source, cancellationToken);

        return KnowledgeSourceDto.FromDomain(source);
    }

    public async Task<KnowledgeSourceDto> GetSourceByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            throw new ValidationException("Source Id cannot be empty.");
        }

        var source = await sourceRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("KnowledgeSource", id);

        return KnowledgeSourceDto.FromDomain(source);
    }

    public async Task<IReadOnlyList<KnowledgeSourceDto>> GetSourcesAsync(
        SourceType? sourceType = null,
        TrustLevel? trustLevel = null,
        bool? isApproved = null,
        CancellationToken cancellationToken = default)
    {
        var sources = await sourceRepository.GetFilteredAsync(sourceType, trustLevel, isApproved, cancellationToken);
        return sources.Select(KnowledgeSourceDto.FromDomain).ToList();
    }

    public async Task<KnowledgeSourceDto> ApproveSourceAsync(
        Guid sourceId,
        CancellationToken cancellationToken = default)
    {
        if (sourceId == Guid.Empty)
        {
            throw new ValidationException("Source Id cannot be empty.");
        }

        var source = await sourceRepository.GetByIdAsync(sourceId, cancellationToken)
            ?? throw new NotFoundException("KnowledgeSource", sourceId);

        var now = dateTimeProvider.UtcNow;
        source.Approve(now);

        await sourceRepository.UpdateAsync(source, cancellationToken);

        return KnowledgeSourceDto.FromDomain(source);
    }

    public async Task<KnowledgeSourceDto> RevokeSourceApprovalAsync(
        Guid sourceId,
        CancellationToken cancellationToken = default)
    {
        if (sourceId == Guid.Empty)
        {
            throw new ValidationException("Source Id cannot be empty.");
        }

        var source = await sourceRepository.GetByIdAsync(sourceId, cancellationToken)
            ?? throw new NotFoundException("KnowledgeSource", sourceId);

        var now = dateTimeProvider.UtcNow;
        source.RevokeApproval(now);

        await sourceRepository.UpdateAsync(source, cancellationToken);

        return KnowledgeSourceDto.FromDomain(source);
    }

    public async Task<KnowledgeSourceDto> UpdateTrustLevelAsync(
        UpdateTrustLevelCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.Id == Guid.Empty)
        {
            throw new ValidationException("Source Id cannot be empty.");
        }

        if (!Enum.IsDefined(command.TrustLevel))
        {
            throw new ValidationException($"Invalid TrustLevel: '{command.TrustLevel}'.");
        }

        var source = await sourceRepository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new NotFoundException("KnowledgeSource", command.Id);

        var now = dateTimeProvider.UtcNow;
        source.UpdateTrustLevel(command.TrustLevel, now);

        await sourceRepository.UpdateAsync(source, cancellationToken);

        return KnowledgeSourceDto.FromDomain(source);
    }

    public async Task<KnowledgeArticleDto> AttachSourceToArticleAsync(
        AttachArticleSourceCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.ArticleId == Guid.Empty)
        {
            throw new ValidationException("ArticleId is required.");
        }

        if (command.SourceId == Guid.Empty)
        {
            throw new ValidationException("SourceId is required.");
        }

        var article = await articleRepository.GetByIdAsync(command.ArticleId, cancellationToken)
            ?? throw new NotFoundException("KnowledgeArticle", command.ArticleId);

        var source = await sourceRepository.GetByIdAsync(command.SourceId, cancellationToken)
            ?? throw new NotFoundException("KnowledgeSource", command.SourceId);

        if (article.SourceIds.Contains(command.SourceId))
        {
            throw new ConflictException($"Source '{command.SourceId}' is already attached to article '{command.ArticleId}'.");
        }

        article.AddSource(source.Id, command.ReferenceText);

        await articleRepository.UpdateAsync(article, cancellationToken);

        return KnowledgeArticleDto.FromDomain(article);
    }

    public async Task<KnowledgeArticleDto> DetachSourceFromArticleAsync(
        DetachArticleSourceCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.ArticleId == Guid.Empty)
        {
            throw new ValidationException("ArticleId is required.");
        }

        if (command.SourceId == Guid.Empty)
        {
            throw new ValidationException("SourceId is required.");
        }

        var article = await articleRepository.GetByIdAsync(command.ArticleId, cancellationToken)
            ?? throw new NotFoundException("KnowledgeArticle", command.ArticleId);

        var source = await sourceRepository.GetByIdAsync(command.SourceId, cancellationToken)
            ?? throw new NotFoundException("KnowledgeSource", command.SourceId);

        if (!article.SourceIds.Contains(command.SourceId))
        {
            throw new NotFoundException($"Source '{command.SourceId}' is not attached to article '{command.ArticleId}'.");
        }

        article.RemoveSource(source.Id);

        await articleRepository.UpdateAsync(article, cancellationToken);

        return KnowledgeArticleDto.FromDomain(article);
    }

    public async Task<IReadOnlyList<KnowledgeSourceDto>> GetSourcesByArticleIdAsync(
        Guid articleId,
        CancellationToken cancellationToken = default)
    {
        if (articleId == Guid.Empty)
        {
            throw new ValidationException("ArticleId is required.");
        }

        var article = await articleRepository.GetByIdAsync(articleId, cancellationToken)
            ?? throw new NotFoundException("KnowledgeArticle", articleId);

        var list = new List<KnowledgeSourceDto>();
        foreach (var sourceId in article.SourceIds)
        {
            var source = await sourceRepository.GetByIdAsync(sourceId, cancellationToken);
            if (source is not null)
            {
                list.Add(KnowledgeSourceDto.FromDomain(source));
            }
        }

        return list;
    }
}
