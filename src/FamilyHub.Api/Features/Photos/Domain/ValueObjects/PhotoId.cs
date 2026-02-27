using Vogen;

namespace FamilyHub.Api.Features.Photos.Domain.ValueObjects;

[ValueObject<Guid>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct PhotoId
{
    public static PhotoId New() => From(Guid.NewGuid());

    private static Validation Validate(Guid value)
    {
        if (value == Guid.Empty)
        {
            return Validation.Invalid("PhotoId cannot be empty.");
        }

        return Validation.Ok;
    }
}
