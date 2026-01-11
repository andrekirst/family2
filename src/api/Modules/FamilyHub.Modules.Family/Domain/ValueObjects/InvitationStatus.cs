using FamilyHub.Modules.Family.Domain.Constants;

namespace FamilyHub.Modules.Family.Domain.ValueObjects;

/// <summary>
/// Represents the status of a family member invitation.
/// </summary>
[ValueObject<string>(conversions: Conversions.Default | Conversions.EfCoreValueConverter)]
public readonly partial struct InvitationStatus
{
    private static readonly string[] ValidStatuses =
    [
        InvitationStatusConstants.PendingValue,
        InvitationStatusConstants.AcceptedValue,
        InvitationStatusConstants.ExpiredValue,
        InvitationStatusConstants.CanceledValue
    ];

    /// <summary>
    /// Invitation is pending acceptance.
    /// </summary>
    public static readonly InvitationStatus Pending = From(InvitationStatusConstants.PendingValue);

    /// <summary>
    /// Invitation has been accepted.
    /// </summary>
    public static readonly InvitationStatus Accepted = From(InvitationStatusConstants.AcceptedValue);

    /// <summary>
    /// Invitation has expired.
    /// </summary>
    public static readonly InvitationStatus Expired = From(InvitationStatusConstants.ExpiredValue);

    /// <summary>
    /// Invitation has been canceled.
    /// </summary>
    public static readonly InvitationStatus Canceled = From(InvitationStatusConstants.CanceledValue);

    private static Validation Validate(string value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? Validation.Invalid("Invitation status cannot be empty.")
            : !ValidStatuses.Contains(value.ToLowerInvariant())
                ? Validation.Invalid($"Invalid invitation status. Must be one of: {string.Join(", ", ValidStatuses)}")
                : Validation.Ok;
    }

    private static string NormalizeInput(string input) =>
        input.Trim().ToLowerInvariant();
}
