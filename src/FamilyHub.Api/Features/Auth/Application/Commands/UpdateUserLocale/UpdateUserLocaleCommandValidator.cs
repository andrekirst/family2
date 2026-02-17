using FluentValidation;

namespace FamilyHub.Api.Features.Auth.Application.Commands.UpdateUserLocale;

/// <summary>
/// Validator for UpdateUserLocaleCommand.
/// Ensures the locale is one of the supported values.
/// </summary>
public sealed class UpdateUserLocaleCommandValidator : AbstractValidator<UpdateUserLocaleCommand>
{
    private static readonly string[] SupportedLocales = ["en", "de"];

    public UpdateUserLocaleCommandValidator()
    {
        RuleFor(x => x.ExternalUserId)
            .NotNull()
            .WithMessage("External user ID is required");

        RuleFor(x => x.Locale)
            .NotEmpty()
            .WithMessage("Locale is required")
            .Must(locale => SupportedLocales.Contains(locale))
            .WithMessage("Locale must be one of: en, de");
    }
}
