using FluentValidation;

namespace FamilyHub.Api.Features.Auth.Application.Commands.UpdateLastLogin;

/// <summary>
/// Validator for UpdateLastLoginCommand.
/// </summary>
public sealed class UpdateLastLoginCommandValidator : AbstractValidator<UpdateLastLoginCommand>
{
    public UpdateLastLoginCommandValidator()
    {
        RuleFor(x => x.ExternalUserId)
            .NotNull()
            .WithMessage("External user ID is required");

        RuleFor(x => x.LoginTime)
            .LessThanOrEqualTo(DateTime.UtcNow.AddMinutes(5))
            .WithMessage("Login time cannot be in the future");
    }
}
