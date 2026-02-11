using Vogen;

namespace FamilyHub.Api.Features.Calendar.Domain.ValueObjects;

[ValueObject<Guid>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct CalendarEventId
{
    public static CalendarEventId New() => From(Guid.NewGuid());

    private static Validation Validate(Guid value)
    {
        if (value == Guid.Empty)
        {
            return Validation.Invalid("Calendar event ID cannot be empty");
        }

        return Validation.Ok;
    }
}
