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
/// Unit tests for BirthdaySetEventHandler.
/// Tests event chain: Profile → Calendar → RabbitMQ publishing.
/// </summary>
public class BirthdaySetEventHandlerTests
{
    private readonly ICalendarService _calendarService;
    private readonly IUserLookupService _userLookupService;
    private readonly IProfileEventPublisher _eventPublisher;
    private readonly ILogger<BirthdaySetEventHandler> _logger;
    private readonly BirthdaySetEventHandler _handler;

    public BirthdaySetEventHandlerTests()
    {
        _calendarService = Substitute.For<ICalendarService>();
        _userLookupService = Substitute.For<IUserLookupService>();
        _eventPublisher = Substitute.For<IProfileEventPublisher>();
        _logger = Substitute.For<ILogger<BirthdaySetEventHandler>>();

        _handler = new BirthdaySetEventHandler(
            _calendarService,
            _userLookupService,
            _eventPublisher,
            _logger);
    }

    #region Happy Path Tests

    [Fact]
    public async Task Handle_WithFamilyId_CallsCalendarService()
    {
        // Arrange
        var profileId = UserProfileId.New();
        var userId = UserId.New();
        var familyId = FamilyId.New();
        var birthday = Birthday.From(new DateOnly(1990, 5, 15));
        var displayName = DisplayName.From("John Doe");

        var @event = new BirthdaySetEvent(
            eventVersion: 1,
            profileId: profileId,
            userId: userId,
            birthday: birthday,
            displayName: displayName);

        _userLookupService.GetUserFamilyIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(familyId);

        // Act
        await _handler.Handle(@event, CancellationToken.None);

        // Assert
        await _calendarService.Received(1).CreateRecurringBirthdayEventAsync(
            familyId,
            userId,
            displayName.Value,
            birthday.Value,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithFamilyId_PublishesToRabbitMQ()
    {
        // Arrange
        var profileId = UserProfileId.New();
        var userId = UserId.New();
        var familyId = FamilyId.New();
        var birthday = Birthday.From(new DateOnly(1990, 5, 15));
        var displayName = DisplayName.From("John Doe");

        var @event = new BirthdaySetEvent(
            eventVersion: 1,
            profileId: profileId,
            userId: userId,
            birthday: birthday,
            displayName: displayName);

        _userLookupService.GetUserFamilyIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(familyId);

        // Act
        await _handler.Handle(@event, CancellationToken.None);

        // Assert
        await _eventPublisher.Received(1).PublishBirthdaySetAsync(
            @event,
            Arg.Any<CancellationToken>());
    }

    #endregion

    #region No FamilyId Tests

    [Fact]
    public async Task Handle_WithoutFamilyId_SkipsCalendarService()
    {
        // Arrange
        var profileId = UserProfileId.New();
        var userId = UserId.New();
        var birthday = Birthday.From(new DateOnly(1990, 5, 15));
        var displayName = DisplayName.From("John Doe");

        var @event = new BirthdaySetEvent(
            eventVersion: 1,
            profileId: profileId,
            userId: userId,
            birthday: birthday,
            displayName: displayName);

        _userLookupService.GetUserFamilyIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns((FamilyId?)null);

        // Act
        await _handler.Handle(@event, CancellationToken.None);

        // Assert - Calendar service should NOT be called when no FamilyId
        // Use ReceivedCalls() count to avoid Vogen struct issues with Arg.Any<>
        _calendarService.ReceivedCalls()
            .Where(c => c.GetMethodInfo().Name == nameof(ICalendarService.CreateRecurringBirthdayEventAsync))
            .Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithoutFamilyId_StillPublishesToRabbitMQ()
    {
        // Arrange
        var profileId = UserProfileId.New();
        var userId = UserId.New();
        var birthday = Birthday.From(new DateOnly(1990, 5, 15));
        var displayName = DisplayName.From("John Doe");

        var @event = new BirthdaySetEvent(
            eventVersion: 1,
            profileId: profileId,
            userId: userId,
            birthday: birthday,
            displayName: displayName);

        _userLookupService.GetUserFamilyIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns((FamilyId?)null);

        // Act
        await _handler.Handle(@event, CancellationToken.None);

        // Assert - RabbitMQ should still be published (event recording)
        await _eventPublisher.Received(1).PublishBirthdaySetAsync(
            @event,
            Arg.Any<CancellationToken>());
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task Handle_WhenCalendarServiceFails_ContinuesProcessing()
    {
        // Arrange
        var profileId = UserProfileId.New();
        var userId = UserId.New();
        var familyId = FamilyId.New();
        var birthday = Birthday.From(new DateOnly(1990, 5, 15));
        var displayName = DisplayName.From("John Doe");

        var @event = new BirthdaySetEvent(
            eventVersion: 1,
            profileId: profileId,
            userId: userId,
            birthday: birthday,
            displayName: displayName);

        _userLookupService.GetUserFamilyIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(familyId);

        // Use specific values to avoid Vogen struct issues with Arg.Any<>
        _calendarService.CreateRecurringBirthdayEventAsync(
            familyId,
            userId,
            displayName.Value,
            birthday.Value,
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Calendar service error"));

        // Act - Should not throw, error is caught and logged
        var act = () => _handler.Handle(@event, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();

        // RabbitMQ publishing should still happen
        await _eventPublisher.Received(1).PublishBirthdaySetAsync(
            @event,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUserLookupFails_PropagatesException()
    {
        // Arrange
        var profileId = UserProfileId.New();
        var userId = UserId.New();
        var birthday = Birthday.From(new DateOnly(1990, 5, 15));
        var displayName = DisplayName.From("John Doe");

        var @event = new BirthdaySetEvent(
            eventVersion: 1,
            profileId: profileId,
            userId: userId,
            birthday: birthday,
            displayName: displayName);

        _userLookupService.GetUserFamilyIdAsync(userId, Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("User lookup failed"));

        // Act
        var act = () => _handler.Handle(@event, CancellationToken.None);

        // Assert - User lookup is critical, exception should propagate
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("User lookup failed");
    }

    #endregion
}
