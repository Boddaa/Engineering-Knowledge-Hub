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

        Id = id;
        Name = name.Trim();
        SourceType = sourceType;
        Url = url?.Trim() ?? string.Empty;
        Description = description?.Trim() ?? string.Empty;
        TrustLevel = trustLevel;
        IsApproved = isApproved;
        CreatedAt = createdAt;
    }

    public void Approve()
    {
        IsApproved = true;
        if (TrustLevel == TrustLevel.Untrusted)
        {
            TrustLevel = TrustLevel.Verified;
        }
    }

    public void RevokeApproval()
    {
        IsApproved = false;
    }

    public void UpdateTrustLevel(TrustLevel trustLevel, DateTimeOffset updatedAt)
    {
        TrustLevel = trustLevel;
        UpdatedAt = updatedAt;
    }

    public void Update(string name, string? url, string? description, DateTimeOffset updatedAt)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainRuleViolationException("KnowledgeSource name cannot be null or whitespace.");

        Name = name.Trim();
        Url = url?.Trim() ?? string.Empty;
        Description = description?.Trim() ?? string.Empty;
        UpdatedAt = updatedAt;
    }
}
