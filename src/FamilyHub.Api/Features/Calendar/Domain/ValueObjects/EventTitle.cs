using Vogen;

namespace FamilyHub.Api.Features.Calendar.Domain.ValueObjects;

[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct EventTitle
{
    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Validation.Invalid("Event title is required");
        }

        if (value.Length < 1)
        {
            return Validation.Invalid("Event title must be at least 1 character");
        }

        if (value.Length > 200)
        {
            return Validation.Invalid("Event title too long (max 200 characters)");
        }

        return Validation.Ok;
    }
}
