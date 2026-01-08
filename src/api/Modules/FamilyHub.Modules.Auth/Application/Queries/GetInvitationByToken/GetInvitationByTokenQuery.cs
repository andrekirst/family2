using FamilyHub.SharedKernel.Application.Abstractions.Authorization;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.Auth.Application.Queries.GetInvitationByToken;

/// <summary>
/// Query to retrieve invitation details by token (for invitees before accepting).
/// Public query - does not require authentication.
/// </summary>
/// <param name="Token">The invitation token.</param>
public sealed record GetInvitationByTokenQuery(
    InvitationToken Token
) : IRequest<GetInvitationByTokenResult?>,
    IPublicQuery;
