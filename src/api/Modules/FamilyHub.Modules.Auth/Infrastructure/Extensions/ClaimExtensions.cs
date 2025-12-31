using System.Security.Claims;

namespace FamilyHub.Modules.Auth.Infrastructure.Extensions;

public static class ClaimExtensions
{
    public static string GetTokenValueByClaimType(this IEnumerable<Claim> claims, string claimType)
    {
        return claims.FirstOrDefault(c => c.Type == claimType)?.Value
            ?? throw new InvalidOperationException($"Missing '{claimType}' claim");
    }
}