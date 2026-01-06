using FamilyHub.Modules.Auth.Application.Queries.GetZitadelAuthUrl;
using FamilyHub.Modules.Auth.Presentation.GraphQL.Payloads;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Queries;

/// <summary>
/// GraphQL queries for authentication operations.
/// </summary>
[ExtendObjectType("Query")]
public sealed class AuthQueries
{
    /// <summary>
    /// Gets the Zitadel OAuth authorization URL with PKCE parameters.
    /// Frontend should redirect user to this URL to start OAuth flow.
    /// </summary>
    /// <param name="loginHint">Optional email or username to pre-fill Zitadel login form (Phase 5: Dual Authentication)</param>
    public async Task<GetZitadelAuthUrlPayload> GetZitadelAuthUrl(
        string? loginHint,
        [Service] IMediator mediator,
        [Service] ILogger<AuthQueries> logger)
    {
        // TODO Please use decorator pattern to log the start end end logging message
        logger.LogInformation("GraphQL: getZitadelAuthUrl query called with loginHint: {LoginHint}",
            string.IsNullOrWhiteSpace(loginHint) ? "none" : "***");

        var result = await mediator.Send(new GetZitadelAuthUrlQuery(loginHint));

        return new GetZitadelAuthUrlPayload
        {
            AuthorizationUrl = result.AuthorizationUrl,
            CodeVerifier = result.CodeVerifier,
            State = result.State
        };
    }
}
