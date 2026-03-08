using FamilyHub.Api.Common.Infrastructure.Validation;
using FamilyHub.Api.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace FamilyHub.Api.Features.Auth.Application.Commands.UpdateUserLocale;

/// <summary>
/// Validator for UpdateUserLocaleCommand.
/// Ensures the locale is one of the supported values.
/// </summary>
public sealed class UpdateUserLocaleCommandValidator : AbstractValidator<UpdateUserLocaleCommand>, IInputValidator<UpdateUserLocaleCommand>
{
    public UpdateUserLocaleCommandValidator(
        IStringLocalizer<ValidationMessages> localizer,
        IOptions<RequestLocalizationOptions> localizationOptions)
    {
        var supportedLocales = localizationOptions.Value.SupportedUICultures?
            .Select(c => c.Name).ToArray() ?? ["en"];

        RuleFor(x => x.ExternalUserId)
            .NotNull()
            .WithMessage(_ => localizer["ExternalUserIdRequired"].Value);

        RuleFor(x => x.Locale)
            .NotEmpty()
            .WithMessage(_ => localizer["LocaleRequired"].Value)
            .Must(locale => supportedLocales.Contains(locale))
            .WithMessage(_ => localizer["LocaleNotSupported"].Value);
    }
}
