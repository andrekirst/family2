using FamilyHub.Api.Features.FileManagement.Application.Commands.RenameFile;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class RenameFileCommandHandlerTests
{
    private readonly IStoredFileRepository _fileRepo = Substitute.For<IStoredFileRepository>();
    private readonly RenameFileCommandHandler _handler;

    public RenameFileCommandHandlerTests()
    {
        _handler = new RenameFileCommandHandler(_fileRepo, TimeProvider.System);
    }

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
            UserId.New(), DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task Handle_ShouldRenameFile()
    {
        var familyId = FamilyId.New();
        var file = CreateTestFile(familyId);
        _fileRepo.GetByIdAsync(file.Id, Arg.Any<CancellationToken>()).Returns(file);

        var command = new RenameFileCommand(
            file.Id,
            FileName.From("renamed.pdf"))
        {
            FamilyId = familyId,
            UserId = UserId.New()
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Value.FileId.Should().Be(file.Id);
        file.Name.Value.Should().Be("renamed.pdf");
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenFileNotFound()
    {
        _fileRepo.GetByIdAsync(FileId.New(), Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs((StoredFile?)null);

        var command = new RenameFileCommand(
            FileId.New(),
            FileName.From("renamed.pdf"))
        {
            FamilyId = FamilyId.New(),
            UserId = UserId.New()
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be(DomainErrorCodes.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenFileBelongsToDifferentFamily()
    {
        var file = CreateTestFile(FamilyId.New());
        _fileRepo.GetByIdAsync(file.Id, Arg.Any<CancellationToken>()).Returns(file);

        var command = new RenameFileCommand(
            file.Id,
            FileName.From("renamed.pdf"))
        {
            FamilyId = FamilyId.New(),
            UserId = UserId.New()
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be(DomainErrorCodes.Forbidden);
    }
}
