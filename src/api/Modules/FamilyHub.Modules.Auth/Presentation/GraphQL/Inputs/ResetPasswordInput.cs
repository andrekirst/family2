namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Inputs;

/// <summary>
/// GraphQL input for resetting password using a token (from email link).
/// </summary>
public sealed record ResetPasswordInput
{
    /// <summary>
    /// Password reset token from the email link.
    /// </summary>
    public required string Token { get; init; }

    /// <summary>
    /// New password. Must meet password policy requirements.
    /// </summary>
    public required string NewPassword { get; init; }

    /// <summary>
    /// New password confirmation. Must match NewPassword.
    /// </summary>
    public required string ConfirmNewPassword { get; init; }
}
