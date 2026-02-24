using FamilyHub.Api.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.CreateTag;

public sealed class CreateTagCommandValidator : AbstractValidator<CreateTagCommand>
{
    public CreateTagCommandValidator(IStringLocalizer<ValidationMessages> localizer)
    {
        RuleFor(x => x.Name).NotNull();
        RuleFor(x => x.Color).NotNull();
        RuleFor(x => x.FamilyId).NotNull();
        RuleFor(x => x.CreatedBy).NotNull();
    }
}
