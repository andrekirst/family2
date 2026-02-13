using FamilyHub.Api.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FamilyHub.Api.Features.EventChain.Application.Commands.CreateChainDefinition;

public sealed class CreateChainDefinitionCommandValidator : AbstractValidator<CreateChainDefinitionCommand>
{
    public CreateChainDefinitionCommandValidator(IStringLocalizer<ValidationMessages> localizer)
    {
        RuleFor(x => x.Name.Value)
            .NotEmpty().WithMessage(_ => localizer["ChainNameRequired"])
            .MaximumLength(200).WithMessage(_ => localizer["ChainNameMaxLength"]);

        RuleFor(x => x.TriggerEventType)
            .NotEmpty().WithMessage(_ => localizer["TriggerEventTypeRequired"]);

        RuleFor(x => x.Steps)
            .NotEmpty().WithMessage(_ => localizer["AtLeastOneStepRequired"]);

        RuleForEach(x => x.Steps).ChildRules(step =>
        {
            step.RuleFor(s => s.Alias.Value)
                .NotEmpty().WithMessage(_ => localizer["StepAliasRequired"])
                .MaximumLength(50).WithMessage(_ => localizer["StepAliasMaxLength"]);

            step.RuleFor(s => s.Name)
                .NotEmpty().WithMessage(_ => localizer["StepNameRequired"]);

            step.RuleFor(s => s.ActionType)
                .NotEmpty().WithMessage(_ => localizer["ActionTypeRequired"]);

            step.RuleFor(s => s.Order)
                .GreaterThan(0).WithMessage(_ => localizer["StepOrderMustBePositive"]);
        });
    }
}
