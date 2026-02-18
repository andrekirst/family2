using FamilyHub.Api.Features.FileManagement.Application.Mappers;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.GetBreadcrumb;

public sealed class GetBreadcrumbQueryHandler(
    IFolderRepository folderRepository)
    : IQueryHandler<GetBreadcrumbQuery, List<FolderDto>>
{
    public async ValueTask<List<FolderDto>> Handle(
        GetBreadcrumbQuery query,
        CancellationToken cancellationToken)
    {
        var folder = await folderRepository.GetByIdAsync(query.FolderId, cancellationToken)
            ?? throw new DomainException("Folder not found", DomainErrorCodes.FolderNotFound);

        if (folder.FamilyId != query.FamilyId)
        {
            throw new DomainException("Folder belongs to a different family", DomainErrorCodes.Forbidden);
        }

        var ancestors = await folderRepository.GetAncestorsAsync(query.FolderId, cancellationToken);

        // Ancestors are root → parent chain; append the current folder
        var breadcrumb = ancestors
            .Select(FileManagementMapper.ToDto)
            .ToList();

        breadcrumb.Add(FileManagementMapper.ToDto(folder));

        return breadcrumb;
    }
}
