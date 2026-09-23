namespace EngineeringKnowledgeHub.Domain.Exceptions;

/// <summary>
/// Thrown when a business rule or invariant in the domain is violated.
/// </summary>
public sealed class DomainRuleViolationException : DomainException
{
    public DomainRuleViolationException(string message) : base(message)
    {
    }
}
