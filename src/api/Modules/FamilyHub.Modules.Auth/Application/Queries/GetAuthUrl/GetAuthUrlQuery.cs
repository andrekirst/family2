using FamilyHub.SharedKernel.Application.CQRS;

namespace FamilyHub.Modules.Auth.Application.Queries.GetAuthUrl;

/// <summary>
/// Query to get the OAuth authorization URL with PKCE parameters.
/// </summary>
/// <param name="LoginHint">Optional email or username to pre-fill login form (Phase 5: Dual Authentication).</param>
public sealed record GetAuthUrlQuery(string? LoginHint = null) : IQuery<GetAuthUrlResult>;
