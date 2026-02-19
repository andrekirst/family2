using Vogen;

namespace FamilyHub.Api.Features.GoogleIntegration.Domain.ValueObjects;

[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct GoogleScopes
{
    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Validation.Invalid("Google scopes cannot be empty");
        return Validation.Ok;
    }

    public bool HasCalendarScope() =>
        Value.Contains("calendar.readonly", StringComparison.OrdinalIgnoreCase) ||
        Value.Contains("calendar.events", StringComparison.OrdinalIgnoreCase);

    private static string NormalizeInput(string input) => input?.Trim() ?? string.Empty;
}
