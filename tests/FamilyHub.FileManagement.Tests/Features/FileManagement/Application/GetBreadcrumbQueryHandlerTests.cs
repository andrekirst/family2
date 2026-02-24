using FamilyHub.Api.Features.FileManagement.Application.Queries.GetBreadcrumb;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class GetBreadcrumbQueryHandlerTests
{
    private static (GetBreadcrumbQueryHandler handler, FakeFolderRepository folderRepo) CreateHandler()
    {
        var folderRepo = new FakeFolderRepository();
        var handler = new GetBreadcrumbQueryHandler(folderRepo);
        return (handler, folderRepo);
    }

    [Fact]
    public async Task Handle_ShouldReturnBreadcrumbFromRootToTarget()
    {
        var familyId = FamilyId.New();
        var userId = UserId.New();
        var (handler, folderRepo) = CreateHandler();

        var root = Folder.CreateRoot(familyId, userId);
        folderRepo.Folders.Add(root);

        var documents = Folder.Create(
            FileName.From("Documents"), root.Id, $"/{root.Id.Value}/", familyId, userId);
        folderRepo.Folders.Add(documents);

        var taxes = Folder.Create(
            FileName.From("Taxes"), documents.Id,
            $"/{root.Id.Value}/{documents.Id.Value}/",
            familyId, userId);
        folderRepo.Folders.Add(taxes);

        var query = new GetBreadcrumbQuery(taxes.Id, familyId);
        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(3);
        result[0].Id.Should().Be(root.Id.Value);
        result[1].Id.Should().Be(documents.Id.Value);
        result[2].Id.Should().Be(taxes.Id.Value);
    }

    [Fact]
    public async Task Handle_ShouldReturnOnlyTargetForRootChild()
    {
        var familyId = FamilyId.New();
        var userId = UserId.New();
        var (handler, folderRepo) = CreateHandler();

        var root = Folder.CreateRoot(familyId, userId);
        folderRepo.Folders.Add(root);

        var folder = Folder.Create(
            FileName.From("Documents"), root.Id, $"/{root.Id.Value}/", familyId, userId);
        folderRepo.Folders.Add(folder);

        var query = new GetBreadcrumbQuery(folder.Id, familyId);
        var result = await handler.Handle(query, CancellationToken.None);

        // Root ancestor + the folder itself
        result.Should().HaveCount(2);
        result[0].Id.Should().Be(root.Id.Value);
        result[1].Id.Should().Be(folder.Id.Value);
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenFolderNotFound()
    {
        var (handler, _) = CreateHandler();

        var query = new GetBreadcrumbQuery(FolderId.New(), FamilyId.New());
        var act = () => handler.Handle(query, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == DomainErrorCodes.FolderNotFound);
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenFolderBelongsToDifferentFamily()
    {
        var familyId = FamilyId.New();
        var userId = UserId.New();
        var (handler, folderRepo) = CreateHandler();

        var root = Folder.CreateRoot(familyId, userId);
        folderRepo.Folders.Add(root);

        var folder = Folder.Create(
            FileName.From("Docs"), root.Id, $"/{root.Id.Value}/", familyId, userId);
        folderRepo.Folders.Add(folder);

        var query = new GetBreadcrumbQuery(folder.Id, FamilyId.New());
        var act = () => handler.Handle(query, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == DomainErrorCodes.Forbidden);
    }
}
