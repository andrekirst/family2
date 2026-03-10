using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.RenameFolder;

public sealed record RenameFolderResult(FolderId FolderId, Folder RenamedFolder);
