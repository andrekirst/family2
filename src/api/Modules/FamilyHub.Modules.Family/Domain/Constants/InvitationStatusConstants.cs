namespace FamilyHub.Modules.Family.Domain.Constants;

/// <summary>
/// Invitation status constants for the Family module.
/// Centralizes status names to prevent string typos and ensure consistency.
/// </summary>
public static class InvitationStatusConstants
{
    /// <summary>
    /// Pending status - invitation has been sent but not yet accepted.
    /// </summary>
    public const string PendingValue = "pending";

    /// <summary>
    /// Accepted status - invitation has been accepted by the recipient.
    /// </summary>
    public const string AcceptedValue = "accepted";

    /// <summary>
    /// Expired status - invitation has passed its expiration date.
    /// </summary>
    public const string ExpiredValue = "expired";

    /// <summary>
    /// Canceled status - invitation was canceled by a family admin.
    /// </summary>
    public const string CanceledValue = "canceled";
}
