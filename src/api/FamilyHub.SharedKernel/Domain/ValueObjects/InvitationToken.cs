using System.Security.Cryptography;

namespace FamilyHub.SharedKernel.Domain.ValueObjects;

/// <summary>
/// Represents a cryptographically secure invitation token (64 URL-safe base64 characters).
/// Used for accepting email-based invitations securely.
/// </summary>
[ValueObject<string>(conversions: Conversions.Default | Conversions.EfCoreValueConverter)]
public readonly partial struct InvitationToken
{
    private const int TokenLength = 64;
    private const int RandomBytesLength = 48; // 48 bytes = 64 base64 characters

    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Validation.Invalid("Invitation token cannot be empty.");
        }

        if (value.Length != TokenLength)
        {
            return Validation.Invalid($"Invitation token must be exactly {TokenLength} characters.");
        }

        // Validate URL-safe base64 characters (A-Z, a-z, 0-9, -, _)
        if (!value.All(c => char.IsLetterOrDigit(c) || c == '-' || c == '_'))
        {
            return Validation.Invalid("Invitation token must contain only URL-safe base64 characters.");
        }

        return Validation.Ok;
    }

    /// <summary>
    /// Generates a new cryptographically secure invitation token.
    /// </summary>
    public static InvitationToken Generate()
    {
        var randomBytes = new byte[RandomBytesLength];

        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }

        // Convert to URL-safe base64 (replace + with -, / with _, remove padding =)
        var base64 = Convert.ToBase64String(randomBytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');

        return From(base64);
    }
}
