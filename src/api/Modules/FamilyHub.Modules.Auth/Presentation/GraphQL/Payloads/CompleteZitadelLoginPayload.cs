using FamilyHub.Modules.Auth.Presentation.GraphQL.Types;

namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Payloads;

/// <summary>
/// GraphQL payload for completed Zitadel OAuth login.
/// </summary>
public sealed record CompleteZitadelLoginPayload
{
    /// <summary>
    /// Authentication result with user and tokens (null if errors occurred)
    /// </summary>
    public AuthenticationResult? AuthenticationResult { get; init; }

    /// <summary>
    /// Errors that occurred during login (empty if successful)
    /// </summary>
    public UserError[] Errors { get; init; } = [];

    /// <summary>
    /// Creates a successful payload
    /// </summary>
    public static CompleteZitadelLoginPayload Success(AuthenticationResult authenticationResult)
        => new() { AuthenticationResult = authenticationResult };

    /// <summary>
    /// Creates a failure payload with errors
    /// </summary>
    public static CompleteZitadelLoginPayload Failure(params UserError[] errors)
        => new() { Errors = errors };
}
