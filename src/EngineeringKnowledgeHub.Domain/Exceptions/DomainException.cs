namespace EngineeringKnowledgeHub.Domain.Exceptions;

/// <summary>
/// Base exception for all domain-level exceptions in Engineering Knowledge Hub.
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message)
    {
    }

    protected DomainException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
