using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace FamilyHub.TestCommon.Auth;

/// <summary>
/// Generates mock JWT tokens for integration tests without requiring a running Keycloak instance.
/// Uses a static RSA key pair shared across all tests in the process.
/// </summary>
public static class MockJwtTokenGenerator
{
    public const string Issuer = "https://test.keycloak.local/realms/TestRealm";
    public const string Audience = "account";

    private static readonly RSA Rsa = RSA.Create(2048);

    public static RsaSecurityKey SecurityKey { get; } = new(Rsa);

    public static SigningCredentials SigningCredentials { get; } =
        new(SecurityKey, SecurityAlgorithms.RsaSha256);

    /// <summary>
    /// Generates a JWT token matching the claim structure Keycloak produces.
    /// Claim names align with <c>ClaimNames</c> constants (sub, email, name, email_verified).
    /// </summary>
    public static string GenerateToken(
        string sub,
        string email,
        string name,
        bool emailVerified = true,
        TimeSpan? lifetime = null)
    {
        var claims = new List<Claim>
        {
            new("sub", sub),
            new("email", email),
            new("name", name),
            new("email_verified", emailVerified.ToString().ToLowerInvariant()),
            new("preferred_username", email),
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.Add(lifetime ?? TimeSpan.FromHours(1)),
            Issuer = Issuer,
            Audience = Audience,
            SigningCredentials = SigningCredentials,
        };

        var handler = new JwtSecurityTokenHandler();
        var token = handler.CreateToken(tokenDescriptor);
        return handler.WriteToken(token);
    }
}
