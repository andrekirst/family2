using Vogen;

namespace FamilyHub.Api.Features.BaseData.Domain.ValueObjects;

[ValueObject<Guid>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct FederalStateId
{
    public static FederalStateId New() => From(Guid.NewGuid());

    private static Validation Validate(Guid value)
    {
        if (value == Guid.Empty)
        {
            return Validation.Invalid("Federal state ID cannot be empty");
        }

        return Validation.Ok;
    }
}
