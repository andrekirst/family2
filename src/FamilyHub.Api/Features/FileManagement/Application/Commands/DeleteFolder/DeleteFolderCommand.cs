using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.DeleteFolder;

public sealed record DeleteFolderCommand(
    FolderId FolderId,
    FamilyId FamilyId,
    UserId DeletedBy
) : ICommand<DeleteFolderResult>;
