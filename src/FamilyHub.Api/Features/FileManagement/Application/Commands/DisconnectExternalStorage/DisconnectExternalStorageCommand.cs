using FamilyHub.Api.Common.Infrastructure.FamilyScope;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.DisconnectExternalStorage;

public sealed record DisconnectExternalStorageCommand(
    ExternalConnectionId ConnectionId,
    FamilyId FamilyId
) : ICommand<DisconnectExternalStorageResult>, IFamilyScoped;
