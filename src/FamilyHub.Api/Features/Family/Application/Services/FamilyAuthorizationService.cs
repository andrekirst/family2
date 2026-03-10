using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Domain.Repositories;

namespace FamilyHub.Api.Features.Family.Application.Services;

/// <summary>
/// Service for checking family-level authorization.
/// Determines whether a user has permission to perform actions on a family.
/// </summary>
public class FamilyAuthorizationService(IFamilyMemberRepository familyMemberRepository)
{
    /// <summary>
    /// Checks if the user has permission to invite members to the family.
    /// Only Owner and Admin roles can send invitations.
    /// </summary>
    public async Task<bool> CanInviteAsync(UserId userId, FamilyId familyId, CancellationToken cancellationToken = default)
    {
        var member = await familyMemberRepository.GetByUserAndFamilyAsync(userId, familyId, cancellationToken);
        return member is not null && member.IsActive && member.Role.CanInvite();
    }
}
