namespace FamilyHub.Modules.UserProfile.Domain.ValueObjects;

/// <summary>
/// Unique identifier for a user profile.
/// Strongly-typed GUID value object.
/// </summary>
[ValueObject<Guid>(conversions: Conversions.Default | Conversions.EfCoreValueConverter)]
public readonly partial struct UserProfileId
{
    /// <summary>
    /// Creates a new unique UserProfileId.
    /// </summary>
    public static UserProfileId New() => From(Guid.NewGuid());
}
