using Vogen;

namespace FamilyHub.Api.Features.Family.Domain.ValueObjects;

/// <summary>
/// Invitation token value object.
/// Represents the SHA256 hash of the plaintext token stored in the database.
/// The plaintext token (64-char URL-safe) is sent in the email link.
/// </summary>
[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct InvitationToken
{
    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Validation.Invalid("Invitation token is required");
        }

        if (value.Length != 64)
        {
            return Validation.Invalid("Invitation token hash must be exactly 64 characters (SHA256 hex)");
        }

        return Validation.Ok;
    }
}
