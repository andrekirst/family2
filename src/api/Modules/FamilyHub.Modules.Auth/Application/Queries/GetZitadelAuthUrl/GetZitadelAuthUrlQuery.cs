using MediatR;

namespace FamilyHub.Modules.Auth.Application.Queries.GetZitadelAuthUrl;

/// <summary>
/// Query to get the Zitadel OAuth authorization URL with PKCE parameters.
/// </summary>
/// <param name="LoginHint">Optional email or username to pre-fill Zitadel login form (Phase 5: Dual Authentication).</param>
public sealed record GetZitadelAuthUrlQuery(string? LoginHint = null) : IRequest<GetZitadelAuthUrlResult>;
