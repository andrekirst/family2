using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.GetFolder;

public sealed record GetFolderQuery(
    FolderId FolderId,
    FamilyId FamilyId
) : IQuery<FolderDto?>;
