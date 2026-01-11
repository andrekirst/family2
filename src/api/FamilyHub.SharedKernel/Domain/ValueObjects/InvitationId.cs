namespace FamilyHub.SharedKernel.Domain.ValueObjects;

/// <summary>
/// Represents a strongly-typed Invitation identifier.
/// Note: Validation allows Guid.Empty for EF Core materialization.
/// Domain logic should ensure non-empty GUIDs are used in business operations.
/// </summary>
[ValueObject<Guid>(conversions: Conversions.Default | Conversions.EfCoreValueConverter)]
public readonly partial struct InvitationId
{
    /// <summary>
    /// Creates a new InvitationId with a newly generated GUID.
    /// </summary>
    /// <returns>A new InvitationId instance.</returns>
    public static InvitationId New() => From(Guid.NewGuid());
}
