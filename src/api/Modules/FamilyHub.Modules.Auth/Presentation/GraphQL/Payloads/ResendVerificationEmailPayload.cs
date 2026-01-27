namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Payloads;

/// <summary>
/// GraphQL payload for resend verification email result.
/// </summary>
public sealed class ResendVerificationEmailPayload
{
    /// <summary>
    /// Indicates the email was sent successfully.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Message for the user.
    /// </summary>
    public string? Message { get; init; }

    /// <summary>
    /// Errors that occurred.
    /// </summary>
    public IReadOnlyList<PayloadError>? Errors { get; init; }
}
