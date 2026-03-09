using FamilyHub.Common.Application;

namespace FamilyHub.Api.Features.ModuleName.Application.Commands.CommandName;

public sealed class CommandNameCommandHandler(
    TimeProvider timeProvider)
    : ICommandHandler<CommandNameCommand, CommandNameResult>
{
    public async ValueTask<CommandNameResult> Handle(
        CommandNameCommand command,
        CancellationToken cancellationToken)
    {
        var utcNow = timeProvider.GetUtcNow();

        // TODO: Implement command logic

        return new CommandNameResult(true);
    }
}
