namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Inputs;

/// <summary>
/// GraphQL input for changing the authenticated user's password.
/// Requires current password verification before allowing change.
/// </summary>
public sealed record ChangePasswordInput
{
    /// <summary>
    /// User's current password for verification.
    /// </summary>
    public required string CurrentPassword { get; init; }

    /// <summary>
    /// New password. Must meet password policy requirements.
    /// </summary>
    public required string NewPassword { get; init; }

    /// <summary>
    /// New password confirmation. Must match NewPassword.
    /// </summary>
    public required string ConfirmNewPassword { get; init; }
}
