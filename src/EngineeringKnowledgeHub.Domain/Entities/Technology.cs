namespace EngineeringKnowledgeHub.Domain.Entities;

using EngineeringKnowledgeHub.Domain.Exceptions;

/// <summary>
/// Represents a technical domain, framework, or protocol (e.g. ASP.NET Core, SQL Server, WPF, RTSP, ONVIF).
/// </summary>
public sealed class Technology
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }

    public Technology(Guid id, string name, string? description = null)
    {
        if (id == Guid.Empty) throw new DomainRuleViolationException("Technology Id cannot be empty.");
        if (string.IsNullOrWhiteSpace(name)) throw new DomainRuleViolationException("Technology name cannot be null or whitespace.");

        Id = id;
        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;
    }

    public void Update(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainRuleViolationException("Technology name cannot be null or whitespace.");

        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;
    }
}
