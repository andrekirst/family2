using FamilyHub.Api.Features.FileManagement.Application.Commands.RestoreFileVersion;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;

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
            UserId.New());
    }

    [Fact]
    public async Task Handle_ShouldRestoreVersion()
    {
        var fileRepo = new FakeStoredFileRepository();
        var versionRepo = new FakeFileVersionRepository();
        var handler = new RestoreFileVersionCommandHandler(versionRepo, fileRepo);

        var file = CreateTestFile();
        fileRepo.Files.Add(file);

        // Create two versions
        var v1 = FileVersion.Create(file.Id, 1, StorageKey.From("key-v1"), FileSize.From(1000), Checksum.From(ValidChecksum), UserId.New());
        v1.MarkAsNotCurrent();
        versionRepo.Versions.Add(v1);

        var v2 = FileVersion.Create(file.Id, 2, StorageKey.From("key-v2"), FileSize.From(2000), Checksum.From(AnotherChecksum), UserId.New());
        versionRepo.Versions.Add(v2);

        // Restore v1
        var command = new RestoreFileVersionCommand(v1.Id, file.Id, UserId.New());
        var result = await handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.NewVersionNumber.Should().Be(3);
        versionRepo.Versions.Should().HaveCount(3);

        // v2 should no longer be current
        v2.IsCurrent.Should().BeFalse();
        // New version should be current and have v1's storage key
        var restoredVersion = versionRepo.Versions.First(v => v.VersionNumber == 3);
        restoredVersion.IsCurrent.Should().BeTrue();
        restoredVersion.StorageKey.Should().Be(v1.StorageKey);
        restoredVersion.Checksum.Should().Be(v1.Checksum);
    }

    [Fact]
    public async Task Handle_FileNotFound_ShouldThrow()
    {
        var fileRepo = new FakeStoredFileRepository();
        var versionRepo = new FakeFileVersionRepository();
        var handler = new RestoreFileVersionCommandHandler(versionRepo, fileRepo);

        var command = new RestoreFileVersionCommand(
            FileVersionId.New(), FileId.New(), UserId.New());

        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*File not found*");
    }

    [Fact]
    public async Task Handle_VersionNotFound_ShouldThrow()
    {
        var fileRepo = new FakeStoredFileRepository();
        var versionRepo = new FakeFileVersionRepository();
        var handler = new RestoreFileVersionCommandHandler(versionRepo, fileRepo);

        var file = CreateTestFile();
        fileRepo.Files.Add(file);

        var command = new RestoreFileVersionCommand(
            FileVersionId.New(), file.Id, UserId.New());

        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*Version not found*");
    }

    [Fact]
    public async Task Handle_VersionBelongsToDifferentFile_ShouldThrow()
    {
        var fileRepo = new FakeStoredFileRepository();
        var versionRepo = new FakeFileVersionRepository();
        var handler = new RestoreFileVersionCommandHandler(versionRepo, fileRepo);

        var file1 = CreateTestFile();
        var file2 = CreateTestFile();
        fileRepo.Files.Add(file1);
        fileRepo.Files.Add(file2);

        // Create version for file2
        var version = FileVersion.Create(file2.Id, 1, StorageKey.From("key"), FileSize.From(100), Checksum.From(ValidChecksum), UserId.New());
        versionRepo.Versions.Add(version);

        // Try to restore it as if it belongs to file1
        var command = new RestoreFileVersionCommand(version.Id, file1.Id, UserId.New());
        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*Version does not belong to this file*");
    }
}
