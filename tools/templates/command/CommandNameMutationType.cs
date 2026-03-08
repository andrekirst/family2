using FamilyHub.Common.Application;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.ModuleName.Application.Commands.CommandName;

[ExtendObjectType("Mutation")]
public sealed class CommandNameMutationType
{
    [Authorize]
    public async Task<CommandNameResult> CommandNameAsync(
        CommandNameCommand command,
        [Service] ICommandBus commandBus,
        CancellationToken cancellationToken)
    {
        return await commandBus.SendAsync(command, cancellationToken);
    }
}
