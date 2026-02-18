using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.GetFolders;

public sealed record GetFoldersQuery(
    FolderId ParentFolderId,
    FamilyId FamilyId
) : IQuery<List<FolderDto>>;
