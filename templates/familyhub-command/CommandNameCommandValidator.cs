using FamilyHub.Api.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FamilyHub.Api.Features.ModuleName.Application.Commands.CommandName;

public sealed class CommandNameCommandValidator : AbstractValidator<CommandNameCommand>
{
    public CommandNameCommandValidator(
        IStringLocalizer<ValidationMessages> localizer)
    {
        // TODO: Add validation rules
    }
}
