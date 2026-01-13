using FamilyHub.Modules.Auth.Application.Abstractions;
using FamilyHub.Modules.Auth.Domain.Repositories;
using MediatR;

namespace FamilyHub.Modules.Auth.Application.Queries.GetPendingInvitations;

/// <summary>
/// Handles queries to retrieve pending invitations for the authenticated user's family.
/// User context and authorization are handled by pipeline behaviors.
/// </summary>
/// <param name="userContext">The current authenticated user context.</param>
/// <param name="invitationRepository">Repository for invitation data access.</param>
public sealed partial class GetPendingInvitationsQueryHandler(
    IUserContext userContext,
    IFamilyMemberInvitationRepository invitationRepository)
    : IRequestHandler<GetPendingInvitationsQuery, GetPendingInvitationsResult>
{
    /// <inheritdoc />
    public async Task<GetPendingInvitationsResult> Handle(
        GetPendingInvitationsQuery request,
        CancellationToken cancellationToken)
    {
        // Get FamilyId from user context (already loaded and validated by behaviors)
        var familyId = userContext.FamilyId;

        // Fetch pending invitations from repository
        var invitations = await invitationRepository.GetPendingByFamilyIdAsync(
            familyId,
            cancellationToken);

        // Map domain entities to DTOs
        return new GetPendingInvitationsResult
        {
            Invitations = invitations
                .Select(PendingInvitationDto.FromDomain)
                .ToList()
        };
    }
}
