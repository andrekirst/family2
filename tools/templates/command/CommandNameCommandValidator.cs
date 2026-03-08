using FamilyHub.Api.Common.Infrastructure.Validation;
using FamilyHub.Api.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FamilyHub.Api.Features.ModuleName.Application.Commands.CommandName;

public sealed class CommandNameCommandValidator : AbstractValidator<CommandNameCommand>, IInputValidator<CommandNameCommand>
{
    public CommandNameCommandValidator(IStringLocalizer<ValidationMessages> localizer)
    {
        RuleFor(x => x.FamilyId)
            .NotNull()
            .WithMessage(_ => localizer["FamilyIdRequired"]);
    }
}
