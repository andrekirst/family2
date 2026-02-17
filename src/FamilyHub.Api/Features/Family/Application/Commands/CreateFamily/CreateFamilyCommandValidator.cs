using FamilyHub.Api.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FamilyHub.Api.Features.Family.Application.Commands.CreateFamily;

/// <summary>
/// Validator for CreateFamilyCommand.
/// Note: Vogen value objects already enforce basic validation,
/// this validator provides additional business rules.
/// </summary>
public sealed class CreateFamilyCommandValidator : AbstractValidator<CreateFamilyCommand>
{
    public CreateFamilyCommandValidator(IStringLocalizer<ValidationMessages> localizer)
    {
        RuleFor(x => x.Name)
            .NotNull()
            .WithMessage(_ => localizer["FamilyNameRequired"]);

        RuleFor(x => x.OwnerId)
            .NotNull()
            .WithMessage(_ => localizer["OwnerIdRequired"]);
    }
}
