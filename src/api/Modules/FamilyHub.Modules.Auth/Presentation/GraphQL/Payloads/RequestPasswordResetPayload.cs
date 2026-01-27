namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Payloads;

/// <summary>
/// GraphQL payload for password reset request result.
/// </summary>
public sealed class RequestPasswordResetPayload
{
    /// <summary>
    /// Always true for security (doesn't reveal if email exists).
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Generic message for the user.
    /// </summary>
    public string? Message { get; init; }

    /// <summary>
    /// Errors that occurred during the request.
    /// </summary>
    public IReadOnlyList<PayloadError>? Errors { get; init; }
}
