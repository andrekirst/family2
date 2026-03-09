using FamilyHub.Api.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FamilyHub.Api.Features.Auth.Application.Commands.UpdateLastLogin;

/// <summary>
/// Validator for UpdateLastLoginCommand.
/// </summary>
public sealed class UpdateLastLoginCommandValidator : AbstractValidator<UpdateLastLoginCommand>
{
    public UpdateLastLoginCommandValidator(
        IStringLocalizer<ValidationMessages> localizer,
        TimeProvider timeProvider)
    {
        RuleFor(x => x.ExternalUserId)
            .NotNull()
            .WithMessage(_ => localizer["ExternalUserIdRequired"]);

        RuleFor(x => x.LoginTime)
            .Must(loginTime => loginTime <= timeProvider.GetUtcNow().UtcDateTime.AddMinutes(5))
            .WithMessage(_ => localizer["LoginTimeCannotBeFuture"]);
    }
}
