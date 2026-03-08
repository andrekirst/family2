using FamilyHub.Api.Common.Infrastructure.Validation;
using FamilyHub.Api.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FamilyHub.Api.Features.ModuleName.Application.Commands.CommandName;

public sealed class CommandNameAuthValidator : AbstractValidator<CommandNameCommand>, IAuthValidator<CommandNameCommand>
{
    public CommandNameAuthValidator(IStringLocalizer<DomainErrors> localizer)
    {
        // TODO: Add authorization validation rules
    }
}
