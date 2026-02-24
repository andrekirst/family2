using FamilyHub.Api.Features.FileManagement.Application.Commands.RenameFile;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class RenameFileCommandHandlerTests
{
    private static StoredFile CreateTestFile(FamilyId familyId)
    {
        return StoredFile.Create(
            FileName.From("original.pdf"),
            MimeType.From("application/pdf"),
            FileSize.From(1024),
            StorageKey.New(),
            Checksum.From("a".PadRight(64, 'a')),
            FolderId.New(),
            familyId,
            UserId.New());
    }

    [Fact]
    public async Task Handle_ShouldRenameFile()
    {
        var familyId = FamilyId.New();
        var fileRepo = new FakeStoredFileRepository();
        var file = CreateTestFile(familyId);
        fileRepo.Files.Add(file);
        var handler = new RenameFileCommandHandler(fileRepo);

        var command = new RenameFileCommand(
            file.Id,
            FileName.From("renamed.pdf"),
            familyId,
            UserId.New());

        var result = await handler.Handle(command, CancellationToken.None);

        result.FileId.Should().Be(file.Id);
        file.Name.Value.Should().Be("renamed.pdf");
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenFileNotFound()
    {
        var fileRepo = new FakeStoredFileRepository();
        var handler = new RenameFileCommandHandler(fileRepo);

        var command = new RenameFileCommand(
            FileId.New(),
            FileName.From("renamed.pdf"),
            FamilyId.New(),
            UserId.New());

        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == DomainErrorCodes.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenFileBelongsToDifferentFamily()
    {
        var fileRepo = new FakeStoredFileRepository();
        var file = CreateTestFile(FamilyId.New());
        fileRepo.Files.Add(file);
        var handler = new RenameFileCommandHandler(fileRepo);

        var command = new RenameFileCommand(
            file.Id,
            FileName.From("renamed.pdf"),
            FamilyId.New(), // different family
            UserId.New());

        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == DomainErrorCodes.Forbidden);
    }
}
