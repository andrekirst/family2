using FluentValidation;

namespace FamilyHub.Api.Features.Family.Application.Commands.RemoveAvatar;

public sealed class RemoveAvatarCommandValidator : AbstractValidator<RemoveAvatarCommand>
{
    public RemoveAvatarCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotNull()
            .WithMessage("User ID is required");
    }
}
