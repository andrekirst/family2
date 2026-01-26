namespace FamilyHub.Modules.UserProfile.Domain.ValueObjects;

/// <summary>
/// Represents the status of a profile change request.
/// Tracks whether a change is pending approval, approved, or rejected.
/// </summary>
[ValueObject<string>(conversions: Conversions.Default | Conversions.EfCoreValueConverter)]
public readonly partial struct ChangeRequestStatus
{
    private static readonly string[] ValidStatuses =
    [
        "pending",
        "approved",
        "rejected"
    ];

    /// <summary>
    /// Pending status - awaiting parent/admin approval.
    /// </summary>
    public static readonly ChangeRequestStatus Pending = From("pending");

    /// <summary>
    /// Approved status - change has been approved and applied.
    /// </summary>
    public static readonly ChangeRequestStatus Approved = From("approved");

    /// <summary>
    /// Rejected status - change was rejected by parent/admin.
    /// </summary>
    public static readonly ChangeRequestStatus Rejected = From("rejected");

    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Validation.Invalid("Change request status cannot be empty.");
        }

        if (!ValidStatuses.Contains(value.ToLowerInvariant()))
        {
            return Validation.Invalid($"Invalid change request status. Must be one of: {string.Join(", ", ValidStatuses)}");
        }

        return Validation.Ok;
    }

    private static string NormalizeInput(string input) =>
        input.Trim().ToLowerInvariant();
}
