using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Models;

namespace FamilyHub.Api.Features.Family.Application.Queries.GetPendingInvitations;

/// <summary>
/// Query to get pending invitations for the current user's family (admin/family view).
/// </summary>
public sealed record GetPendingInvitationsQuery : IReadOnlyQuery<List<InvitationDto>>, IRequireFamily
{
    public UserId UserId { get; init; }
    public FamilyId FamilyId { get; init; }
}
