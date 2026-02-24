using FamilyHub.Api.Features.FileManagement.Application.Commands.CreateFileVersion;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class CreateFileVersionCommandHandlerTests
{
    private static readonly string ValidChecksum = new('a', 64);
    private static readonly string AnotherChecksum = new('b', 64);

    [Fact]
    public async Task Handle_ShouldCreateFirstVersion()
    {
        var fileRepo = new FakeStoredFileRepository();
        var versionRepo = new FakeFileVersionRepository();
        var handler = new CreateFileVersionCommandHandler(versionRepo, fileRepo);

        var file = StoredFile.Create(
            FileName.From("doc.pdf"),
            MimeType.From("application/pdf"),
            FileSize.From(1000),
            StorageKey.From("key-1"),
            Checksum.From(ValidChecksum),
            FolderId.New(),
            FamilyId.New(),
            UserId.New());
        fileRepo.Files.Add(file);

        var command = new CreateFileVersionCommand(
            file.Id,
            StorageKey.From("version-key-1"),
            FileSize.From(1000),
            Checksum.From(ValidChecksum),
            UserId.New());

        var result = await handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.VersionNumber.Should().Be(1);
        versionRepo.Versions.Should().HaveCount(1);
        versionRepo.Versions.First().IsCurrent.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldMarkPreviousVersionAsNotCurrent()
    {
        var fileRepo = new FakeStoredFileRepository();
        var versionRepo = new FakeFileVersionRepository();
        var handler = new CreateFileVersionCommandHandler(versionRepo, fileRepo);

        var file = StoredFile.Create(
            FileName.From("doc.pdf"),
            MimeType.From("application/pdf"),
            FileSize.From(1000),
            StorageKey.From("key-1"),
            Checksum.From(ValidChecksum),
            FolderId.New(),
            FamilyId.New(),
            UserId.New());
        fileRepo.Files.Add(file);

        // Create first version
        await handler.Handle(new CreateFileVersionCommand(
            file.Id,
            StorageKey.From("version-key-1"),
            FileSize.From(1000),
            Checksum.From(ValidChecksum),
            UserId.New()), CancellationToken.None);

        // Create second version
        var result = await handler.Handle(new CreateFileVersionCommand(
            file.Id,
            StorageKey.From("version-key-2"),
            FileSize.From(2000),
            Checksum.From(AnotherChecksum),
            UserId.New()), CancellationToken.None);

        result.VersionNumber.Should().Be(2);
        versionRepo.Versions.Should().HaveCount(2);
        versionRepo.Versions.First(v => v.VersionNumber == 1).IsCurrent.Should().BeFalse();
        versionRepo.Versions.First(v => v.VersionNumber == 2).IsCurrent.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_FileNotFound_ShouldThrow()
    {
        var fileRepo = new FakeStoredFileRepository();
        var versionRepo = new FakeFileVersionRepository();
        var handler = new CreateFileVersionCommandHandler(versionRepo, fileRepo);

        var command = new CreateFileVersionCommand(
            FileId.New(),
            StorageKey.From("key"),
            FileSize.From(100),
            Checksum.From(ValidChecksum),
            UserId.New());

        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*File not found*");
    }
}
