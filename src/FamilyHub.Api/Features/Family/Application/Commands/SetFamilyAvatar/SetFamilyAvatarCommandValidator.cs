using FluentValidation;

namespace FamilyHub.Api.Features.Family.Application.Commands.SetFamilyAvatar;

public sealed class SetFamilyAvatarCommandValidator : AbstractValidator<SetFamilyAvatarCommand>
{
    public SetFamilyAvatarCommandValidator()
    {
        RuleFor(x => x.UserId).NotNull().WithMessage("User ID is required");
        RuleFor(x => x.FamilyId).NotNull().WithMessage("Family ID is required");
        RuleFor(x => x.AvatarId).NotNull().WithMessage("Avatar ID is required");
    }
}
