using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.RenameFile;

public sealed record RenameFileCommand(
    FileId FileId,
    FileName NewName,
    FamilyId FamilyId,
    UserId RenamedBy
) : ICommand<RenameFileResult>;
