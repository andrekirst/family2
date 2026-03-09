using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Auth.Domain.ValueObjects;
using FamilyHub.Api.Features.Auth.Domain.Entities;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
using FamilyHub.Api.Features.Family.Application.Commands.UploadAvatar;
using FamilyHub.Api.Common.Infrastructure.Avatar;
using FluentAssertions;
using NSubstitute;

namespace FamilyHub.Family.Tests.Features.Family.Application;

public class UploadAvatarCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateAvatarAndReturnResult()
    {
        // Arrange
        var user = CreateTestUser();
        var (handler, _, _, _, _) = CreateHandler(user);
        var command = CreateCommand(user.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AvatarId.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task Handle_ShouldStoreAllFourVariants()
    {
        // Arrange
        var user = CreateTestUser();
        var (handler, _, _, fileStorage, _) = CreateHandler(user);
        var command = CreateCommand(user.Id);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert — 4 size variants stored
        await fileStorage.Received(4).SaveAsync(Arg.Any<byte[]>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldAddAvatarToRepository()
    {
        // Arrange
        var user = CreateTestUser();
        var (handler, avatarRepo, _, _, _) = CreateHandler(user);
        var command = CreateCommand(user.Id);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        await avatarRepo.Received(1).AddAsync(
            Arg.Is<AvatarAggregate>(a =>
                a.Variants.Count == 4 &&
                a.OriginalFileName == "test.jpg" &&
                a.OriginalMimeType == "image/jpeg"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldSetUserAvatar()
    {
        // Arrange
        var user = CreateTestUser();
        var (handler, _, _, _, _) = CreateHandler(user);
        var command = CreateCommand(user.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        user.AvatarId.Should().NotBeNull();
        user.AvatarId.Should().Be(result.AvatarId);
    }

    [Fact]
    public async Task Handle_ShouldDeletePreviousAvatar_WhenUserAlreadyHasOne()
    {
        // Arrange
        var user = CreateTestUser();
        var previousAvatar = CreateTestAvatar();
        user.SetAvatar(previousAvatar.Id);
        user.ClearDomainEvents();

        var (handler, avatarRepo, _, fileStorage, _) = CreateHandler(user, existingAvatar: previousAvatar);
        var command = CreateCommand(user.Id);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert — previous avatar was deleted
        await avatarRepo.Received(1).DeleteAsync(previousAvatar.Id, Arg.Any<CancellationToken>());
        // Previous variant storage keys should have been deleted
        await fileStorage.Received(previousAvatar.Variants.Count).DeleteAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowDomainException_WhenUserNotFound()
    {
        // Arrange
        var userId = UserId.New();
        var (handler, _, _, _, _) = CreateHandler(user: null, userId: userId);
        var command = CreateCommand(userId);

        // Act & Assert
        var act = () => handler.Handle(command, CancellationToken.None).AsTask();
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("User not found");
    }

    // --- Helpers ---

    private static User CreateTestUser()
    {
        var email = Email.From("test@example.com");
        var name = UserName.From("Test User");
        var externalId = ExternalUserId.From("test-external-id");

        var user = User.Register(email, name, externalId, emailVerified: true);
        user.ClearDomainEvents();

        return user;
    }

    private static AvatarAggregate CreateTestAvatar()
    {
        var variants = new Dictionary<AvatarSize, AvatarVariantData>
        {
            [AvatarSize.Tiny] = new("key-tiny", "image/jpeg", 100, 24, 24),
            [AvatarSize.Small] = new("key-small", "image/jpeg", 200, 48, 48),
            [AvatarSize.Medium] = new("key-medium", "image/jpeg", 500, 128, 128),
            [AvatarSize.Large] = new("key-large", "image/jpeg", 1000, 512, 512),
        };

        return AvatarAggregate.Create("previous.jpg", "image/jpeg", variants);
    }

    private static UploadAvatarCommand CreateCommand(UserId userId) =>
        new(new byte[] { 0xFF, 0xD8, 0xFF }, "test.jpg", "image/jpeg",
            null, null, null, null) { UserId = userId, FamilyId = FamilyId.New() };

    private static (
        UploadAvatarCommandHandler Handler,
        IAvatarRepository AvatarRepo,
        IUserRepository UserRepo,
        IFileStorageService FileStorage,
        IAvatarProcessingService Processing
    ) CreateHandler(User? user, AvatarAggregate? existingAvatar = null, UserId? userId = null)
    {
        var userRepo = Substitute.For<IUserRepository>();
        var lookupId = user?.Id ?? userId ?? UserId.New();
        userRepo.GetByIdAsync(lookupId, CancellationToken.None).Returns(user);

        var avatarRepo = Substitute.For<IAvatarRepository>();
        if (existingAvatar is not null)
        {
            avatarRepo.GetByIdAsync(existingAvatar.Id, CancellationToken.None).Returns(existingAvatar);
        }

        var fileStorage = Substitute.For<IFileStorageService>();
        fileStorage.SaveAsync(Arg.Any<byte[]>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => $"fake-storage-key-{Guid.NewGuid():N}");

        var processing = Substitute.For<IAvatarProcessingService>();
        processing.ProcessAvatarAsync(Arg.Any<Stream>(), Arg.Any<CropArea?>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<AvatarSize, byte[]>
            {
                [AvatarSize.Tiny] = new byte[] { 1, 2, 3 },
                [AvatarSize.Small] = new byte[] { 4, 5, 6 },
                [AvatarSize.Medium] = new byte[] { 7, 8, 9 },
                [AvatarSize.Large] = new byte[] { 10, 11, 12 }
            });

        var handler = new UploadAvatarCommandHandler(userRepo, avatarRepo, processing, fileStorage);
        return (handler, avatarRepo, userRepo, fileStorage, processing);
    }
}
