using FamilyHub.Modules.Auth.Presentation.GraphQL.Types;

namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Payloads;

/// <summary>
/// GraphQL payload for completed Zitadel OAuth login.
/// </summary>
[Obsolete("Replaced by Hot Chocolate v14 Mutation Conventions. Remove after frontend migration.")]
public sealed record CompleteZitadelLoginPayload
{
    /// <summary>
    /// Authentication result with user and tokens (null if errors occurred)
    /// </summary>
    public AuthenticationResult? AuthenticationResult { get; init; }

    /// <summary>
    /// Constructor for successful payload (called by MutationHandler).
    /// </summary>
    /// <param name="authenticationResult">The authentication result from ToGraphQLType()</param>
    public CompleteZitadelLoginPayload(AuthenticationResult authenticationResult)
    {
        AuthenticationResult = authenticationResult;
    }

    /// <summary>
    /// Constructor for error payload (called by MutationHandler).
    /// </summary>
    /// <param name="errors">List of errors that occurred</param>
    public CompleteZitadelLoginPayload(IReadOnlyList<UserError> errors)
    {
        AuthenticationResult = null;
        Errors = errors;
    }

    /// <summary>
    /// List of errors that occurred during mutation execution.
    /// Null or empty when the mutation succeeded.
    /// </summary>
    public IReadOnlyList<UserError>? Errors { get; init; }
}

/// <summary>
/// Represents a GraphQL error with a message and optional code.
/// </summary>
public sealed record UserError
{
    /// <summary>
    /// Gets the human-readable error message.
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Gets the optional error code for programmatic handling.
    /// </summary>
    public string? Code { get; init; }
}
