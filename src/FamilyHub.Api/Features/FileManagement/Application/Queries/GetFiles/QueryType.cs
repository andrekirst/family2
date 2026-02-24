using System.Security.Claims;
using FamilyHub.Api.Common.Infrastructure;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.GetFiles;

[ExtendObjectType(typeof(FileManagementQuery))]
public class QueryType
{
    [Authorize]
    public async Task<List<StoredFileDto>> GetFiles(
        Guid? folderId,
        ClaimsPrincipal claimsPrincipal,
        [Service] IQueryBus queryBus,
        [Service] IUserRepository userRepository,
        [Service] IFolderRepository folderRepository,
        CancellationToken cancellationToken)
    {
        var externalUserIdString = claimsPrincipal.FindFirst(ClaimNames.Sub)?.Value
            ?? throw new UnauthorizedAccessException("User not authenticated");

        var user = await userRepository.GetByExternalIdAsync(
            ExternalUserId.From(externalUserIdString), cancellationToken)
            ?? throw new UnauthorizedAccessException("User not found");

        var familyId = user.FamilyId
            ?? throw new UnauthorizedAccessException("User is not a member of any family");

        var effectiveFolderId = await ResolveRootFolderIdAsync(
            folderId, familyId, user.Id, folderRepository, cancellationToken);

        var query = new GetFilesQuery(effectiveFolderId, familyId);
        return await queryBus.QueryAsync(query, cancellationToken);
    }

    private static async Task<FolderId> ResolveRootFolderIdAsync(
        Guid? folderId,
        FamilyId familyId,
        UserId userId,
        IFolderRepository folderRepository,
        CancellationToken cancellationToken)
    {
        if (folderId.HasValue && folderId.Value != Guid.Empty)
            return FolderId.From(folderId.Value);

        var rootFolder = await folderRepository.GetRootFolderAsync(familyId, cancellationToken);
        if (rootFolder is null)
        {
            rootFolder = Folder.CreateRoot(familyId, userId);
            await folderRepository.AddAsync(rootFolder, cancellationToken);
        }

        return rootFolder.Id;
    }
}
