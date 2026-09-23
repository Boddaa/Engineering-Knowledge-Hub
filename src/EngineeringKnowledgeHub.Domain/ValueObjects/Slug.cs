namespace EngineeringKnowledgeHub.Domain.ValueObjects;

using System.Text;
using System.Text.RegularExpressions;
using EngineeringKnowledgeHub.Domain.Exceptions;

/// <summary>
/// Represents a URL-friendly, unique slug for a knowledge article.
/// Fully supports Unicode alphanumeric characters (including Arabic and Latin scripts).
/// </summary>
public sealed class Slug : IEquatable<Slug>
{
    private static readonly Regex SlugPattern = new(@"^[\p{L}0-9]+(?:-[\p{L}0-9]+)*$", RegexOptions.Compiled);
    public const int MaxLength = 200;

    public string Value { get; }

    private Slug(string value)
    {
        Value = value;
    }

    public static Slug Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainRuleViolationException("Slug cannot be null or whitespace.");
        }

        var normalized = value.Trim().ToLowerInvariant();

        if (normalized.Length > MaxLength)
        {
            throw new DomainRuleViolationException($"Slug exceeds maximum length of {MaxLength} characters.");
        }

        if (!SlugPattern.IsMatch(normalized))
        {
            throw new DomainRuleViolationException(
                $"Invalid slug format: '{value}'. Slugs must contain only lowercase letters (including Arabic/Unicode) and numbers separated by single hyphens.");
        }

        return new Slug(normalized);
    }

    public static Slug FromTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainRuleViolationException("Cannot generate slug from empty title.");
        }

        var sb = new StringBuilder();
        var prevDash = false;

        foreach (var c in title.Trim().ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(c))
            {
                sb.Append(c);
                prevDash = false;
            }
            else
            {
                if (!prevDash && sb.Length > 0)
                {
                    sb.Append('-');
                    prevDash = true;
                }
            }
        }

        var slugStr = sb.ToString().TrimEnd('-');

        if (string.IsNullOrEmpty(slugStr))
        {
            throw new DomainRuleViolationException("Generated slug resulted in an empty string.");
        }

        if (slugStr.Length > MaxLength)
        {
            slugStr = slugStr[..MaxLength].TrimEnd('-');
        }

        return Create(slugStr);
    }

    public bool Equals(Slug? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return string.Equals(Value, other.Value, StringComparison.Ordinal);
    }

    public override bool Equals(object? obj) => Equals(obj as Slug);

    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);

    public override string ToString() => Value;

    public static implicit operator string(Slug slug) => slug.Value;

    public static bool operator ==(Slug? left, Slug? right) => Equals(left, right);

    public static bool operator !=(Slug? left, Slug? right) => !Equals(left, right);
}
