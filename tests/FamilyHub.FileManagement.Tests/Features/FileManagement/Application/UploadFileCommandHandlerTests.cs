using FamilyHub.Api.Features.FileManagement.Application.Commands.UploadFile;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class UploadFileCommandHandlerTests
{
    private readonly IStoredFileRepository _fileRepo = Substitute.For<IStoredFileRepository>();
    private readonly IFolderRepository _folderRepo = Substitute.For<IFolderRepository>();
    private readonly UploadFileCommandHandler _handler;

    public UploadFileCommandHandlerTests()
    {
        _handler = new UploadFileCommandHandler(_fileRepo, _folderRepo, TimeProvider.System);
    }

    private static Folder CreateTestFolder(FamilyId familyId)
    {
        return Folder.CreateRoot(familyId, UserId.New(), DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task Handle_ShouldCreateFileAndReturnResult()
    {
        var familyId = FamilyId.New();
        var folder = CreateTestFolder(familyId);
        _folderRepo.GetByIdAsync(folder.Id, Arg.Any<CancellationToken>()).Returns(folder);

        var command = new UploadFileCommand(
            FileName.From("test.pdf"),
            MimeType.From("application/pdf"),
            FileSize.From(1024),
            StorageKey.New(),
            Checksum.From("a".PadRight(64, 'a')),
            folder.Id)
        {
            FamilyId = familyId,
            UserId = UserId.New()
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.FileId.Value.Should().NotBe(Guid.Empty);
        await _fileRepo.Received(1).AddAsync(Arg.Any<StoredFile>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenFolderNotFound()
    {
        _folderRepo.GetByIdAsync(FolderId.New(), Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs((Folder?)null);

        var command = new UploadFileCommand(
            FileName.From("test.pdf"),
            MimeType.From("application/pdf"),
            FileSize.From(1024),
            StorageKey.New(),
            Checksum.From("a".PadRight(64, 'a')),
            FolderId.New())
        {
            FamilyId = FamilyId.New(),
            UserId = UserId.New()
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be(DomainErrorCodes.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenFolderBelongsToDifferentFamily()
    {
        var folder = CreateTestFolder(FamilyId.New());
        _folderRepo.GetByIdAsync(folder.Id, Arg.Any<CancellationToken>()).Returns(folder);

        var command = new UploadFileCommand(
            FileName.From("test.pdf"),
            MimeType.From("application/pdf"),
            FileSize.From(1024),
            StorageKey.New(),
            Checksum.From("a".PadRight(64, 'a')),
            folder.Id)
        {
            FamilyId = FamilyId.New(),
            UserId =
            UserId.New()
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be(DomainErrorCodes.Forbidden);
    }
}
