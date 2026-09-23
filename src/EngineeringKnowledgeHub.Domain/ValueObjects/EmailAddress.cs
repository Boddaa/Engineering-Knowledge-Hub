namespace EngineeringKnowledgeHub.Domain.ValueObjects;

using System.Text.RegularExpressions;
using EngineeringKnowledgeHub.Domain.Exceptions;

/// <summary>
/// Represents a validated, normalized email address for a user.
/// </summary>
public sealed class EmailAddress : IEquatable<EmailAddress>
{
    private static readonly Regex EmailPattern = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public const int MaxLength = 256;

    public string Value { get; }

    private EmailAddress(string value)
    {
        Value = value;
    }

    public static EmailAddress Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new DomainRuleViolationException("Email address cannot be null or whitespace.");
        }

        var normalized = email.Trim().ToLowerInvariant();

        if (normalized.Length > MaxLength)
        {
            throw new DomainRuleViolationException($"Email address exceeds maximum length of {MaxLength} characters.");
        }

        if (!EmailPattern.IsMatch(normalized))
        {
            throw new DomainRuleViolationException($"Invalid email address format: '{email}'.");
        }

        return new EmailAddress(normalized);
    }

    public bool Equals(EmailAddress? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return string.Equals(Value, other.Value, StringComparison.Ordinal);
    }

    public override bool Equals(object? obj) => Equals(obj as EmailAddress);

    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);

    public override string ToString() => Value;

    public static implicit operator string(EmailAddress email) => email.Value;

    public static bool operator ==(EmailAddress? left, EmailAddress? right) => Equals(left, right);

    public static bool operator !=(EmailAddress? left, EmailAddress? right) => !Equals(left, right);
}
