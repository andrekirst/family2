using FamilyHub.Modules.Auth.Domain.Repositories;
using FamilyHub.Modules.Family.Domain.Repositories;
using FamilyHub.Modules.Family.Domain.Specifications;
using FamilyHub.SharedKernel.Application.CQRS;

namespace FamilyHub.Modules.Auth.Application.Queries.GetInvitationByToken;

/// <summary>
/// Handles queries to retrieve invitation details by token.
/// Validation (invitation exists and is pending) is handled by GetInvitationByTokenQueryValidator.
/// </summary>
/// <param name="invitationRepository">Repository for invitation data access.</param>
/// <param name="familyRepository">Repository for family data access.</param>
/// <param name="userRepository">Repository for user data access.</param>
public sealed class GetInvitationByTokenQueryHandler(
    IFamilyMemberInvitationRepository invitationRepository,
    IFamilyRepository familyRepository,
    IUserRepository userRepository)
    : IQueryHandler<GetInvitationByTokenQuery, GetInvitationByTokenResult?>
{
    /// <inheritdoc />
    public async Task<GetInvitationByTokenResult?> Handle(
        GetInvitationByTokenQuery request,
        CancellationToken cancellationToken)
    {
        // Fetch invitation by token (validator already confirmed it exists and is pending)
        var invitation = await invitationRepository.FindOneAsync(
            new InvitationByTokenSpecification(request.Token),
            cancellationToken);

        if (invitation == null)
        {
            return null;
        }

        // Fetch family information
        var family = await familyRepository.GetByIdAsync(invitation.FamilyId, cancellationToken);

        if (family == null)
        {
            return null; // Orphaned invitation - family was deleted
        }

        // Get member count from users in the family
        var members = await userRepository.GetByFamilyIdAsync(invitation.FamilyId, cancellationToken);
        var memberCount = members.Count;

        // Map domain entity to DTO with family info
        return GetInvitationByTokenResult.FromDomain(invitation, family, memberCount);
    }
}
