using System.Security.Cryptography;
using System.Text;

namespace FamilyHub.SharedKernel.Domain.ValueObjects;

/// <summary>
/// Represents a user-friendly invitation display code (8 alphanumeric characters).
/// Example format: "INV-KX7M2P"
/// </summary>
[ValueObject<string>(conversions: Conversions.Default | Conversions.EfCoreValueConverter)]
public readonly partial struct InvitationDisplayCode
{
    private const int CodeLength = 8;
    private const string AllowedCharacters = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // Excludes ambiguous: 0, O, I, 1
    private const string AllowedCharactersBothCases = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjklmnpqrstuvwxyz23456789";

    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Validation.Invalid("Display code cannot be empty.");
        }

        if (value.Length != CodeLength)
        {
            return Validation.Invalid($"Display code must be exactly {CodeLength} characters.");
        }

        // Validation happens after normalization in Vogen, so value is already uppercase
        // But accept both cases to handle edge cases
        if (!value.All(c => AllowedCharactersBothCases.Contains(c)))
        {
            return Validation.Invalid("Display code must contain only alphanumeric characters (excluding ambiguous characters).");
        }

        return Validation.Ok;
    }

    private static string NormalizeInput(string input) =>
        input.Trim().ToUpperInvariant();

    /// <summary>
    /// Generates a new cryptographically random display code.
    /// </summary>
    public static InvitationDisplayCode Generate()
    {
        var codeBuilder = new StringBuilder(CodeLength);
        var randomBytes = new byte[CodeLength];

        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }

        foreach (var randomByte in randomBytes)
        {
            codeBuilder.Append(AllowedCharacters[randomByte % AllowedCharacters.Length]);
        }

        return From(codeBuilder.ToString());
    }
}
