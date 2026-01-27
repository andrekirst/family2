namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Payloads;

/// <summary>
/// GraphQL payload for email verification result.
/// </summary>
public sealed class VerifyEmailPayload
{
    /// <summary>
    /// Indicates the email was verified successfully.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Message for the user.
    /// </summary>
    public string? Message { get; init; }

    /// <summary>
    /// Errors that occurred during email verification.
    /// </summary>
    public IReadOnlyList<PayloadError>? Errors { get; init; }
}
