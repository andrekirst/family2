using FamilyHub.Modules.Auth.Application.Queries.GetAuthUrl;
using FamilyHub.Modules.Auth.Presentation.GraphQL.Payloads;
using MediatR;

namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Types;

/// <summary>
/// GraphQL type extensions for AuthType.
/// Adds resolver fields for authentication operations.
/// </summary>
[ExtendObjectType(typeof(AuthType))]
public sealed class AuthTypeExtensions
{
    /// <summary>
    /// Gets the OAuth authorization URL with PKCE parameters.
    /// Frontend should redirect user to this URL to start OAuth flow.
    /// Provider-agnostic to support multiple authentication systems (currently Zitadel).
    /// </summary>
    /// <param name="loginHint">Optional email or username to pre-fill login form (Phase 5: Dual Authentication)</param>
    /// <param name="mediator"></param>
    /// <param name="cancellationToken"></param>
    [GraphQLDescription("Get OAuth authorization URL with PKCE parameters")]
    public async Task<AuthUrlPayload> Url(
        string? loginHint,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAuthUrlQuery(loginHint), cancellationToken);

        return new AuthUrlPayload
        {
            AuthorizationUrl = result.AuthorizationUrl,
            CodeVerifier = result.CodeVerifier,
            State = result.State
        };
    }
}
