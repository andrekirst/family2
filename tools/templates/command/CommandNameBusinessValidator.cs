using FamilyHub.Api.Common.Infrastructure.Validation;
using FamilyHub.Api.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FamilyHub.Api.Features.ModuleName.Application.Commands.CommandName;

public sealed class CommandNameBusinessValidator : AbstractValidator<CommandNameCommand>, IBusinessValidator<CommandNameCommand>
{
    public CommandNameBusinessValidator(IStringLocalizer<DomainErrors> localizer)
    {
        // TODO: Add business validation rules
    }
}
