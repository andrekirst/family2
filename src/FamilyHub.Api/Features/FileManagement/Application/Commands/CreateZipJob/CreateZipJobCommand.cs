using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.CreateZipJob;

public sealed record CreateZipJobCommand(
    FamilyId FamilyId,
    UserId InitiatedBy,
    List<Guid> FileIds
) : ICommand<CreateZipJobResult>;
