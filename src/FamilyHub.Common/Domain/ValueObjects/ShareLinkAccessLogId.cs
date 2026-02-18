using Vogen;

namespace FamilyHub.Common.Domain.ValueObjects;

[ValueObject<Guid>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct ShareLinkAccessLogId
{
    public static ShareLinkAccessLogId New() => From(Guid.NewGuid());

    private static Validation Validate(Guid value) =>
        value == Guid.Empty ? Validation.Invalid("ShareLinkAccessLogId cannot be empty") : Validation.Ok;
}
