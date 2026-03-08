using FamilyHub.Api.Common.Infrastructure.Validation;
using FamilyHub.Api.Resources;
using FamilyHub.Common.Domain;
using FamilyHub.EventChain.Domain.Repositories;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FamilyHub.Api.Features.EventChain.Application.Commands.ExecuteChain;

public sealed class ExecuteChainBusinessValidator : AbstractValidator<ExecuteChainCommand>, IBusinessValidator<ExecuteChainCommand>
{
    public ExecuteChainBusinessValidator(
        IChainDefinitionRepository definitionRepository,
        IStringLocalizer<DomainErrors> localizer)
    {
        RuleFor(x => x.ChainDefinitionId)
            .MustAsync(async (chainDefinitionId, ct) =>
            {
                var definition = await definitionRepository.GetByIdWithStepsAsync(chainDefinitionId, ct);
                return definition is not null;
            })
            .WithErrorCode(DomainErrorCodes.ChainDefinitionNotFound)
            .WithMessage(_ => localizer[DomainErrorCodes.ChainDefinitionNotFound].Value);
    }
}
