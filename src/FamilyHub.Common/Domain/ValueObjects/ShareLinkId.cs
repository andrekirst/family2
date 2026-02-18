using Vogen;

namespace FamilyHub.Common.Domain.ValueObjects;

[ValueObject<Guid>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct ShareLinkId
{
    public static ShareLinkId New() => From(Guid.NewGuid());

    private static Validation Validate(Guid value) =>
        value == Guid.Empty ? Validation.Invalid("ShareLinkId cannot be empty") : Validation.Ok;
}
