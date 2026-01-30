using Vogen;

namespace FamilyHub.Api.Domain.ValueObjects;

[ValueObject<Guid>(conversions: Conversions.Default | Conversions.EfCoreValueConverter)]
public readonly partial struct RefreshTokenId
{
    public static RefreshTokenId New() => From(Guid.NewGuid());
}
