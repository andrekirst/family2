using FamilyHub.Api.Common.Infrastructure.Validation;
using FamilyHub.Api.Resources;
using FamilyHub.Common.Domain;
using FamilyHub.EventChain.Domain.Repositories;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FamilyHub.Api.Features.EventChain.Application.Commands.DeleteChainDefinition;

public sealed class DeleteChainDefinitionBusinessValidator : AbstractValidator<DeleteChainDefinitionCommand>, IBusinessValidator<DeleteChainDefinitionCommand>
{
    public DeleteChainDefinitionBusinessValidator(
        IChainDefinitionRepository repository,
        IStringLocalizer<DomainErrors> localizer)
    {
        RuleFor(x => x.Id)
            .MustAsync(async (id, ct) =>
            {
                var definition = await repository.GetByIdAsync(id, ct);
                return definition is not null;
            })
            .WithErrorCode(DomainErrorCodes.ChainDefinitionNotFound)
            .WithMessage(_ => localizer[DomainErrorCodes.ChainDefinitionNotFound].Value);
    }
}
