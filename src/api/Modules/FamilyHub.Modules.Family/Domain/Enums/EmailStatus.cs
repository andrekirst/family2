namespace FamilyHub.Modules.Family.Domain.Enums;

/// <summary>
/// Email delivery status.
/// </summary>
public enum EmailStatus
{
    /// <summary>
    /// Email is waiting to be sent.
    /// </summary>
    PENDING = 0,

    /// <summary>
    /// Email has been successfully sent.
    /// </summary>
    SENT = 1,

    /// <summary>
    /// Email sending failed but can be retried.
    /// </summary>
    FAILED = 2,

    /// <summary>
    /// Email sending failed permanently after maximum retry attempts.
    /// </summary>
    PERMANENTLY_FAILED = 3
}
