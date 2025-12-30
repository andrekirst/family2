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
    public async Task<GetZitadelAuthUrlPayload> GetZitadelAuthUrl(
        [Service] IMediator mediator,
        [Service] ILogger<AuthQueries> logger)
    {
        // TODO Please use decorator pattern to log the start end end logging message
        logger.LogInformation("GraphQL: getZitadelAuthUrl query called");

        var result = await mediator.Send(new GetZitadelAuthUrlQuery());

        // Please create an adapter to map domain entity to GraphQL type
        return new GetZitadelAuthUrlPayload
        {
            AuthorizationUrl = result.AuthorizationUrl,
            CodeVerifier = result.CodeVerifier,
            State = result.State
        };
    }
}
