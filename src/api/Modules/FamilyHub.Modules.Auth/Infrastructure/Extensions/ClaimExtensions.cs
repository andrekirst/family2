using System.Security.Claims;

namespace FamilyHub.Modules.Auth.Infrastructure.Extensions;

/// <summary>
/// Extension methods for working with security claims.
/// </summary>
public static class ClaimExtensions
{
    /// <summary>
    /// Gets the value of a claim by its type from a collection of claims.
    /// </summary>
    /// <param name="claims">The collection of claims to search.</param>
    /// <param name="claimType">The type of claim to find.</param>
    /// <param name="throwIfMissing">If true, throws an exception when the claim is not found; otherwise returns null.</param>
    /// <returns>The claim value if found; otherwise null (or throws if throwIfMissing is true).</returns>
    /// <exception cref="InvalidOperationException">Thrown when the claim is not found and throwIfMissing is true.</exception>
    public static string? GetTokenValueByClaimType(this IEnumerable<Claim> claims, string claimType, bool throwIfMissing = true)
    {
        var value = claims.FirstOrDefault(c => c.Type == claimType)?.Value;

        return value ?? (value is null && throwIfMissing ? throw new InvalidOperationException($"Missing '{claimType}' claim") : null);
    }
}
