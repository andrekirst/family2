using Vogen;

namespace FamilyHub.Api.Features.Dashboard.Domain.ValueObjects;

[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct DashboardLayoutName
{
    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Validation.Invalid("Dashboard layout name is required");

        if (value.Length > 100)
            return Validation.Invalid("Dashboard layout name too long (max 100 characters)");

        return Validation.Ok;
    }
}
