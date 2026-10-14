namespace EngineeringKnowledgeHub.Api.Security;

using Microsoft.AspNetCore.Authentication;

public sealed class GovernanceAuthenticationOptions : AuthenticationSchemeOptions
{
    public const string SectionName = "Authentication:Governance";

    /// <summary>
    /// Configured identity tokens mapped to user metadata and security roles.
    /// In production, tokens are supplied via environment variables or secret vaults.
    /// In development and testing, tokens are loaded from appsettings.Development.json.
    /// </summary>
    public Dictionary<string, GovernanceTokenIdentity> Tokens { get; set; } = new(StringComparer.Ordinal);
}

public sealed class GovernanceTokenIdentity
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = [];
}
