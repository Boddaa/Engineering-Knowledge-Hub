namespace EngineeringKnowledgeHub.Domain.Entities;

using EngineeringKnowledgeHub.Domain.Exceptions;
using EngineeringKnowledgeHub.Domain.ValueObjects;

/// <summary>
/// User identity entity representing an engineer, author, reviewer, or administrator.
/// Pure domain model decoupled from authentication and credential storage mechanisms.
/// </summary>
public sealed class User
{
    private readonly HashSet<Guid> _roleIds = [];

    public Guid Id { get; private set; }
    public string Username { get; private set; }
    public string DisplayName { get; private set; }
    public EmailAddress Email { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    public IReadOnlyCollection<Guid> RoleIds => _roleIds;

    public User(
        Guid id,
        string username,
        string displayName,
        EmailAddress email,
        DateTimeOffset createdAt,
        bool isActive = true)
    {
        if (id == Guid.Empty) throw new DomainRuleViolationException("User Id cannot be empty.");
        if (string.IsNullOrWhiteSpace(username)) throw new DomainRuleViolationException("Username cannot be null or whitespace.");
        if (string.IsNullOrWhiteSpace(displayName)) throw new DomainRuleViolationException("DisplayName cannot be null or whitespace.");

        Id = id;
        Username = username.Trim().ToLowerInvariant();
        DisplayName = displayName.Trim();
        Email = email ?? throw new DomainRuleViolationException("Email cannot be null.");
        CreatedAt = createdAt;
        IsActive = isActive;
    }

    public void UpdateProfile(string displayName, EmailAddress email, DateTimeOffset updatedAt)
    {
        if (string.IsNullOrWhiteSpace(displayName)) throw new DomainRuleViolationException("DisplayName cannot be null or whitespace.");

        DisplayName = displayName.Trim();
        Email = email ?? throw new DomainRuleViolationException("Email cannot be null.");
        UpdatedAt = updatedAt;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;

    public void AssignRole(Guid roleId)
    {
        if (roleId == Guid.Empty) throw new DomainRuleViolationException("Role Id cannot be empty.");
        _roleIds.Add(roleId);
    }

    public void RemoveRole(Guid roleId) => _roleIds.Remove(roleId);
}
