using FamilyHub.Api.Features.FileManagement.Application.Commands.RestoreFileVersion;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class RestoreFileVersionCommandHandlerTests
{
    private static readonly string ValidChecksum = new('a', 64);
    private static readonly string AnotherChecksum = new('b', 64);

    private static StoredFile CreateTestFile()
    {
        return StoredFile.Create(
            FileName.From("doc.pdf"),
            MimeType.From("application/pdf"),
            FileSize.From(1000),
            StorageKey.From("original-key"),
            Checksum.From(ValidChecksum),
            FolderId.New(),
            FamilyId.New(),
            UserId.New(), DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task Handle_ShouldRestoreVersion()
    {
        var fileRepo = Substitute.For<IStoredFileRepository>();
        var versionRepo = Substitute.For<IFileVersionRepository>();
        var handler = new RestoreFileVersionCommandHandler(versionRepo, fileRepo, TimeProvider.System);

        var file = CreateTestFile();
        fileRepo.GetByIdAsync(file.Id, Arg.Any<CancellationToken>()).Returns(file);

        var v1 = FileVersion.Create(file.Id, 1, StorageKey.From("key-v1"), FileSize.From(1000), Checksum.From(ValidChecksum), UserId.New(), DateTimeOffset.UtcNow);
        v1.MarkAsNotCurrent();
        var v2 = FileVersion.Create(file.Id, 2, StorageKey.From("key-v2"), FileSize.From(2000), Checksum.From(AnotherChecksum), UserId.New(), DateTimeOffset.UtcNow);

        versionRepo.GetByIdAsync(v1.Id, Arg.Any<CancellationToken>()).Returns(v1);
        versionRepo.GetCurrentVersionAsync(file.Id, Arg.Any<CancellationToken>()).Returns(v2);
        versionRepo.GetMaxVersionNumberAsync(file.Id, Arg.Any<CancellationToken>()).Returns(2);

        var command = new RestoreFileVersionCommand(v1.Id, file.Id)
        {
            UserId = UserId.New(),
            FamilyId = FamilyId.New()
        };
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.NewVersionNumber.Should().Be(3);
        v2.IsCurrent.Should().BeFalse();
        await versionRepo.Received(1).AddAsync(
            Arg.Is<FileVersion>(v => v.VersionNumber == 3 && v.IsCurrent && v.StorageKey == v1.StorageKey),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_FileNotFound_ShouldThrow()
    {
        var fileRepo = Substitute.For<IStoredFileRepository>();
        var versionRepo = Substitute.For<IFileVersionRepository>();
        var handler = new RestoreFileVersionCommandHandler(versionRepo, fileRepo, TimeProvider.System);

        fileRepo.GetByIdAsync(FileId.New(), Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs((StoredFile?)null);

        var command = new RestoreFileVersionCommand(
            FileVersionId.New(),
            FileId.New())
        {
            UserId = UserId.New(),
            FamilyId = FamilyId.New()
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Match("*File not found*");
    }

    [Fact]
    public async Task Handle_VersionNotFound_ShouldThrow()
    {
        var fileRepo = Substitute.For<IStoredFileRepository>();
        var versionRepo = Substitute.For<IFileVersionRepository>();
        var handler = new RestoreFileVersionCommandHandler(versionRepo, fileRepo, TimeProvider.System);

        var file = CreateTestFile();
        fileRepo.GetByIdAsync(file.Id, Arg.Any<CancellationToken>()).Returns(file);
        versionRepo.GetByFileIdAsync(file.Id, Arg.Any<CancellationToken>())
            .Returns(new List<FileVersion>());

        var command = new RestoreFileVersionCommand(
            FileVersionId.New(),
            file.Id)
        {
            UserId = UserId.New(),
            FamilyId = FamilyId.New()
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Match("*Version not found*");
    }

    [Fact]
    public async Task Handle_VersionBelongsToDifferentFile_ShouldThrow()
    {
        var fileRepo = Substitute.For<IStoredFileRepository>();
        var versionRepo = Substitute.For<IFileVersionRepository>();
        var handler = new RestoreFileVersionCommandHandler(versionRepo, fileRepo, TimeProvider.System);

        var file1 = CreateTestFile();
        var file2 = CreateTestFile();
        fileRepo.GetByIdAsync(file1.Id, Arg.Any<CancellationToken>()).Returns(file1);

        var version = FileVersion.Create(file2.Id, 1, StorageKey.From("key"), FileSize.From(100), Checksum.From(ValidChecksum), UserId.New(), DateTimeOffset.UtcNow);
        versionRepo.GetByFileIdAsync(file1.Id, Arg.Any<CancellationToken>())
            .Returns(new List<FileVersion>()); // version belongs to file2, not file1

        var command = new RestoreFileVersionCommand(version.Id, file1.Id)
        {
            UserId = UserId.New(),
            FamilyId = FamilyId.New()
        };
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Match("*Version*not*found*");
    }
}
