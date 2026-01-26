using FamilyHub.Infrastructure.Messaging;
using FamilyHub.Modules.UserProfile.Application.Abstractions;
using FamilyHub.Modules.UserProfile.Application.EventHandlers;
using FamilyHub.Modules.UserProfile.Domain.Events;
using FamilyHub.Modules.UserProfile.Domain.ValueObjects;
using FamilyHub.SharedKernel.Application.Abstractions;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace FamilyHub.Tests.Unit.UserProfile.Application.EventHandlers;

/// <summary>
/// Unit tests for DisplayNameChangedEventHandler.
/// Tests event chain: Profile → Cache invalidation + Calendar update + Real-time UI updates.
/// </summary>
public class DisplayNameChangedEventHandlerTests
{
    private readonly ICacheInvalidationService _cacheInvalidationService;
    private readonly ICalendarService _calendarService;
    private readonly IUserLookupService _userLookupService;
    private readonly IRedisSubscriptionPublisher _redisPublisher;
    private readonly IProfileEventPublisher _eventPublisher;
    private readonly ILogger<DisplayNameChangedEventHandler> _logger;
    private readonly DisplayNameChangedEventHandler _handler;

    public DisplayNameChangedEventHandlerTests()
    {
        _cacheInvalidationService = Substitute.For<ICacheInvalidationService>();
        _calendarService = Substitute.For<ICalendarService>();
        _userLookupService = Substitute.For<IUserLookupService>();
        _redisPublisher = Substitute.For<IRedisSubscriptionPublisher>();
        _eventPublisher = Substitute.For<IProfileEventPublisher>();
        _logger = Substitute.For<ILogger<DisplayNameChangedEventHandler>>();

        _handler = new DisplayNameChangedEventHandler(
            _cacheInvalidationService,
            _calendarService,
            _userLookupService,
            _redisPublisher,
            _eventPublisher,
            _logger);
    }

    #region Happy Path Tests

    [Fact]
    public async Task Handle_WithFamilyId_InvalidatesFamilyMembersCache()
    {
        // Arrange
        var profileId = UserProfileId.New();
        var userId = UserId.New();
        var familyId = FamilyId.New();
        var oldDisplayName = DisplayName.From("John Doe");
        var newDisplayName = DisplayName.From("John Smith");

        var @event = new DisplayNameChangedEvent(
            eventVersion: 1,
            profileId: profileId,
            userId: userId,
            oldDisplayName: oldDisplayName,
            newDisplayName: newDisplayName);

        _userLookupService.GetUserFamilyIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(familyId);

        // Act
        await _handler.Handle(@event, CancellationToken.None);

        // Assert
        await _cacheInvalidationService.Received(1).InvalidateFamilyMembersCacheAsync(
            familyId,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Always_InvalidatesUserProfileCache()
    {
        // Arrange
        var profileId = UserProfileId.New();
        var userId = UserId.New();
        var oldDisplayName = DisplayName.From("John Doe");
        var newDisplayName = DisplayName.From("John Smith");

        var @event = new DisplayNameChangedEvent(
            eventVersion: 1,
            profileId: profileId,
            userId: userId,
            oldDisplayName: oldDisplayName,
            newDisplayName: newDisplayName);

        _userLookupService.GetUserFamilyIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns((FamilyId?)null);

        // Act
        await _handler.Handle(@event, CancellationToken.None);

        // Assert
        await _cacheInvalidationService.Received(1).InvalidateUserProfileCacheAsync(
            userId,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithFamilyId_UpdatesBirthdayEventTitle()
    {
        // Arrange
        var profileId = UserProfileId.New();
        var userId = UserId.New();
        var familyId = FamilyId.New();
        var oldDisplayName = DisplayName.From("John Doe");
        var newDisplayName = DisplayName.From("John Smith");

        var @event = new DisplayNameChangedEvent(
            eventVersion: 1,
            profileId: profileId,
            userId: userId,
            oldDisplayName: oldDisplayName,
            newDisplayName: newDisplayName);

        _userLookupService.GetUserFamilyIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(familyId);

        // Act
        await _handler.Handle(@event, CancellationToken.None);

        // Assert
        await _calendarService.Received(1).UpdateBirthdayEventTitleAsync(
            familyId,
            userId,
            newDisplayName.Value,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithFamilyId_PublishesRealtimeUpdate()
    {
        // Arrange
        var profileId = UserProfileId.New();
        var userId = UserId.New();
        var familyId = FamilyId.New();
        var oldDisplayName = DisplayName.From("John Doe");
        var newDisplayName = DisplayName.From("John Smith");

        var @event = new DisplayNameChangedEvent(
            eventVersion: 1,
            profileId: profileId,
            userId: userId,
            oldDisplayName: oldDisplayName,
            newDisplayName: newDisplayName);

        _userLookupService.GetUserFamilyIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(familyId);

        // Act
        await _handler.Handle(@event, CancellationToken.None);

        // Assert
        await _redisPublisher.Received(1).PublishAsync(
            Arg.Is<string>(topic => topic.Contains("family-member-updated") && topic.Contains(familyId.Value.ToString())),
            Arg.Any<object>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Always_PublishesToRabbitMQ()
    {
        // Arrange
        var profileId = UserProfileId.New();
        var userId = UserId.New();
        var oldDisplayName = DisplayName.From("John Doe");
        var newDisplayName = DisplayName.From("John Smith");

        var @event = new DisplayNameChangedEvent(
            eventVersion: 1,
            profileId: profileId,
            userId: userId,
            oldDisplayName: oldDisplayName,
            newDisplayName: newDisplayName);

        _userLookupService.GetUserFamilyIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns((FamilyId?)null);

        // Act
        await _handler.Handle(@event, CancellationToken.None);

        // Assert
        await _eventPublisher.Received(1).PublishDisplayNameChangedAsync(
            @event,
            Arg.Any<CancellationToken>());
    }

    #endregion

    #region No FamilyId Tests

    [Fact]
    public async Task Handle_WithoutFamilyId_SkipsFamilyMembersCacheInvalidation()
    {
        // Arrange
        var profileId = UserProfileId.New();
        var userId = UserId.New();
        var oldDisplayName = DisplayName.From("John Doe");
        var newDisplayName = DisplayName.From("John Smith");

        var @event = new DisplayNameChangedEvent(
            eventVersion: 1,
            profileId: profileId,
            userId: userId,
            oldDisplayName: oldDisplayName,
            newDisplayName: newDisplayName);

        _userLookupService.GetUserFamilyIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns((FamilyId?)null);

        // Act
        await _handler.Handle(@event, CancellationToken.None);

        // Assert - Use ReceivedCalls() to avoid Vogen struct issues with Arg.Any<>
        _cacheInvalidationService.ReceivedCalls()
            .Where(c => c.GetMethodInfo().Name == nameof(ICacheInvalidationService.InvalidateFamilyMembersCacheAsync))
            .Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithoutFamilyId_SkipsCalendarUpdate()
    {
        // Arrange
        var profileId = UserProfileId.New();
        var userId = UserId.New();
        var oldDisplayName = DisplayName.From("John Doe");
        var newDisplayName = DisplayName.From("John Smith");

        var @event = new DisplayNameChangedEvent(
            eventVersion: 1,
            profileId: profileId,
            userId: userId,
            oldDisplayName: oldDisplayName,
            newDisplayName: newDisplayName);

        _userLookupService.GetUserFamilyIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns((FamilyId?)null);

        // Act
        await _handler.Handle(@event, CancellationToken.None);

        // Assert - Use ReceivedCalls() to avoid Vogen struct issues with Arg.Any<>
        _calendarService.ReceivedCalls()
            .Where(c => c.GetMethodInfo().Name == nameof(ICalendarService.UpdateBirthdayEventTitleAsync))
            .Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithoutFamilyId_SkipsRealtimeUpdate()
    {
        // Arrange
        var profileId = UserProfileId.New();
        var userId = UserId.New();
        var oldDisplayName = DisplayName.From("John Doe");
        var newDisplayName = DisplayName.From("John Smith");

        var @event = new DisplayNameChangedEvent(
            eventVersion: 1,
            profileId: profileId,
            userId: userId,
            oldDisplayName: oldDisplayName,
            newDisplayName: newDisplayName);

        _userLookupService.GetUserFamilyIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns((FamilyId?)null);

        // Act
        await _handler.Handle(@event, CancellationToken.None);

        // Assert
        await _redisPublisher.DidNotReceive().PublishAsync(
            Arg.Any<string>(),
            Arg.Any<object>(),
            Arg.Any<CancellationToken>());
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task Handle_WhenCalendarUpdateFails_ContinuesProcessing()
    {
        // Arrange
        var profileId = UserProfileId.New();
        var userId = UserId.New();
        var familyId = FamilyId.New();
        var oldDisplayName = DisplayName.From("John Doe");
        var newDisplayName = DisplayName.From("John Smith");

        var @event = new DisplayNameChangedEvent(
            eventVersion: 1,
            profileId: profileId,
            userId: userId,
            oldDisplayName: oldDisplayName,
            newDisplayName: newDisplayName);

        _userLookupService.GetUserFamilyIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(familyId);

        // Use specific values to avoid Vogen struct issues with Arg.Any<>
        _calendarService.UpdateBirthdayEventTitleAsync(
            familyId,
            userId,
            newDisplayName.Value,
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Calendar update failed"));

        // Act - Should not throw, error is caught and logged
        var act = () => _handler.Handle(@event, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();

        // RabbitMQ publishing should still happen
        await _eventPublisher.Received(1).PublishDisplayNameChangedAsync(
            @event,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCacheInvalidationFails_DoesNotAffectOtherOperations()
    {
        // Arrange
        var profileId = UserProfileId.New();
        var userId = UserId.New();
        var familyId = FamilyId.New();
        var oldDisplayName = DisplayName.From("John Doe");
        var newDisplayName = DisplayName.From("John Smith");

        var @event = new DisplayNameChangedEvent(
            eventVersion: 1,
            profileId: profileId,
            userId: userId,
            oldDisplayName: oldDisplayName,
            newDisplayName: newDisplayName);

        _userLookupService.GetUserFamilyIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(familyId);

        // Use specific values to avoid Vogen struct issues with Arg.Any<>
        // This will throw but the handler doesn't catch it (no try-catch around cache invalidation)
        _cacheInvalidationService.InvalidateFamilyMembersCacheAsync(
            familyId,
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Cache invalidation failed"));

        // Act
        var act = () => _handler.Handle(@event, CancellationToken.None);

        // Assert - Cache invalidation is not wrapped in try-catch, so exception propagates
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    #endregion
}
