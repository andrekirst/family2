using FamilyHub.Api.Features.FileManagement.Application.Commands.CreateFileVersion;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class CreateFileVersionCommandHandlerTests
{
    private static readonly string ValidChecksum = new('a', 64);
    private static readonly string AnotherChecksum = new('b', 64);

    [Fact]
    public async Task Handle_ShouldCreateFirstVersion()
    {
        var fileRepo = Substitute.For<IStoredFileRepository>();
        var versionRepo = Substitute.For<IFileVersionRepository>();
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
        fileRepo.GetByIdAsync(file.Id, Arg.Any<CancellationToken>()).Returns(file);
        versionRepo.GetMaxVersionNumberAsync(file.Id, Arg.Any<CancellationToken>()).Returns(0);
        versionRepo.GetCurrentVersionAsync(file.Id, Arg.Any<CancellationToken>())
            .Returns((FileVersion?)null);

        var command = new CreateFileVersionCommand(
            file.Id,
            StorageKey.From("version-key-1"),
            FileSize.From(1000),
            Checksum.From(ValidChecksum))
        {
            UserId = UserId.New(),
            FamilyId = FamilyId.New()
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.VersionNumber.Should().Be(1);
        await versionRepo.Received(1).AddAsync(
            Arg.Is<FileVersion>(v => v.IsCurrent && v.VersionNumber == 1),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldMarkPreviousVersionAsNotCurrent()
    {
        var fileRepo = Substitute.For<IStoredFileRepository>();
        var versionRepo = Substitute.For<IFileVersionRepository>();
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
        fileRepo.GetByIdAsync(file.Id, Arg.Any<CancellationToken>()).Returns(file);

        var existingVersion = FileVersion.Create(file.Id, 1, StorageKey.From("version-key-1"),
            FileSize.From(1000), Checksum.From(ValidChecksum), UserId.New());
        versionRepo.GetMaxVersionNumberAsync(file.Id, Arg.Any<CancellationToken>()).Returns(1);
        versionRepo.GetCurrentVersionAsync(file.Id, Arg.Any<CancellationToken>())
            .Returns(existingVersion);

        var command = new CreateFileVersionCommand(
            file.Id,
            StorageKey.From("version-key-2"),
            FileSize.From(2000),
            Checksum.From(AnotherChecksum))
        {
            UserId = UserId.New(),
            FamilyId = FamilyId.New()
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.VersionNumber.Should().Be(2);
        existingVersion.IsCurrent.Should().BeFalse();
        await versionRepo.Received(1).AddAsync(
            Arg.Is<FileVersion>(v => v.IsCurrent && v.VersionNumber == 2),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_FileNotFound_ShouldThrow()
    {
        var fileRepo = Substitute.For<IStoredFileRepository>();
        var versionRepo = Substitute.For<IFileVersionRepository>();
        var handler = new CreateFileVersionCommandHandler(versionRepo, fileRepo);

        fileRepo.GetByIdAsync(FileId.New(), Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs((StoredFile?)null);

        var command = new CreateFileVersionCommand(
            FileId.New(),
            StorageKey.From("key"),
            FileSize.From(100),
            Checksum.From(ValidChecksum))
        {
            UserId = UserId.New(),
            FamilyId = FamilyId.New()
        };

        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*File not found*");
    }
}
