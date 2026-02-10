using FluentValidation;

namespace FamilyHub.Api.Features.Family.Application.Commands.SendInvitation;

public class SendInvitationCommandValidator : AbstractValidator<SendInvitationCommand>
{
    public SendInvitationCommandValidator()
    {
        RuleFor(x => x.InviteeEmail.Value)
            .NotEmpty().WithMessage("Invitee email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.Role.Value)
            .Must(role => role is "Admin" or "Member")
            .WithMessage("Invitation role must be 'Admin' or 'Member' (Owner role cannot be assigned via invitation)");
    }
}
