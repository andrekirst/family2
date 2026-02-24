using FamilyHub.Api.Features.FileManagement.Application.Queries.GetFileVersions;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class GetFileVersionsQueryHandlerTests
{
    private static readonly string ValidChecksum = new('a', 64);

    [Fact]
    public async Task Handle_ShouldReturnVersions()
    {
        var fileRepo = new FakeStoredFileRepository();
        var versionRepo = new FakeFileVersionRepository();
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
            UserId.New());
        fileRepo.Files.Add(file);

        var v1 = FileVersion.Create(file.Id, 1, StorageKey.From("v1"), FileSize.From(1000), Checksum.From(ValidChecksum), UserId.New(), isCurrent: false);
        var v2 = FileVersion.Create(file.Id, 2, StorageKey.From("v2"), FileSize.From(2000), Checksum.From(ValidChecksum), UserId.New());
        versionRepo.Versions.Add(v1);
        versionRepo.Versions.Add(v2);

        var query = new GetFileVersionsQuery(file.Id, familyId);
        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(2);
        result.First().VersionNumber.Should().Be(2); // Descending order
        result.Last().VersionNumber.Should().Be(1);
    }

    [Fact]
    public async Task Handle_NoVersions_ShouldReturnEmpty()
    {
        var fileRepo = new FakeStoredFileRepository();
        var versionRepo = new FakeFileVersionRepository();
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
            UserId.New());
        fileRepo.Files.Add(file);

        var query = new GetFileVersionsQuery(file.Id, familyId);
        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_FileNotFound_ShouldThrow()
    {
        var fileRepo = new FakeStoredFileRepository();
        var versionRepo = new FakeFileVersionRepository();
        var handler = new GetFileVersionsQueryHandler(versionRepo, fileRepo);

        var query = new GetFileVersionsQuery(FileId.New(), FamilyId.New());
        var act = () => handler.Handle(query, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*File not found*");
    }

    [Fact]
    public async Task Handle_DifferentFamily_ShouldThrow()
    {
        var fileRepo = new FakeStoredFileRepository();
        var versionRepo = new FakeFileVersionRepository();
        var handler = new GetFileVersionsQueryHandler(versionRepo, fileRepo);

        var file = StoredFile.Create(
            FileName.From("doc.pdf"),
            MimeType.From("application/pdf"),
            FileSize.From(1000),
            StorageKey.From("key"),
            Checksum.From(ValidChecksum),
            FolderId.New(),
            FamilyId.New(),
            UserId.New());
        fileRepo.Files.Add(file);

        var query = new GetFileVersionsQuery(file.Id, FamilyId.New()); // Different family
        var act = () => handler.Handle(query, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*File not found in this family*");
    }
}
