using FamilyHub.Modules.Auth.Presentation.GraphQL.Types;
using FamilyHub.SharedKernel.Presentation.GraphQL;

namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Payloads;

/// <summary>
/// GraphQL payload for completed Zitadel OAuth login.
/// </summary>
public sealed record CompleteZitadelLoginPayload : PayloadBase
{
    /// <summary>
    /// Authentication result with user and tokens (null if errors occurred)
    /// </summary>
    public AuthenticationResult? AuthenticationResult { get; init; }

    /// <summary>
    /// Constructor for successful payload (called by factory).
    /// </summary>
    /// <param name="authenticationResult">The authentication result</param>
    public CompleteZitadelLoginPayload(AuthenticationResult authenticationResult)
    {
        AuthenticationResult = authenticationResult;
    }

    /// <summary>
    /// Constructor for error payload (called by factory).
    /// </summary>
    /// <param name="errors">List of errors that occurred</param>
    public CompleteZitadelLoginPayload(IReadOnlyList<UserError> errors) : base(errors)
    {
        AuthenticationResult = null;
    }
}
