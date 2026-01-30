using Vogen;

namespace FamilyHub.Api.Domain.ValueObjects;

[ValueObject<Guid>(conversions: Conversions.Default | Conversions.EfCoreValueConverter)]
public readonly partial struct UserId
{
    public static UserId New() => From(Guid.NewGuid());
}
