using FluentValidation;

namespace FamilyHub.Modules.Auth.Application.Commands.CompleteZitadelLogin;

/// <summary>
/// Validator for CompleteZitadelLoginCommand.
/// </summary>
public sealed class CompleteZitadelLoginCommandValidator : AbstractValidator<CompleteZitadelLoginCommand>
{
    public CompleteZitadelLoginCommandValidator()
    {
        RuleFor(x => x.AuthorizationCode)
            .NotEmpty()
            .WithMessage("Authorization code is required");

        RuleFor(x => x.CodeVerifier)
            .NotEmpty()
            .WithMessage("Code verifier is required (PKCE)");
    }
}
