namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Inputs;

/// <summary>
/// GraphQL input for completing Zitadel OAuth login.
/// </summary>
public sealed record CompleteZitadelLoginInput
{
    /// <summary>
    /// Authorization code received from Zitadel callback
    /// </summary>
    public required string AuthorizationCode { get; init; }

    /// <summary>
    /// PKCE code verifier that frontend stored during authorization request
    /// </summary>
    public required string CodeVerifier { get; init; }
}
