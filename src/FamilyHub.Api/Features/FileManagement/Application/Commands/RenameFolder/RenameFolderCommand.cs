using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.RenameFolder;

public sealed record RenameFolderCommand(
    FolderId FolderId,
    FileName NewName,
    FamilyId FamilyId,
    UserId RenamedBy
) : ICommand<RenameFolderResult>;
