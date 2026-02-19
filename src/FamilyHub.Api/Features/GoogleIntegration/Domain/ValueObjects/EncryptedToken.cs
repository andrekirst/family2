using Vogen;

namespace FamilyHub.Api.Features.GoogleIntegration.Domain.ValueObjects;

[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct EncryptedToken
{
    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Validation.Invalid("Encrypted token cannot be empty");
        return Validation.Ok;
    }
}
