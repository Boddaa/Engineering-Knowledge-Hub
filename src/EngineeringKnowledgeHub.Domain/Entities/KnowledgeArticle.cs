namespace EngineeringKnowledgeHub.Domain.Entities;

using EngineeringKnowledgeHub.Domain.Enums;
using EngineeringKnowledgeHub.Domain.Exceptions;
using EngineeringKnowledgeHub.Domain.ValueObjects;

/// <summary>
/// Core knowledge aggregate root representing human-readable technical documentation,
/// procedures, troubleshooting guides, configurations, and engineering notes.
/// </summary>
public sealed class KnowledgeArticle
{
    private readonly HashSet<Guid> _tagIds = [];
    private readonly HashSet<Guid> _productIds = [];
    private readonly HashSet<Guid> _technologyIds = [];
    private readonly HashSet<Guid> _sourceIds = [];

    public Guid Id { get; private set; }
    public string Title { get; private set; }
    public Slug Slug { get; private set; }
    public string Summary { get; private set; }
    public string Content { get; private set; }
    public Guid KnowledgeTypeId { get; private set; }
    public KnowledgeStatus Status { get; private set; }
    public VerificationStatus VerificationStatus { get; private set; }
    public Guid AuthorId { get; private set; }
    public Guid? ReviewerId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public DateTimeOffset? PublishedAt { get; private set; }
    public DateTimeOffset? LastReviewedAt { get; private set; }

    public IReadOnlyCollection<Guid> TagIds => _tagIds;
    public IReadOnlyCollection<Guid> ProductIds => _productIds;
    public IReadOnlyCollection<Guid> TechnologyIds => _technologyIds;
    public IReadOnlyCollection<Guid> SourceIds => _sourceIds;

    public KnowledgeArticle(
        Guid id,
        string title,
        Slug slug,
        string summary,
        string content,
        Guid knowledgeTypeId,
        Guid authorId,
        DateTimeOffset createdAt)
    {
        if (id == Guid.Empty) throw new DomainRuleViolationException("Article Id cannot be empty.");
        if (authorId == Guid.Empty) throw new DomainRuleViolationException("Author Id cannot be empty.");
        if (knowledgeTypeId == Guid.Empty) throw new DomainRuleViolationException("Knowledge Type Id cannot be empty.");

        ValidateTitleAndContent(title, content);

        Id = id;
        Title = title.Trim();
        Slug = slug ?? throw new DomainRuleViolationException("Slug cannot be null.");
        Summary = summary?.Trim() ?? string.Empty;
        Content = content;
        KnowledgeTypeId = knowledgeTypeId;
        AuthorId = authorId;
        Status = KnowledgeStatus.Draft;
        VerificationStatus = VerificationStatus.Unverified;
        CreatedAt = createdAt;
    }

    public void UpdateContent(string title, Slug slug, string summary, string content, DateTimeOffset updatedAt)
    {
        ValidateTitleAndContent(title, content);

        Title = title.Trim();
        Slug = slug ?? throw new DomainRuleViolationException("Slug cannot be null.");
        Summary = summary?.Trim() ?? string.Empty;
        Content = content;
        UpdatedAt = updatedAt;

        // Modifying published or verified knowledge requires re-verification
        if (Status is KnowledgeStatus.Published or KnowledgeStatus.Verified)
        {
            Status = KnowledgeStatus.Draft;
            VerificationStatus = VerificationStatus.Unverified;
        }
    }

    public void SubmitForReview()
    {
        if (Status is not (KnowledgeStatus.Draft or KnowledgeStatus.Deprecated))
        {
            throw new DomainRuleViolationException($"Cannot submit for review from status '{Status}'. Only Draft or Deprecated articles can be submitted.");
        }

        Status = KnowledgeStatus.PendingReview;
    }

    public void Approve(Guid reviewerId, DateTimeOffset reviewedAt)
    {
        if (reviewerId == Guid.Empty)
        {
            throw new DomainRuleViolationException("Reviewer Id cannot be empty.");
        }

        if (reviewerId == AuthorId)
        {
            throw new DomainRuleViolationException("Author cannot approve their own knowledge article. Peer review is mandatory.");
        }

        if (Status != KnowledgeStatus.PendingReview)
        {
            throw new DomainRuleViolationException($"Cannot approve an article with status '{Status}'. Must be PendingReview.");
        }

        ReviewerId = reviewerId;
        LastReviewedAt = reviewedAt;
        PublishedAt = PublishedAt ?? reviewedAt;
        Status = KnowledgeStatus.Published;
        VerificationStatus = VerificationStatus.Verified;
    }

    public void Reject(Guid reviewerId, DateTimeOffset reviewedAt)
    {
        if (reviewerId == Guid.Empty)
        {
            throw new DomainRuleViolationException("Reviewer Id cannot be empty.");
        }

        if (reviewerId == AuthorId)
        {
            throw new DomainRuleViolationException("Author cannot review/reject their own article.");
        }

        if (Status != KnowledgeStatus.PendingReview)
        {
            throw new DomainRuleViolationException($"Cannot reject an article with status '{Status}'. Must be PendingReview.");
        }

        ReviewerId = reviewerId;
        LastReviewedAt = reviewedAt;
        Status = KnowledgeStatus.Draft;
        VerificationStatus = VerificationStatus.Rejected;
    }

    public void Deprecate()
    {
        if (Status != KnowledgeStatus.Published)
        {
            throw new DomainRuleViolationException($"Cannot deprecate article in status '{Status}'. Only Published articles can be deprecated.");
        }

        Status = KnowledgeStatus.Deprecated;
    }

    public void Archive()
    {
        Status = KnowledgeStatus.Archived;
    }

    public void AddTag(Guid tagId)
    {
        if (tagId == Guid.Empty) throw new DomainRuleViolationException("Tag Id cannot be empty.");
        _tagIds.Add(tagId);
    }

    public void RemoveTag(Guid tagId) => _tagIds.Remove(tagId);

    public void AddProduct(Guid productId)
    {
        if (productId == Guid.Empty) throw new DomainRuleViolationException("Product Id cannot be empty.");
        _productIds.Add(productId);
    }

    public void RemoveProduct(Guid productId) => _productIds.Remove(productId);

    public void AddTechnology(Guid technologyId)
    {
        if (technologyId == Guid.Empty) throw new DomainRuleViolationException("Technology Id cannot be empty.");
        _technologyIds.Add(technologyId);
    }

    public void RemoveTechnology(Guid technologyId) => _technologyIds.Remove(technologyId);

    public void AddSource(Guid sourceId)
    {
        if (sourceId == Guid.Empty) throw new DomainRuleViolationException("Source Id cannot be empty.");
        _sourceIds.Add(sourceId);
    }

    public void RemoveSource(Guid sourceId) => _sourceIds.Remove(sourceId);

    private static void ValidateTitleAndContent(string title, string content)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainRuleViolationException("Article title cannot be null or whitespace.");
        }

        if (title.Trim().Length > 200)
        {
            throw new DomainRuleViolationException("Article title exceeds maximum length of 200 characters.");
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new DomainRuleViolationException("Article content cannot be null or whitespace.");
        }
    }
}
