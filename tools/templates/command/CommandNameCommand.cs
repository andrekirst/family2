using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.ModuleName.Application.Commands.CommandName;

public sealed record CommandNameCommand(
) : ICommand<CommandNameResult>, IRequireFamily
{
    public UserId UserId { get; init; }
    public FamilyId FamilyId { get; init; }
}
