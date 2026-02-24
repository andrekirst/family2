using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.ProcessInboxFiles;

public sealed record ProcessInboxFilesCommand(
    FamilyId FamilyId,
    UserId UserId
) : ICommand<ProcessInboxFilesResult>;
