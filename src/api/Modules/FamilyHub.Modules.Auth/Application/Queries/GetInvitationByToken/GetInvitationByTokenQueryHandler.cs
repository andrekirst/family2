using FamilyHub.Modules.Family.Domain.Repositories;

namespace FamilyHub.Modules.Auth.Application.Queries.GetInvitationByToken;

/// <summary>
/// Handles queries to retrieve invitation details by token.
/// Validation (invitation exists and is pending) is handled by GetInvitationByTokenQueryValidator.
/// </summary>
public sealed class GetInvitationByTokenQueryHandler(
    IFamilyMemberInvitationRepository invitationRepository)
    : IRequestHandler<GetInvitationByTokenQuery, GetInvitationByTokenResult?>
{
    public async Task<GetInvitationByTokenResult?> Handle(
        GetInvitationByTokenQuery request,
        CancellationToken cancellationToken)
    {
        // Fetch invitation by token (validator already confirmed it exists and is pending)
        var invitation = await invitationRepository.GetByTokenAsync(
            request.Token,
            cancellationToken);

        // Map domain entity to DTO
        return invitation != null
            ? GetInvitationByTokenResult.FromDomain(invitation)
            : null;
    }
}
