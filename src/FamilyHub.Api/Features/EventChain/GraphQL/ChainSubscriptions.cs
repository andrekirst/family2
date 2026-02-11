using FamilyHub.Api.Features.EventChain.Models;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.EventChain.GraphQL;

[ExtendObjectType("Subscription")]
public class ChainSubscriptions
{
    [Authorize]
    [Subscribe]
    [Topic("ChainExecutionUpdated_{familyId}")]
    public ChainExecutionDto ChainExecutionUpdated(
        Guid familyId,
        [EventMessage] ChainExecutionDto execution)
        => execution;

    [Authorize]
    [Subscribe]
    [Topic("StepExecutionUpdated_{chainExecutionId}")]
    public StepExecutionDto StepExecutionUpdated(
        Guid chainExecutionId,
        [EventMessage] StepExecutionDto stepExecution)
        => stepExecution;
}
