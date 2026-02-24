using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.CreateFolder;

public sealed record CreateFolderCommand(
    FileName Name,
    FolderId? ParentFolderId,
    FamilyId FamilyId,
    UserId CreatedBy
) : ICommand<CreateFolderResult>;
