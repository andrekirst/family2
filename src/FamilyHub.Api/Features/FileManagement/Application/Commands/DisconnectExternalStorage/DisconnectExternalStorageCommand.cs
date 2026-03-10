using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.DisconnectExternalStorage;

public sealed record DisconnectExternalStorageCommand(
    ExternalConnectionId ConnectionId
) : ICommand<Result<DisconnectExternalStorageResult>>, IRequireFamily
{
    public UserId UserId { get; init; }
    public FamilyId FamilyId { get; init; }
}
