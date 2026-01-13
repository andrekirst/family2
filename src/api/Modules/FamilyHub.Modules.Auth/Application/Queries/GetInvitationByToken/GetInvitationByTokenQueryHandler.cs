using FamilyHub.Modules.Family.Domain.Specifications;
using FamilyHub.SharedKernel.Application.CQRS;

namespace FamilyHub.Modules.Auth.Application.Queries.GetInvitationByToken;

/// <summary>
/// Handles queries to retrieve invitation details by token.
/// Validation (invitation exists and is pending) is handled by GetInvitationByTokenQueryValidator.
/// </summary>
/// <param name="invitationRepository">Repository for invitation data access.</param>
public sealed class GetInvitationByTokenQueryHandler(
    IFamilyMemberInvitationRepository invitationRepository)
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

        // Map domain entity to DTO
        return invitation != null
            ? GetInvitationByTokenResult.FromDomain(invitation)
            : null;
    }
}
