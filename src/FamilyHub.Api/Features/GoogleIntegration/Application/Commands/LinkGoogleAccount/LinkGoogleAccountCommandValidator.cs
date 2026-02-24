using FluentValidation;

namespace FamilyHub.Api.Features.GoogleIntegration.Application.Commands.LinkGoogleAccount;

public sealed class LinkGoogleAccountCommandValidator : AbstractValidator<LinkGoogleAccountCommand>
{
    public LinkGoogleAccountCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().WithMessage("Authorization code is required");
        RuleFor(x => x.State).NotEmpty().WithMessage("OAuth state is required");
    }
}
