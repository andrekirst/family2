namespace FamilyHub.Modules.Family.Domain.Enums;

/// <summary>
/// Email delivery status.
/// </summary>
public enum EmailStatus
{
    /// <summary>
    /// Email is waiting to be sent.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Email has been successfully sent.
    /// </summary>
    Sent = 1,

    /// <summary>
    /// Email sending failed but can be retried.
    /// </summary>
    Failed = 2,

    /// <summary>
    /// Email sending failed permanently after maximum retry attempts.
    /// </summary>
    PermanentlyFailed = 3
}
