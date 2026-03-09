using FamilyHub.Api.Features.FileManagement.Application.Queries.GetFileVersions;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class GetFileVersionsQueryHandlerTests
{
    private static readonly string ValidChecksum = new('a', 64);

    [Fact]
    public async Task Handle_ShouldReturnVersions()
    {
        var fileRepo = Substitute.For<IStoredFileRepository>();
        var versionRepo = Substitute.For<IFileVersionRepository>();
        var handler = new GetFileVersionsQueryHandler(versionRepo, fileRepo);

        var familyId = FamilyId.New();
        var file = StoredFile.Create(
            FileName.From("doc.pdf"),
            MimeType.From("application/pdf"),
            FileSize.From(1000),
            StorageKey.From("key"),
            Checksum.From(ValidChecksum),
            FolderId.New(),
            familyId,
            UserId.New(), DateTimeOffset.UtcNow);
        fileRepo.GetByIdAsync(file.Id, Arg.Any<CancellationToken>()).Returns(file);

        var v1 = FileVersion.Create(file.Id, 1, StorageKey.From("v1"), FileSize.From(1000), Checksum.From(ValidChecksum), UserId.New(), DateTimeOffset.UtcNow, isCurrent: false);
        var v2 = FileVersion.Create(file.Id, 2, StorageKey.From("v2"), FileSize.From(2000), Checksum.From(ValidChecksum), UserId.New(), DateTimeOffset.UtcNow);
        versionRepo.GetByFileIdAsync(file.Id, Arg.Any<CancellationToken>())
            .Returns(new List<FileVersion> { v1, v2 });

        var query = new GetFileVersionsQuery(file.Id)
        {
            FamilyId = familyId,
            UserId = UserId.New()
        };
        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(2);
        result.Should().Contain(v => v.VersionNumber == 1);
        result.Should().Contain(v => v.VersionNumber == 2);
    }

    [Fact]
    public async Task Handle_NoVersions_ShouldReturnEmpty()
    {
        var fileRepo = Substitute.For<IStoredFileRepository>();
        var versionRepo = Substitute.For<IFileVersionRepository>();
        var handler = new GetFileVersionsQueryHandler(versionRepo, fileRepo);

        var familyId = FamilyId.New();
        var file = StoredFile.Create(
            FileName.From("doc.pdf"),
            MimeType.From("application/pdf"),
            FileSize.From(1000),
            StorageKey.From("key"),
            Checksum.From(ValidChecksum),
            FolderId.New(),
            familyId,
            UserId.New(), DateTimeOffset.UtcNow);
        fileRepo.GetByIdAsync(file.Id, Arg.Any<CancellationToken>()).Returns(file);
        versionRepo.GetByFileIdAsync(file.Id, Arg.Any<CancellationToken>())
            .Returns(new List<FileVersion>());

        var query = new GetFileVersionsQuery(file.Id)
        {
            FamilyId = familyId,
            UserId = UserId.New()
        };
        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_FileNotFound_ShouldThrow()
    {
        var fileRepo = Substitute.For<IStoredFileRepository>();
        var versionRepo = Substitute.For<IFileVersionRepository>();
        var handler = new GetFileVersionsQueryHandler(versionRepo, fileRepo);

        fileRepo.GetByIdAsync(FileId.New(), Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs((StoredFile?)null);

        var query = new GetFileVersionsQuery(FileId.New())
        {
            FamilyId = FamilyId.New(),
            UserId = UserId.New()
        };
        var act = () => handler.Handle(query, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*File not found*");
    }

    [Fact]
    public async Task Handle_DifferentFamily_ShouldThrow()
    {
        var fileRepo = Substitute.For<IStoredFileRepository>();
        var versionRepo = Substitute.For<IFileVersionRepository>();
        var handler = new GetFileVersionsQueryHandler(versionRepo, fileRepo);

        var file = StoredFile.Create(
            FileName.From("doc.pdf"),
            MimeType.From("application/pdf"),
            FileSize.From(1000),
            StorageKey.From("key"),
            Checksum.From(ValidChecksum),
            FolderId.New(),
            FamilyId.New(),
            UserId.New(), DateTimeOffset.UtcNow);
        fileRepo.GetByIdAsync(file.Id, Arg.Any<CancellationToken>()).Returns(file);

        var query = new GetFileVersionsQuery(file.Id)
        {
            FamilyId = FamilyId.New(),
            UserId = UserId.New()
        }; // Different family
        var act = () => handler.Handle(query, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*File not found in this family*");
    }
}
