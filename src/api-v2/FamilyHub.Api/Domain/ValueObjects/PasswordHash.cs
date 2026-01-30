using Vogen;

namespace FamilyHub.Api.Domain.ValueObjects;

[ValueObject<string>(conversions: Conversions.Default | Conversions.EfCoreValueConverter)]
public readonly partial struct PasswordHash
{
    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Validation.Invalid("Password hash cannot be empty.");

        return Validation.Ok;
    }
}
