using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.GetBreadcrumb;

public sealed record GetBreadcrumbQuery(
    FolderId FolderId,
    FamilyId FamilyId
) : IQuery<List<FolderDto>>;
