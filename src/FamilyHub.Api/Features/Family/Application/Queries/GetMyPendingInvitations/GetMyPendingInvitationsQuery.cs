using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Models;

namespace FamilyHub.Api.Features.Family.Application.Queries.GetMyPendingInvitations;

/// <summary>
/// Query to get pending invitations for the current user's email address.
/// </summary>
public sealed record GetMyPendingInvitationsQuery : IReadOnlyQuery<List<InvitationDto>>, IRequireUser
{
    public UserId UserId { get; init; }
}
