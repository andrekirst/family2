using System.Security.Claims;

namespace FamilyHub.Modules.Auth.Infrastructure.Extensions;

public static class ClaimExtensions
{
    public static string? GetTokenValueByClaimType(this IEnumerable<Claim> claims, string claimType, bool throwIfMissing = true)
    {
        var value = claims.FirstOrDefault(c => c.Type == claimType)?.Value;

        return value ?? (value is null && throwIfMissing ? throw new InvalidOperationException($"Missing '{claimType}' claim") : null);
    }
}