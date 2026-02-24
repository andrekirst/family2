using FamilyHub.Api.Features.FileManagement.Application.Commands.UploadFile;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class UploadFileCommandHandlerTests
{
    private static (UploadFileCommandHandler handler, FakeStoredFileRepository fileRepo, FakeFolderRepository folderRepo) CreateHandler()
    {
        var fileRepo = new FakeStoredFileRepository();
        var folderRepo = new FakeFolderRepository();
        var handler = new UploadFileCommandHandler(fileRepo, folderRepo);
        return (handler, fileRepo, folderRepo);
    }

    private static Folder CreateTestFolder(FamilyId familyId)
    {
        return Folder.CreateRoot(familyId, UserId.New());
    }

    [Fact]
    public async Task Handle_ShouldCreateFileAndReturnResult()
    {
        var familyId = FamilyId.New();
        var (handler, fileRepo, folderRepo) = CreateHandler();
        var folder = CreateTestFolder(familyId);
        folderRepo.Folders.Add(folder);

        var command = new UploadFileCommand(
            FileName.From("test.pdf"),
            MimeType.From("application/pdf"),
            FileSize.From(1024),
            StorageKey.New(),
            Checksum.From("a".PadRight(64, 'a')),
            folder.Id,
            familyId,
            UserId.New());

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.FileId.Value.Should().NotBe(Guid.Empty);
        fileRepo.Files.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenFolderNotFound()
    {
        var (handler, _, _) = CreateHandler();

        var command = new UploadFileCommand(
            FileName.From("test.pdf"),
            MimeType.From("application/pdf"),
            FileSize.From(1024),
            StorageKey.New(),
            Checksum.From("a".PadRight(64, 'a')),
            FolderId.New(),
            FamilyId.New(),
            UserId.New());

        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == DomainErrorCodes.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenFolderBelongsToDifferentFamily()
    {
        var (handler, _, folderRepo) = CreateHandler();
        var folder = CreateTestFolder(FamilyId.New());
        folderRepo.Folders.Add(folder);

        var command = new UploadFileCommand(
            FileName.From("test.pdf"),
            MimeType.From("application/pdf"),
            FileSize.From(1024),
            StorageKey.New(),
            Checksum.From("a".PadRight(64, 'a')),
            folder.Id,
            FamilyId.New(), // different family
            UserId.New());

        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == DomainErrorCodes.Forbidden);
    }
}
