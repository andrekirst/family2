using Vogen;

namespace FamilyHub.EventChain.Domain.ValueObjects;

[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct ChainName
{
    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Validation.Invalid("Chain name cannot be empty");

        if (value.Length > 200)
        {
            return Validation.Invalid("Chain name cannot exceed 200 characters");
        }

        return Validation.Ok;
    }

    private static string NormalizeInput(string input)
        => input?.Trim() ?? string.Empty;
}
