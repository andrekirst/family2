namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Inputs;

/// <summary>
/// GraphQL input for resetting password using a 6-digit code (mobile flow).
/// </summary>
public sealed record ResetPasswordWithCodeInput
{
    /// <summary>
    /// Email address of the account to reset.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// 6-digit password reset code from the email.
    /// </summary>
    public required string Code { get; init; }

    /// <summary>
    /// New password. Must meet password policy requirements.
    /// </summary>
    public required string NewPassword { get; init; }

    /// <summary>
    /// New password confirmation. Must match NewPassword.
    /// </summary>
    public required string ConfirmNewPassword { get; init; }
}
