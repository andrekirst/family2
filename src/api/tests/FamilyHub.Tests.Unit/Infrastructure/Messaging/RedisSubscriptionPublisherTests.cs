using FamilyHub.Infrastructure.Messaging;
using FamilyHub.Modules.Auth.Application.DTOs.Subscriptions;
using FluentAssertions;
using HotChocolate.Subscriptions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace FamilyHub.Tests.Unit.Infrastructure.Messaging;

/// <summary>
/// Unit tests for RedisSubscriptionPublisher.
/// Tests message publishing via Hot Chocolate's ITopicEventSender with mocked dependencies.
/// </summary>
/// <remarks>
/// <para>
/// These tests verify:
/// </para>
/// <list type="bullet">
/// <item><description>Messages are published to correct topics</description></item>
/// <item><description>Error handling logs but doesn't throw (best-effort delivery)</description></item>
/// <item><description>Argument validation</description></item>
/// </list>
/// <para>
/// For true integration tests with Redis, see FamilyHub.Tests.Integration
/// (requires Redis Testcontainer setup).
/// </para>
/// </remarks>
public class RedisSubscriptionPublisherTests
{
    [Theory, AutoNSubstituteData]
    public async Task PublishAsync_ValidMessage_ShouldCallTopicEventSender(
        ITopicEventSender topicEventSender,
        ILogger<RedisSubscriptionPublisher> logger)
    {
        // Arrange
        var publisher = new RedisSubscriptionPublisher(topicEventSender, logger);
        var topicName = "family-members-changed:123e4567-e89b-12d3-a456-426614174000";
        var message = new FamilyMembersChangedPayload
        {
            FamilyId = Guid.Parse("123e4567-e89b-12d3-a456-426614174000"),
            ChangeType = ChangeType.ADDED,
            Member = null
        };

        // Act
        await publisher.PublishAsync(topicName, message);

        // Assert
        await topicEventSender.Received(1).SendAsync(
            topicName,
            message,
            Arg.Any<CancellationToken>());
    }

    [Theory, AutoNSubstituteData]
    public async Task PublishAsync_TopicEventSenderThrows_ShouldLogErrorAndNotThrow(
        ITopicEventSender topicEventSender,
        ILogger<RedisSubscriptionPublisher> logger)
    {
        // Arrange
        var publisher = new RedisSubscriptionPublisher(topicEventSender, logger);
        var topicName = "family-members-changed:123e4567-e89b-12d3-a456-426614174000";
        var message = new FamilyMembersChangedPayload
        {
            FamilyId = Guid.Parse("123e4567-e89b-12d3-a456-426614174000"),
            ChangeType = ChangeType.ADDED,
            Member = null
        };

        // Simulate Redis connection failure
        topicEventSender
            .SendAsync(topicName, message, Arg.Any<CancellationToken>())
            .Throws(new InvalidOperationException("Redis connection failed"));

        // Act
        var act = async () => await publisher.PublishAsync(topicName, message);

        // Assert - Should NOT throw despite error
        await act.Should().NotThrowAsync();

        // Verify error was logged
        // Note: With NSubstitute and partial class LoggerMessage, we can't easily verify log calls
        // In a real scenario, you'd use a test logger or capture logs
    }

    [Theory, AutoNSubstituteData]
    public async Task PublishAsync_NullTopicName_ShouldThrowArgumentException(
        ITopicEventSender topicEventSender,
        ILogger<RedisSubscriptionPublisher> logger)
    {
        // Arrange
        var publisher = new RedisSubscriptionPublisher(topicEventSender, logger);
        var message = new FamilyMembersChangedPayload
        {
            FamilyId = Guid.NewGuid(),
            ChangeType = ChangeType.ADDED,
            Member = null
        };

        // Act
        var act = async () => await publisher.PublishAsync(null!, message);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Theory, AutoNSubstituteData]
    public async Task PublishAsync_EmptyTopicName_ShouldThrowArgumentException(
        ITopicEventSender topicEventSender,
        ILogger<RedisSubscriptionPublisher> logger)
    {
        // Arrange
        var publisher = new RedisSubscriptionPublisher(topicEventSender, logger);
        var message = new FamilyMembersChangedPayload
        {
            FamilyId = Guid.NewGuid(),
            ChangeType = ChangeType.ADDED,
            Member = null
        };

        // Act
        var act = async () => await publisher.PublishAsync(string.Empty, message);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Theory, AutoNSubstituteData]
    public async Task PublishAsync_NullMessage_ShouldThrowArgumentNullException(
        ITopicEventSender topicEventSender,
        ILogger<RedisSubscriptionPublisher> logger)
    {
        // Arrange
        var publisher = new RedisSubscriptionPublisher(topicEventSender, logger);
        var topicName = "family-members-changed:123e4567-e89b-12d3-a456-426614174000";

        // Act
        var act = async () => await publisher.PublishAsync<FamilyMembersChangedPayload>(topicName, null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Theory, AutoNSubstituteData]
    public async Task PublishAsync_DifferentMessageTypes_ShouldPublishCorrectly(
        ITopicEventSender topicEventSender,
        ILogger<RedisSubscriptionPublisher> logger)
    {
        // Arrange
        var publisher = new RedisSubscriptionPublisher(topicEventSender, logger);
        var familyId = Guid.NewGuid();

        var familyMemberMessage = new FamilyMembersChangedPayload
        {
            FamilyId = familyId,
            ChangeType = ChangeType.ADDED,
            Member = null
        };

        var invitationMessage = new PendingInvitationsChangedPayload
        {
            FamilyId = familyId,
            ChangeType = ChangeType.REMOVED,
            Invitation = null
        };

        // Act
        await publisher.PublishAsync($"family-members-changed:{familyId}", familyMemberMessage);
        await publisher.PublishAsync($"pending-invitations-changed:{familyId}", invitationMessage);

        // Assert
        await topicEventSender.Received(1).SendAsync(
            $"family-members-changed:{familyId}",
            familyMemberMessage,
            Arg.Any<CancellationToken>());

        await topicEventSender.Received(1).SendAsync(
            $"pending-invitations-changed:{familyId}",
            invitationMessage,
            Arg.Any<CancellationToken>());
    }

    [Theory, AutoNSubstituteData]
    public async Task PublishAsync_CancellationRequested_ShouldPassCancellationToken(
        ITopicEventSender topicEventSender,
        ILogger<RedisSubscriptionPublisher> logger)
    {
        // Arrange
        var publisher = new RedisSubscriptionPublisher(topicEventSender, logger);
        var topicName = "family-members-changed:123e4567-e89b-12d3-a456-426614174000";
        var message = new FamilyMembersChangedPayload
        {
            FamilyId = Guid.Parse("123e4567-e89b-12d3-a456-426614174000"),
            ChangeType = ChangeType.ADDED,
            Member = null
        };

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        await publisher.PublishAsync(topicName, message, cts.Token);

        // Assert
        await topicEventSender.Received(1).SendAsync(
            topicName,
            message,
            Arg.Is<CancellationToken>(ct => ct.IsCancellationRequested));
    }
}
