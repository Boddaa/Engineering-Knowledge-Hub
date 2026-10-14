namespace EngineeringKnowledgeHub.Api.Security;

using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public sealed class GovernanceAuthenticationHandler(
    IOptionsMonitor<GovernanceAuthenticationOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder) : AuthenticationHandler<GovernanceAuthenticationOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var authHeaderValues))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var authHeader = authHeaderValues.ToString();
        if (string.IsNullOrWhiteSpace(authHeader))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        const string bearerPrefix = "Bearer ";
        if (!authHeader.StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var token = authHeader[bearerPrefix.Length..].Trim();
        if (string.IsNullOrEmpty(token))
        {
            return Task.FromResult(AuthenticateResult.Fail("Empty Bearer token supplied."));
        }

        // Fail-safe closed: If no tokens are configured, fail closed
        if (Options.Tokens is null || Options.Tokens.Count == 0)
        {
            return Task.FromResult(AuthenticateResult.Fail("Governance authentication is not configured. Audit endpoints are locked."));
        }

        if (!Options.Tokens.TryGetValue(token, out var identity) || identity is null)
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid or unrecognized authentication token."));
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, identity.UserId.ToString()),
            new(ClaimTypes.Name, identity.Username)
        };

        foreach (var role in identity.Roles)
        {
            if (!string.IsNullOrWhiteSpace(role))
            {
                claims.Add(new(ClaimTypes.Role, role.Trim()));
            }
        }

        var claimsIdentity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(claimsIdentity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
