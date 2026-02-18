using Vogen;

namespace FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;

[ValueObject<double>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct Latitude
{
    private static Validation Validate(double value)
    {
        if (value is < -90 or > 90)
            return Validation.Invalid("Latitude must be between -90 and 90");
        return Validation.Ok;
    }
}

[ValueObject<double>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct Longitude
{
    private static Validation Validate(double value)
    {
        if (value is < -180 or > 180)
            return Validation.Invalid("Longitude must be between -180 and 180");
        return Validation.Ok;
    }
}
