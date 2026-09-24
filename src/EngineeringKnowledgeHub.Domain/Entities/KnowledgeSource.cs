namespace EngineeringKnowledgeHub.Domain.Entities;

using EngineeringKnowledgeHub.Domain.Enums;
using EngineeringKnowledgeHub.Domain.Exceptions;

/// <summary>
/// Origin or citation reference for technical knowledge (vendor documentation, incidents, external APIs, etc.).
/// </summary>
public sealed class KnowledgeSource
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public SourceType SourceType { get; private set; }
    public string Url { get; private set; }
    public string Description { get; private set; }
    public TrustLevel TrustLevel { get; private set; }
    public bool IsApproved { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    public KnowledgeSource(
        Guid id,
        string name,
        SourceType sourceType,
        DateTimeOffset createdAt,
        string? url = null,
        string? description = null,
        TrustLevel trustLevel = TrustLevel.Untrusted,
        bool isApproved = false)
    {
        if (id == Guid.Empty) throw new DomainRuleViolationException("KnowledgeSource Id cannot be empty.");
        if (string.IsNullOrWhiteSpace(name)) throw new DomainRuleViolationException("KnowledgeSource name cannot be null or whitespace.");
        if (!Enum.IsDefined(sourceType)) throw new DomainRuleViolationException($"Invalid SourceType: '{sourceType}'.");
        if (!Enum.IsDefined(trustLevel)) throw new DomainRuleViolationException($"Invalid TrustLevel: '{trustLevel}'.");

        var trimmedUrl = url?.Trim() ?? string.Empty;
        if (!string.IsNullOrEmpty(trimmedUrl) && !Uri.TryCreate(trimmedUrl, UriKind.Absolute, out _))
        {
            throw new DomainRuleViolationException("KnowledgeSource Url must be a valid absolute URI if provided.");
        }

        Id = id;
        Name = name.Trim();
        SourceType = sourceType;
        Url = trimmedUrl;
        Description = description?.Trim() ?? string.Empty;
        TrustLevel = trustLevel;
        IsApproved = isApproved;
        CreatedAt = createdAt;
    }

    public void Approve(DateTimeOffset? approvedAt = null)
    {
        IsApproved = true;
        if (TrustLevel == TrustLevel.Untrusted)
        {
            TrustLevel = TrustLevel.Verified;
        }

        if (approvedAt.HasValue)
        {
            UpdatedAt = approvedAt.Value;
        }
    }

    public void RevokeApproval(DateTimeOffset? revokedAt = null)
    {
        IsApproved = false;
        if (revokedAt.HasValue)
        {
            UpdatedAt = revokedAt.Value;
        }
    }

    public void UpdateTrustLevel(TrustLevel trustLevel, DateTimeOffset updatedAt)
    {
        if (!Enum.IsDefined(trustLevel)) throw new DomainRuleViolationException($"Invalid TrustLevel: '{trustLevel}'.");

        TrustLevel = trustLevel;
        UpdatedAt = updatedAt;
    }

    public void Update(string name, string? url, string? description, DateTimeOffset updatedAt)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainRuleViolationException("KnowledgeSource name cannot be null or whitespace.");

        var trimmedUrl = url?.Trim() ?? string.Empty;
        if (!string.IsNullOrEmpty(trimmedUrl) && !Uri.TryCreate(trimmedUrl, UriKind.Absolute, out _))
        {
            throw new DomainRuleViolationException("KnowledgeSource Url must be a valid absolute URI if provided.");
        }

        Name = name.Trim();
        Url = trimmedUrl;
        Description = description?.Trim() ?? string.Empty;
        UpdatedAt = updatedAt;
    }
}
