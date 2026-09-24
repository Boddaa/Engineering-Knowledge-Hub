namespace EngineeringKnowledgeHub.Application.Governance;

using EngineeringKnowledgeHub.Application.Common.Exceptions;
using EngineeringKnowledgeHub.Application.Common.Interfaces;
using EngineeringKnowledgeHub.Application.DTOs;
using EngineeringKnowledgeHub.Domain.Constants;
using EngineeringKnowledgeHub.Domain.Entities;

public sealed class KnowledgeGovernanceService(
    IKnowledgeVersionRepository versionRepository,
    IAuditLogRepository auditLogRepository,
    IKnowledgeArticleRepository articleRepository,
    IDateTimeProvider dateTimeProvider) : IKnowledgeGovernanceService
{
    public async Task<KnowledgeVersionDto> CreateVersionAsync(
        CreateKnowledgeVersionCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.ArticleId == Guid.Empty)
        {
            throw new ValidationException("ArticleId cannot be empty.");
        }

        if (command.ChangedById == Guid.Empty)
        {
            throw new ValidationException("ChangedById cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(command.ChangeSummary))
        {
            throw new ValidationException("ChangeSummary is required.");
        }

        var article = await articleRepository.GetByIdAsync(command.ArticleId, cancellationToken)
            ?? throw new NotFoundException("KnowledgeArticle", command.ArticleId);

        var title = string.IsNullOrWhiteSpace(command.Title) ? article.Title : command.Title;
        var content = string.IsNullOrWhiteSpace(command.Content) ? article.Content : command.Content;

        int versionNumber;
        if (command.VersionNumber.HasValue)
        {
            if (command.VersionNumber.Value <= 0)
            {
                throw new ValidationException("Version number must be greater than zero.");
            }

            if (await versionRepository.ExistsAsync(command.ArticleId, command.VersionNumber.Value, cancellationToken))
            {
                throw new ConflictException($"Version {command.VersionNumber.Value} already exists for article '{command.ArticleId}'.");
            }

            versionNumber = command.VersionNumber.Value;
        }
        else
        {
            var latest = await versionRepository.GetLatestVersionNumberAsync(command.ArticleId, cancellationToken);
            versionNumber = latest + 1;

            if (await versionRepository.ExistsAsync(command.ArticleId, versionNumber, cancellationToken))
            {
                throw new ConflictException($"Version {versionNumber} already exists for article '{command.ArticleId}'.");
            }
        }

        var now = dateTimeProvider.UtcNow;
        var version = new KnowledgeVersion(
            Guid.NewGuid(),
            command.ArticleId,
            versionNumber,
            title,
            content,
            command.ChangedById,
            command.ChangeSummary,
            now);

        await versionRepository.AddAsync(version, cancellationToken);

        var auditLog = new AuditLog(
            Guid.NewGuid(),
            command.ChangedById,
            GovernanceAuditActions.KnowledgeVersionCreated,
            nameof(KnowledgeVersion),
            version.Id,
            $"Version {version.VersionNumber} created for article '{command.ArticleId}'.",
            now);

        await auditLogRepository.AddAsync(auditLog, cancellationToken);

        return KnowledgeVersionDto.FromDomain(version);
    }

    public async Task<IReadOnlyList<KnowledgeVersionDto>> GetVersionHistoryAsync(
        Guid articleId,
        CancellationToken cancellationToken = default)
    {
        if (articleId == Guid.Empty)
        {
            throw new ValidationException("ArticleId cannot be empty.");
        }

        _ = await articleRepository.GetByIdAsync(articleId, cancellationToken)
            ?? throw new NotFoundException("KnowledgeArticle", articleId);

        var versions = await versionRepository.GetByArticleIdAsync(articleId, cancellationToken);

        return versions
            .OrderByDescending(v => v.VersionNumber)
            .ThenByDescending(v => v.CreatedAt)
            .Select(KnowledgeVersionDto.FromDomain)
            .ToList();
    }

    public async Task<KnowledgeVersionDto> GetVersionByNumberAsync(
        Guid articleId,
        int versionNumber,
        CancellationToken cancellationToken = default)
    {
        if (articleId == Guid.Empty)
        {
            throw new ValidationException("ArticleId cannot be empty.");
        }

        if (versionNumber <= 0)
        {
            throw new ValidationException("Version number must be greater than zero.");
        }

        _ = await articleRepository.GetByIdAsync(articleId, cancellationToken)
            ?? throw new NotFoundException("KnowledgeArticle", articleId);

        var version = await versionRepository.GetByArticleAndVersionNumberAsync(articleId, versionNumber, cancellationToken)
            ?? throw new NotFoundException("KnowledgeVersion", $"Article: {articleId}, Version: {versionNumber}");

        return KnowledgeVersionDto.FromDomain(version);
    }

    public async Task<KnowledgeVersionDto> GetVersionByIdAsync(
        Guid versionId,
        CancellationToken cancellationToken = default)
    {
        if (versionId == Guid.Empty)
        {
            throw new ValidationException("VersionId cannot be empty.");
        }

        var version = await versionRepository.GetByIdAsync(versionId, cancellationToken)
            ?? throw new NotFoundException("KnowledgeVersion", versionId);

        return KnowledgeVersionDto.FromDomain(version);
    }

    public async Task<AuditLogDto> RecordAuditLogAsync(
        RecordAuditLogCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.UserId == Guid.Empty)
        {
            throw new ValidationException("UserId cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(command.Action))
        {
            throw new ValidationException("Action is required.");
        }

        if (string.IsNullOrWhiteSpace(command.EntityName))
        {
            throw new ValidationException("EntityName is required.");
        }

        if (command.EntityId == Guid.Empty)
        {
            throw new ValidationException("EntityId cannot be empty.");
        }

        var now = dateTimeProvider.UtcNow;
        var audit = new AuditLog(
            Guid.NewGuid(),
            command.UserId,
            command.Action,
            command.EntityName,
            command.EntityId,
            command.Details,
            now);

        await auditLogRepository.AddAsync(audit, cancellationToken);

        return AuditLogDto.FromDomain(audit);
    }

    public async Task<IReadOnlyList<AuditLogDto>> GetAuditLogsByEntityAsync(
        string entityName,
        Guid entityId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(entityName))
        {
            throw new ValidationException("EntityName is required.");
        }

        if (entityId == Guid.Empty)
        {
            throw new ValidationException("EntityId cannot be empty.");
        }

        var logs = await auditLogRepository.GetByEntityAsync(entityName, entityId, cancellationToken);

        return logs
            .OrderByDescending(l => l.CreatedAt)
            .ThenByDescending(l => l.Id)
            .Select(AuditLogDto.FromDomain)
            .ToList();
    }

    public async Task<IReadOnlyList<AuditLogDto>> GetAuditLogsByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            throw new ValidationException("UserId cannot be empty.");
        }

        var logs = await auditLogRepository.GetByUserIdAsync(userId, cancellationToken);

        return logs
            .OrderByDescending(l => l.CreatedAt)
            .ThenByDescending(l => l.Id)
            .Select(AuditLogDto.FromDomain)
            .ToList();
    }

    public async Task<IReadOnlyList<AuditLogDto>> GetAllAuditLogsAsync(
        CancellationToken cancellationToken = default)
    {
        var logs = await auditLogRepository.GetAllAsync(cancellationToken);

        return logs
            .OrderByDescending(l => l.CreatedAt)
            .ThenByDescending(l => l.Id)
            .Select(AuditLogDto.FromDomain)
            .ToList();
    }
}
