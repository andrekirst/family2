using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.MoveFolder;

public sealed record MoveFolderCommand(
    FolderId FolderId,
    FolderId TargetParentFolderId,
    FamilyId FamilyId,
    UserId MovedBy
) : ICommand<MoveFolderResult>;
