namespace EngineeringKnowledgeHub.Domain.Entities;

using EngineeringKnowledgeHub.Domain.Exceptions;

/// <summary>
/// Immutable historical snapshot of a published knowledge article.
/// Captures content, metadata, actor, and summary of change at a specific version point.
/// </summary>
public sealed class KnowledgeVersion
{
    public Guid Id { get; private set; }
    public Guid ArticleId { get; private set; }
    public int VersionNumber { get; private set; }
    public string Title { get; private set; }
    public string Content { get; private set; }
    public Guid ChangedById { get; private set; }
    public string ChangeSummary { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public KnowledgeVersion(
        Guid id,
        Guid articleId,
        int versionNumber,
        string title,
        string content,
        Guid changedById,
        string changeSummary,
        DateTimeOffset createdAt)
    {
        if (id == Guid.Empty)
        {
            throw new DomainRuleViolationException("Version Id cannot be empty.");
        }

        if (articleId == Guid.Empty)
        {
            throw new DomainRuleViolationException("Article Id cannot be empty.");
        }

        if (versionNumber <= 0)
        {
            throw new DomainRuleViolationException("Version number must be greater than zero.");
        }

        if (changedById == Guid.Empty)
        {
            throw new DomainRuleViolationException("ChangedBy user Id cannot be empty.");
        }

        ValidateTitleAndContent(title, content);

        Id = id;
        ArticleId = articleId;
        VersionNumber = versionNumber;
        Title = title.Trim();
        Content = content;
        ChangedById = changedById;
        ChangeSummary = changeSummary?.Trim() ?? string.Empty;
        CreatedAt = createdAt;
    }

    private static void ValidateTitleAndContent(string title, string content)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainRuleViolationException("Version title cannot be null or whitespace.");
        }

        if (title.Trim().Length > 200)
        {
            throw new DomainRuleViolationException("Version title exceeds maximum length of 200 characters.");
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new DomainRuleViolationException("Version content cannot be null or whitespace.");
        }
    }
}
