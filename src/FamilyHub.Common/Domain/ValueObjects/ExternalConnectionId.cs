using Vogen;

namespace FamilyHub.Common.Domain.ValueObjects;

[ValueObject<Guid>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct ExternalConnectionId
{
    public static ExternalConnectionId New() => From(Guid.NewGuid());
}
