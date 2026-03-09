using FamilyHub.Api.Features.FileManagement.Application.Queries.GetBreadcrumb;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class GetBreadcrumbQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnBreadcrumbFromRootToTarget()
    {
        var familyId = FamilyId.New();
        var userId = UserId.New();
        var folderRepo = Substitute.For<IFolderRepository>();
        var handler = new GetBreadcrumbQueryHandler(folderRepo);

        var root = Folder.CreateRoot(familyId, userId, DateTimeOffset.UtcNow);
        var documents = Folder.Create(
            FileName.From("Documents"), root.Id, $"/{root.Id.Value}/", familyId, userId, DateTimeOffset.UtcNow);
        var taxes = Folder.Create(
            FileName.From("Taxes"), documents.Id,
            $"/{root.Id.Value}/{documents.Id.Value}/",
            familyId, userId, DateTimeOffset.UtcNow);

        folderRepo.GetByIdAsync(taxes.Id, Arg.Any<CancellationToken>()).Returns(taxes);
        folderRepo.GetAncestorsAsync(taxes.Id, Arg.Any<CancellationToken>())
            .Returns(new List<Folder> { root, documents });

        var query = new GetBreadcrumbQuery(taxes.Id)
        {
            FamilyId = familyId,
            UserId = UserId.New()
        };
        var result = await handler.Handle(query, CancellationToken.None);

        result.Value.Should().HaveCount(3);
        result.Value[0].Id.Should().Be(root.Id.Value);
        result.Value[1].Id.Should().Be(documents.Id.Value);
        result.Value[2].Id.Should().Be(taxes.Id.Value);
    }

    [Fact]
    public async Task Handle_ShouldReturnOnlyTargetForRootChild()
    {
        var familyId = FamilyId.New();
        var userId = UserId.New();
        var folderRepo = Substitute.For<IFolderRepository>();
        var handler = new GetBreadcrumbQueryHandler(folderRepo);

        var root = Folder.CreateRoot(familyId, userId, DateTimeOffset.UtcNow);
        var folder = Folder.Create(
            FileName.From("Documents"), root.Id, $"/{root.Id.Value}/", familyId, userId, DateTimeOffset.UtcNow);

        folderRepo.GetByIdAsync(folder.Id, Arg.Any<CancellationToken>()).Returns(folder);
        folderRepo.GetAncestorsAsync(folder.Id, Arg.Any<CancellationToken>())
            .Returns(new List<Folder> { root });

        var query = new GetBreadcrumbQuery(folder.Id)
        {
            FamilyId = familyId,
            UserId = UserId.New()
        };
        var result = await handler.Handle(query, CancellationToken.None);

        result.Value.Should().HaveCount(2);
        result.Value[0].Id.Should().Be(root.Id.Value);
        result.Value[1].Id.Should().Be(folder.Id.Value);
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenFolderNotFound()
    {
        var folderRepo = Substitute.For<IFolderRepository>();
        var handler = new GetBreadcrumbQueryHandler(folderRepo);

        folderRepo.GetByIdAsync(FolderId.New(), Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs((Folder?)null);

        var query = new GetBreadcrumbQuery(FolderId.New())
        {
            FamilyId = FamilyId.New(),
            UserId = UserId.New()
        };
        var result = await handler.Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be(DomainErrorCodes.FolderNotFound);
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenFolderBelongsToDifferentFamily()
    {
        var familyId = FamilyId.New();
        var userId = UserId.New();
        var folderRepo = Substitute.For<IFolderRepository>();
        var handler = new GetBreadcrumbQueryHandler(folderRepo);

        var root = Folder.CreateRoot(familyId, userId, DateTimeOffset.UtcNow);
        var folder = Folder.Create(
            FileName.From("Docs"), root.Id, $"/{root.Id.Value}/", familyId, userId, DateTimeOffset.UtcNow);

        folderRepo.GetByIdAsync(folder.Id, Arg.Any<CancellationToken>()).Returns(folder);

        var query = new GetBreadcrumbQuery(folder.Id)
        {
            FamilyId = FamilyId.New(),
            UserId = UserId.New()
        };
        var result = await handler.Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be(DomainErrorCodes.Forbidden);
    }
}
