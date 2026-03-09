using FamilyHub.Api.Features.FileManagement.Application.Commands.TagFile;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;
using Tag = FamilyHub.Api.Features.FileManagement.Domain.Entities.Tag;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class TagFileCommandHandlerTests
{
    private readonly IStoredFileRepository _fileRepo = Substitute.For<IStoredFileRepository>();
    private readonly ITagRepository _tagRepo = Substitute.For<ITagRepository>();
    private readonly IFileTagRepository _fileTagRepo = Substitute.For<IFileTagRepository>();
    private readonly TagFileCommandHandler _handler;

    public TagFileCommandHandlerTests()
    {
        _handler = new TagFileCommandHandler(_fileRepo, _tagRepo, _fileTagRepo, TimeProvider.System);
    }

    private static StoredFile CreateTestFile(FamilyId familyId)
    {
        return StoredFile.Create(
            FileName.From("photo.jpg"),
            MimeType.From("image/jpeg"),
            FileSize.From(1024),
            StorageKey.New(),
            Checksum.From("a".PadRight(64, 'a')),
            FolderId.New(),
            familyId,
            UserId.New(), DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task Handle_ShouldTagFile()
    {
        var familyId = FamilyId.New();
        var file = CreateTestFile(familyId);
        var tag = Tag.Create(TagName.From("Photos"), TagColor.From("#FF0000"), familyId, UserId.New(), DateTimeOffset.UtcNow);

        _fileRepo.GetByIdAsync(file.Id, Arg.Any<CancellationToken>()).Returns(file);
        _tagRepo.GetByIdAsync(tag.Id, Arg.Any<CancellationToken>()).Returns(tag);
        _fileTagRepo.ExistsAsync(file.Id, tag.Id, Arg.Any<CancellationToken>()).Returns(false);

        var command = new TagFileCommand(file.Id, tag.Id)
        {
            FamilyId = familyId,
            UserId = UserId.New()
        };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _fileTagRepo.Received(1).AddAsync(
            Arg.Is<FileTag>(ft => ft.FileId == file.Id && ft.TagId == tag.Id),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldBeIdempotent()
    {
        var familyId = FamilyId.New();
        var file = CreateTestFile(familyId);
        var tag = Tag.Create(TagName.From("Photos"), TagColor.From("#FF0000"), familyId, UserId.New(), DateTimeOffset.UtcNow);

        _fileRepo.GetByIdAsync(file.Id, Arg.Any<CancellationToken>()).Returns(file);
        _tagRepo.GetByIdAsync(tag.Id, Arg.Any<CancellationToken>()).Returns(tag);
        _fileTagRepo.ExistsAsync(file.Id, tag.Id, Arg.Any<CancellationToken>()).Returns(true);

        var command = new TagFileCommand(file.Id, tag.Id)
        {
            FamilyId = familyId,
            UserId = UserId.New()
        };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _fileTagRepo.DidNotReceive().AddAsync(Arg.Any<FileTag>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenFileNotFound()
    {
        var familyId = FamilyId.New();
        var tag = Tag.Create(TagName.From("Photos"), TagColor.From("#FF0000"), familyId, UserId.New(), DateTimeOffset.UtcNow);

        _fileRepo.GetByIdAsync(FileId.New(), Arg.Any<CancellationToken>()).ReturnsForAnyArgs((StoredFile?)null);
        _tagRepo.GetByIdAsync(tag.Id, Arg.Any<CancellationToken>()).Returns(tag);

        var command = new TagFileCommand(FileId.New(), tag.Id)
        {
            FamilyId = familyId,
            UserId = UserId.New()
        };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be(DomainErrorCodes.FileNotFound);
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenTagNotFound()
    {
        var familyId = FamilyId.New();
        var file = CreateTestFile(familyId);

        _fileRepo.GetByIdAsync(file.Id, Arg.Any<CancellationToken>()).Returns(file);
        _tagRepo.GetByIdAsync(TagId.New(), Arg.Any<CancellationToken>()).ReturnsForAnyArgs((Tag?)null);

        var command = new TagFileCommand(file.Id, TagId.New())
        {
            FamilyId = familyId,
            UserId = UserId.New()
        };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be(DomainErrorCodes.TagNotFound);
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenFileBelongsToDifferentFamily()
    {
        var file = CreateTestFile(FamilyId.New());
        var differentFamily = FamilyId.New();
        var tag = Tag.Create(TagName.From("Photos"), TagColor.From("#FF0000"), differentFamily, UserId.New(), DateTimeOffset.UtcNow);

        _fileRepo.GetByIdAsync(file.Id, Arg.Any<CancellationToken>()).Returns(file);
        _tagRepo.GetByIdAsync(tag.Id, Arg.Any<CancellationToken>()).Returns(tag);

        var command = new TagFileCommand(file.Id, tag.Id)
        {
            FamilyId = differentFamily,
            UserId = UserId.New()
        };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be(DomainErrorCodes.Forbidden);
    }
}
