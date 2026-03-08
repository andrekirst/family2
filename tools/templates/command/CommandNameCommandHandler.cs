using FamilyHub.Common.Application;

namespace FamilyHub.Api.Features.ModuleName.Application.Commands.CommandName;

public sealed class CommandNameCommandHandler
    : ICommandHandler<CommandNameCommand, CommandNameResult>
{
    public async ValueTask<CommandNameResult> Handle(
        CommandNameCommand command,
        CancellationToken cancellationToken)
    {
        // TODO: Implement command handling logic
        await Task.CompletedTask;

        return new CommandNameResult(true);
    }
}
