namespace FamilyHub.Modules.Auth.Domain.ValueObjects;

/// <summary>
/// Represents a strongly-typed RefreshToken identifier.
/// </summary>
[ValueObject<Guid>(conversions: Conversions.Default | Conversions.EfCoreValueConverter)]
public readonly partial struct RefreshTokenId
{
    /// <summary>
    /// Creates a new RefreshTokenId with a newly generated GUID.
    /// </summary>
    public static RefreshTokenId New() => From(Guid.NewGuid());
}
