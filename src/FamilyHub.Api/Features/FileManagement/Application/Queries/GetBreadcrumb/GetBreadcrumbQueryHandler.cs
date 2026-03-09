using FamilyHub.Api.Features.FileManagement.Application.Mappers;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.GetBreadcrumb;

public sealed class GetBreadcrumbQueryHandler(
    IFolderRepository folderRepository)
    : IQueryHandler<GetBreadcrumbQuery, Result<List<FolderDto>>>
{
    public async ValueTask<Result<List<FolderDto>>> Handle(
        GetBreadcrumbQuery query,
        CancellationToken cancellationToken)
    {
        var folder = await folderRepository.GetByIdAsync(query.FolderId, cancellationToken);
        if (folder is null)
        {
            return DomainError.NotFound(DomainErrorCodes.FolderNotFound, "Folder not found");
        }

        if (folder.FamilyId != query.FamilyId)
        {
            return DomainError.Forbidden(DomainErrorCodes.Forbidden, "Folder belongs to a different family");
        }

        var ancestors = await folderRepository.GetAncestorsAsync(query.FolderId, cancellationToken);

        var breadcrumb = ancestors
            .Select(FileManagementMapper.ToDto)
            .ToList();

        breadcrumb.Add(FileManagementMapper.ToDto(folder));

        return breadcrumb;
    }
}
