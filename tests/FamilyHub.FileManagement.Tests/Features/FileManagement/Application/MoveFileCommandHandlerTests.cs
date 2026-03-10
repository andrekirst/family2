using FamilyHub.Api.Features.FileManagement.Application.Commands.MoveFile;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class MoveFileCommandHandlerTests
{
    private static StoredFile CreateTestFile(FamilyId familyId, FolderId folderId)
    {
        return StoredFile.Create(
            FileName.From("document.pdf"),
            MimeType.From("application/pdf"),
            FileSize.From(1024),
            StorageKey.New(),
            Checksum.From("a".PadRight(64, 'a')),
            folderId,
            familyId,
            UserId.New(), DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task Handle_ShouldMoveFileToTargetFolder()
    {
        var familyId = FamilyId.New();
        var fileRepo = Substitute.For<IStoredFileRepository>();
        var folderRepo = Substitute.For<IFolderRepository>();

        var sourceFolder = Folder.CreateRoot(familyId, UserId.New(), DateTimeOffset.UtcNow);
        var targetFolder = Folder.Create(
            FileName.From("Target"),
            sourceFolder.Id,
            $"/{sourceFolder.Id.Value}/",
            familyId,
            UserId.New(), DateTimeOffset.UtcNow);

        var file = CreateTestFile(familyId, sourceFolder.Id);
        fileRepo.GetByIdAsync(file.Id, Arg.Any<CancellationToken>()).Returns(file);
        folderRepo.GetByIdAsync(targetFolder.Id, Arg.Any<CancellationToken>()).Returns(targetFolder);

        var handler = new MoveFileCommandHandler(fileRepo, folderRepo, TimeProvider.System);

        var command = new MoveFileCommand(
            file.Id,
            targetFolder.Id)
        {
            FamilyId = familyId,
            UserId = UserId.New()
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Value.FileId.Should().Be(file.Id);
        file.FolderId.Should().Be(targetFolder.Id);
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenTargetFolderBelongsToDifferentFamily()
    {
        var familyId = FamilyId.New();
        var otherFamilyId = FamilyId.New();
        var fileRepo = Substitute.For<IStoredFileRepository>();
        var folderRepo = Substitute.For<IFolderRepository>();

        var sourceFolder = Folder.CreateRoot(familyId, UserId.New(), DateTimeOffset.UtcNow);
        var targetFolder = Folder.CreateRoot(otherFamilyId, UserId.New(), DateTimeOffset.UtcNow);

        var file = CreateTestFile(familyId, sourceFolder.Id);
        fileRepo.GetByIdAsync(file.Id, Arg.Any<CancellationToken>()).Returns(file);
        folderRepo.GetByIdAsync(targetFolder.Id, Arg.Any<CancellationToken>()).Returns(targetFolder);

        var handler = new MoveFileCommandHandler(fileRepo, folderRepo, TimeProvider.System);

        var command = new MoveFileCommand(
            file.Id,
            targetFolder.Id)
        {
            FamilyId = familyId,
            UserId = UserId.New()
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be(DomainErrorCodes.Forbidden);
    }
}
