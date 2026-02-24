using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.MoveFile;

public sealed record MoveFileCommand(
    FileId FileId,
    FolderId TargetFolderId,
    FamilyId FamilyId,
    UserId MovedBy
) : ICommand<MoveFileResult>;
