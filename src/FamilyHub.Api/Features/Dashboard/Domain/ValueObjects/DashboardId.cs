using Vogen;

namespace FamilyHub.Api.Features.Dashboard.Domain.ValueObjects;

[ValueObject<Guid>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct DashboardId
{
    public static DashboardId New() => From(Guid.NewGuid());

    private static Validation Validate(Guid value)
    {
        if (value == Guid.Empty)
            return Validation.Invalid("Dashboard ID cannot be empty");

        return Validation.Ok;
    }
}
