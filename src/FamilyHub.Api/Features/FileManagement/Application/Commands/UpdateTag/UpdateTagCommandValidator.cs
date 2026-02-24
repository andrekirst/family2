using FamilyHub.Api.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.UpdateTag;

public sealed class UpdateTagCommandValidator : AbstractValidator<UpdateTagCommand>
{
    public UpdateTagCommandValidator(IStringLocalizer<ValidationMessages> localizer)
    {
        RuleFor(x => x.TagId).NotNull();
        RuleFor(x => x.FamilyId).NotNull();
    }
}
