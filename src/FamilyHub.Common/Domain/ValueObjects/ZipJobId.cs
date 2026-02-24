using Vogen;

namespace FamilyHub.Common.Domain.ValueObjects;

[ValueObject<Guid>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct ZipJobId
{
    public static ZipJobId New() => From(Guid.NewGuid());
}
