using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;
using FamilyHub.EventChain.Domain.Entities;
using FamilyHub.EventChain.Domain.Repositories;
using FamilyHub.EventChain.Infrastructure.Registry;

namespace FamilyHub.Api.Features.EventChain.Application.Commands.UpdateChainDefinition;

public sealed class UpdateChainDefinitionCommandHandler(
    IChainDefinitionRepository repository,
    IChainRegistry registry,
    TimeProvider timeProvider)
    : ICommandHandler<UpdateChainDefinitionCommand, Result<UpdateChainDefinitionResult>>
{
    public async ValueTask<Result<UpdateChainDefinitionResult>> Handle(
        UpdateChainDefinitionCommand command,
        CancellationToken cancellationToken)
    {
        var utcNow = timeProvider.GetUtcNow();
        var definition = await repository.GetByIdWithStepsAsync(command.Id, cancellationToken);

        if (definition is null)
        {
            return DomainError.NotFound(DomainErrorCodes.ChainDefinitionNotFound, "Chain definition not found");
        }

        definition.Update(command.Name, command.Description, command.IsEnabled, utcNow);

        if (command.Steps is not null)
        {
            definition.ClearSteps();

            foreach (var stepCmd in command.Steps.OrderBy(s => s.Order))
            {
                if (!registry.IsValidAction(stepCmd.ActionType, stepCmd.ActionVersion.Value))
                {
                    return DomainError.BusinessRule(
                        DomainErrorCodes.UnknownActionType,
                        $"Unknown action: {stepCmd.ActionType}@{stepCmd.ActionVersion.Value}");
                }

                var action = registry.GetAction(stepCmd.ActionType, stepCmd.ActionVersion.Value)!;

                var step = ChainDefinitionStep.Create(
                    definition.Id,
                    stepCmd.Alias,
                    stepCmd.Name,
                    stepCmd.ActionType,
                    stepCmd.ActionVersion,
                    action.Module,
                    stepCmd.InputMappings,
                    stepCmd.Condition,
                    action.IsCompensatable,
                    null,
                    stepCmd.Order);

                definition.AddStep(step);
            }
        }

        await repository.UpdateAsync(definition, cancellationToken);

        return new UpdateChainDefinitionResult(definition.Id, definition);
    }
}
