using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.ModuleName.Application.Commands.CommandName;

[ExtendObjectType<FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes.RootMutation>]
public sealed class CommandNameMutationType
{
    [Authorize]
    public async Task<CommandNameResult> CommandName(
        CommandNameCommand command,
        [Service] Mediator.IMediator mediator,
        CancellationToken cancellationToken)
    {
        return await mediator.Send(command, cancellationToken);
    }
}
