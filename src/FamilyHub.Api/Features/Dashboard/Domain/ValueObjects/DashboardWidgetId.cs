using Vogen;

namespace FamilyHub.Api.Features.Dashboard.Domain.ValueObjects;

[ValueObject<Guid>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct DashboardWidgetId
{
    public static DashboardWidgetId New() => From(Guid.NewGuid());

    private static Validation Validate(Guid value)
    {
        if (value == Guid.Empty)
            return Validation.Invalid("Dashboard widget ID cannot be empty");

        return Validation.Ok;
    }
}
