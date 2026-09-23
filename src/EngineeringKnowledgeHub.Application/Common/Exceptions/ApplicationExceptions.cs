namespace EngineeringKnowledgeHub.Application.Common.Exceptions;

/// <summary>
/// Thrown when a requested entity cannot be found.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }

    public NotFoundException(string entityName, object key)
        : base($"Entity '{entityName}' with key ({key}) was not found.")
    {
    }
}

/// <summary>
/// Thrown when an operation violates a uniqueness or state conflict.
/// </summary>
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message)
    {
    }
}

/// <summary>
/// Thrown when application-level input shape or parameter validation fails.
/// </summary>
public class ValidationException : Exception
{
    public ValidationException(string message) : base(message)
    {
    }
}
