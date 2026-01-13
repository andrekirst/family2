using FamilyHub.Modules.Family.Domain.Specifications;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using FluentValidation;

namespace FamilyHub.Modules.Auth.Application.Queries.GetInvitationByToken;

/// <summary>
/// Validator for GetInvitationByTokenQuery.
/// Validates that the invitation token corresponds to a valid, pending invitation.
/// </summary>
public sealed class GetInvitationByTokenQueryValidator : AbstractValidator<GetInvitationByTokenQuery>
{
    private readonly IFamilyMemberInvitationRepository _invitationRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetInvitationByTokenQueryValidator"/> class.
    /// </summary>
    /// <param name="invitationRepository">Repository for invitation data access.</param>
    public GetInvitationByTokenQueryValidator(IFamilyMemberInvitationRepository invitationRepository)
    {
        _invitationRepository = invitationRepository;

        RuleFor(x => x.Token)
            .NotNull()
            .WithMessage("Invitation token is required")
            .MustAsync(BeValidPendingInvitation)
            .WithMessage("Invitation not found or not pending");
    }

    private async Task<bool> BeValidPendingInvitation(InvitationToken token, CancellationToken cancellationToken)
    {
        var invitation = await _invitationRepository.FindOneAsync(
            new InvitationByTokenSpecification(token),
            cancellationToken);

        // Not found or not pending
        return invitation != null && invitation.Status == InvitationStatus.Pending;
    }
}
