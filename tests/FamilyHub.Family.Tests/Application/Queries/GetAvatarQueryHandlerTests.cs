using FamilyHub.Api.Features.Family.Application.Queries.GetAvatar;
using FamilyHub.Api.Features.Family.Infrastructure.Avatar;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace FamilyHub.Family.Tests.Application.Queries;

public class GetAvatarQueryHandlerTests
{
    private readonly IAvatarRepository _avatarRepository = Substitute.For<IAvatarRepository>();
    private readonly IFileStorageService _fileStorageService = Substitute.For<IFileStorageService>();
    private readonly GetAvatarQueryHandler _handler;

    public GetAvatarQueryHandlerTests()
    {
        _handler = new GetAvatarQueryHandler(_avatarRepository, _fileStorageService);
    }

    [Fact]
    public async Task Handle_ShouldReturnAvatarData_WhenAvatarExists()
    {
        // Arrange
        var fileData = new byte[] { 1, 2, 3, 4, 5 };
        var avatar = CreateTestAvatar(AvatarSize.Small, "storage-key-small", "image/png");
        var query = new GetAvatarQuery(avatar.Id.Value, "small") { UserId = UserId.New() };

        _avatarRepository.GetByIdAsync(AvatarId.From(Guid.NewGuid()), Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs(avatar);
        _fileStorageService.GetAsync("storage-key-small", Arg.Any<CancellationToken>())
            .Returns(fileData);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.MimeType.Should().Be("image/png");
        result.Value.ETag.Should().Be("\"storage-key-small\"");
        result.Value.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ShouldReturnValidationError_WhenSizeIsInvalid()
    {
        // Arrange
        var query = new GetAvatarQuery(Guid.NewGuid(), "invalid") { UserId = UserId.New() };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.ErrorCode.Should().Be("INVALID_AVATAR_SIZE");
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenAvatarNotFound()
    {
        // Arrange
        var query = new GetAvatarQuery(Guid.NewGuid(), "small") { UserId = UserId.New() };

        _avatarRepository.GetByIdAsync(AvatarId.From(Guid.NewGuid()), Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs((AvatarAggregate?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.ErrorCode.Should().Be("AVATAR_NOT_FOUND");
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenVariantNotFound()
    {
        // Arrange
        var avatar = CreateTestAvatar(AvatarSize.Small, "storage-key-small", "image/png");
        var query = new GetAvatarQuery(avatar.Id.Value, "large") { UserId = UserId.New() };

        _avatarRepository.GetByIdAsync(AvatarId.From(Guid.NewGuid()), Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs(avatar);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.ErrorCode.Should().Be("AVATAR_VARIANT_NOT_FOUND");
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenFileDataNotFound()
    {
        // Arrange
        var avatar = CreateTestAvatar(AvatarSize.Small, "storage-key-small", "image/png");
        var query = new GetAvatarQuery(avatar.Id.Value, "small") { UserId = UserId.New() };

        _avatarRepository.GetByIdAsync(AvatarId.From(Guid.NewGuid()), Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs(avatar);
        _fileStorageService.GetAsync("storage-key-small", Arg.Any<CancellationToken>())
            .Returns((byte[]?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.ErrorCode.Should().Be("AVATAR_DATA_NOT_FOUND");
    }

    private static AvatarAggregate CreateTestAvatar(AvatarSize size, string storageKey, string mimeType)
    {
        var variants = new Dictionary<AvatarSize, AvatarVariantData>
        {
            [size] = new(storageKey, mimeType, 1024, 48, 48)
        };

        return AvatarAggregate.Create("test.png", mimeType, variants, DateTimeOffset.UtcNow);
    }
}
