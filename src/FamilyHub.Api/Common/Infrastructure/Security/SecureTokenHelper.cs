using System.Security.Cryptography;
using System.Text;

namespace FamilyHub.Api.Common.Infrastructure.Security;

public static class SecureTokenHelper
{
    /// <summary>
    /// Generates a 64-character URL-safe cryptographically random token.
    /// </summary>
    public static string GenerateSecureToken()
    {
        var bytes = new byte[48]; // 48 bytes = 64 base64url chars
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");
    }

    /// <summary>
    /// Computes SHA256 hash of a string and returns it as a 64-character hex string.
    /// </summary>
    public static string ComputeSha256Hash(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexStringLower(bytes);
    }
}
