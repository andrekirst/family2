using Vogen;

namespace FamilyHub.Api.Features.GoogleIntegration.Domain.ValueObjects;

[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct GoogleAccountId
{
    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Validation.Invalid("Google account ID cannot be empty");
        if (value.Length > 255)
            return Validation.Invalid("Google account ID cannot exceed 255 characters");
        return Validation.Ok;
    }

    private static string NormalizeInput(string input) => input?.Trim() ?? string.Empty;
}
