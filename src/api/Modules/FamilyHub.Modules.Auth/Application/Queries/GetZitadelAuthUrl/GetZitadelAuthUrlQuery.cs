using MediatR;

namespace FamilyHub.Modules.Auth.Application.Queries.GetZitadelAuthUrl;

/// <summary>
/// Query to get the Zitadel OAuth authorization URL with PKCE parameters.
/// </summary>
public sealed record GetZitadelAuthUrlQuery : IRequest<GetZitadelAuthUrlResult>;
