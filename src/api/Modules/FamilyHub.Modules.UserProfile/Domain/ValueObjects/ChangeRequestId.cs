namespace FamilyHub.Modules.UserProfile.Domain.ValueObjects;

/// <summary>
/// Unique identifier for a profile change request.
/// Strongly-typed GUID value object.
/// </summary>
[ValueObject<Guid>(conversions: Conversions.Default | Conversions.EfCoreValueConverter)]
public readonly partial struct ChangeRequestId
{
    /// <summary>
    /// Creates a new unique ChangeRequestId.
    /// </summary>
    public static ChangeRequestId New() => From(Guid.NewGuid());
}
