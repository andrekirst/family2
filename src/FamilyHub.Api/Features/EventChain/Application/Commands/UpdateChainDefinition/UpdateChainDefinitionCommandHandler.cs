using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;
using FamilyHub.EventChain.Domain.Entities;
using FamilyHub.EventChain.Domain.Repositories;
using FamilyHub.EventChain.Infrastructure.Registry;

namespace FamilyHub.Api.Features.EventChain.Application.Commands.UpdateChainDefinition;

public sealed class UpdateChainDefinitionCommandHandler(
    IChainDefinitionRepository repository,
    IChainRegistry registry)
    : ICommandHandler<UpdateChainDefinitionCommand, UpdateChainDefinitionResult>
{
    public async ValueTask<UpdateChainDefinitionResult> Handle(
        UpdateChainDefinitionCommand command,
        CancellationToken cancellationToken)
    {
        var definition = await repository.GetByIdWithStepsAsync(command.Id, cancellationToken)
            ?? throw new DomainException("Chain definition not found", DomainErrorCodes.ChainDefinitionNotFound);

        definition.Update(command.Name, command.Description, command.IsEnabled);

        if (command.Steps is not null)
        {
            definition.ClearSteps();

            foreach (var stepCmd in command.Steps.OrderBy(s => s.Order))
            {
                if (!registry.IsValidAction(stepCmd.ActionType, stepCmd.ActionVersion.Value))
                    throw new InvalidOperationException(
                        $"Unknown action: {stepCmd.ActionType}@{stepCmd.ActionVersion.Value}");

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

        return new UpdateChainDefinitionResult(definition.Id);
    }
}
