using Vogen;

namespace FamilyHub.Api.Features.GoogleIntegration.Domain.ValueObjects;

[ValueObject<Guid>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct GoogleAccountLinkId
{
    public static GoogleAccountLinkId New() => From(Guid.NewGuid());

    private static Validation Validate(Guid value)
    {
        if (value == Guid.Empty)
            return Validation.Invalid("Google account link ID cannot be empty");
        return Validation.Ok;
    }
}
