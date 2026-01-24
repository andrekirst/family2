using FamilyHub.Modules.Auth.Application.Abstractions;
using FamilyHub.Modules.Auth.Application.DTOs.Subscriptions;
using FamilyHub.Modules.Auth.Domain;
using FamilyHub.Modules.Auth.Domain.Repositories;
using FamilyHub.Modules.Auth.Presentation.GraphQL.Subscriptions;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace FamilyHub.Tests.Unit.Auth.Presentation.GraphQL.Subscriptions;

/// <summary>
/// Unit tests for InvitationSubscriptions GraphQL subscription resolvers.
/// Tests authorization logic for subscription access (family membership and role checks).
/// </summary>
/// <remarks>
/// <para>
/// These tests verify that subscription authorization works correctly:
/// </para>
/// <list type="bullet">
/// <item><description>FamilyMembersChanged: Requires family membership (any role)</description></item>
/// <item><description>PendingInvitationsChanged: Requires OWNER or ADMIN role</description></item>
/// </list>
/// <para>
/// Authorization failures result in yield break (subscription terminates immediately).
/// </para>
/// </remarks>
public class InvitationSubscriptionsTests
{
    #region FamilyMembersChanged Tests

    [Theory, AutoNSubstituteData]
    public async Task FamilyMembersChanged_ShouldYieldMessage_WhenUserIsFamilyMember(
        IUserContext userContext,
        IUserRepository userRepository,
        ILogger<InvitationSubscriptions> logger)
    {
        // Arrange
        var familyId = FamilyId.New();
        var currentUserId = UserId.New();
        var user = User.CreateFromOAuth(
            Email.From("member@example.com"),
            "external-user-id",
            "oauth-provider",
            familyId);

        // User is a member of the family (via familyId in CreateFromOAuth)

        userContext.UserId.Returns(currentUserId);
        userRepository.GetByIdAsync(currentUserId, Arg.Any<CancellationToken>())
            .Returns(user);

        var message = new FamilyMembersChangedPayload
        {
            FamilyId = familyId.Value,
            ChangeType = ChangeType.ADDED,
            Member = null
        };

        var subscriptions = new InvitationSubscriptions();

        // Act
        var results = new List<FamilyMembersChangedPayload>();
        await foreach (var result in subscriptions.FamilyMembersChanged(
            familyId.Value,
            userContext,
            userRepository,
            message,
            logger,
            CancellationToken.None))
        {
            results.Add(result);
        }

        // Assert
        results.Should().HaveCount(1);
        results[0].Should().Be(message);
    }

    [Theory, AutoNSubstituteData]
    public async Task FamilyMembersChanged_ShouldYieldNothing_WhenUserNotFamilyMember(
        IUserContext userContext,
        IUserRepository userRepository,
        ILogger<InvitationSubscriptions> logger)
    {
        // Arrange
        var familyId = FamilyId.New();
        var currentUserId = UserId.New();
        var user = User.CreateFromOAuth(
            Email.From("nonmember@example.com"),
            "external-user-id",
            "oauth-provider",
            FamilyId.New());

        // User is NOT a member of any family (different familyId)
        userContext.UserId.Returns(currentUserId);
        userRepository.GetByIdAsync(currentUserId, Arg.Any<CancellationToken>())
            .Returns(user);

        var message = new FamilyMembersChangedPayload
        {
            FamilyId = familyId.Value,
            ChangeType = ChangeType.ADDED,
            Member = null
        };

        var subscriptions = new InvitationSubscriptions();

        // Act
        var results = new List<FamilyMembersChangedPayload>();
        await foreach (var result in subscriptions.FamilyMembersChanged(
            familyId.Value,
            userContext,
            userRepository,
            message,
            logger,
            CancellationToken.None))
        {
            results.Add(result);
        }

        // Assert
        results.Should().BeEmpty();
    }

    [Theory, AutoNSubstituteData]
    public async Task FamilyMembersChanged_ShouldYieldNothing_WhenUserNotFound(
        IUserContext userContext,
        IUserRepository userRepository,
        ILogger<InvitationSubscriptions> logger)
    {
        // Arrange
        var familyId = FamilyId.New();
        var currentUserId = UserId.New();

        userContext.UserId.Returns(currentUserId);
        userRepository.GetByIdAsync(currentUserId, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var message = new FamilyMembersChangedPayload
        {
            FamilyId = familyId.Value,
            ChangeType = ChangeType.ADDED,
            Member = null
        };

        var subscriptions = new InvitationSubscriptions();

        // Act
        var results = new List<FamilyMembersChangedPayload>();
        await foreach (var result in subscriptions.FamilyMembersChanged(
            familyId.Value,
            userContext,
            userRepository,
            message,
            logger,
            CancellationToken.None))
        {
            results.Add(result);
        }

        // Assert
        results.Should().BeEmpty();
    }

    #endregion

    #region PendingInvitationsChanged Tests

    [Theory, AutoNSubstituteData]
    public async Task PendingInvitationsChanged_ShouldYieldMessage_WhenUserIsOwner(
        IUserContext userContext,
        IUserRepository userRepository,
        ILogger<InvitationSubscriptions> logger)
    {
        // Arrange
        var familyId = FamilyId.New();
        var currentUserId = UserId.New();
        var user = User.CreateFromOAuth(
            Email.From("owner@example.com"),
            "external-user-id",
            "oauth-provider",
            familyId);

        // User is OWNER of the family
        user.UpdateRole(FamilyRole.Owner);

        userContext.UserId.Returns(currentUserId);
        userRepository.GetByIdAsync(currentUserId, Arg.Any<CancellationToken>())
            .Returns(user);

        var message = new PendingInvitationsChangedPayload
        {
            FamilyId = familyId.Value,
            ChangeType = ChangeType.ADDED,
            Invitation = null
        };

        var subscriptions = new InvitationSubscriptions();

        // Act
        var results = new List<PendingInvitationsChangedPayload>();
        await foreach (var result in subscriptions.PendingInvitationsChanged(
            familyId.Value,
            userContext,
            userRepository,
            message,
            logger,
            CancellationToken.None))
        {
            results.Add(result);
        }

        // Assert
        results.Should().HaveCount(1);
        results[0].Should().Be(message);
    }

    [Theory, AutoNSubstituteData]
    public async Task PendingInvitationsChanged_ShouldYieldMessage_WhenUserIsAdmin(
        IUserContext userContext,
        IUserRepository userRepository,
        ILogger<InvitationSubscriptions> logger)
    {
        // Arrange
        var familyId = FamilyId.New();
        var currentUserId = UserId.New();
        var user = User.CreateFromOAuth(
            Email.From("admin@example.com"),
            "external-user-id",
            "oauth-provider",
            familyId);

        // User is ADMIN of the family
        user.UpdateRole(FamilyRole.Admin);

        userContext.UserId.Returns(currentUserId);
        userRepository.GetByIdAsync(currentUserId, Arg.Any<CancellationToken>())
            .Returns(user);

        var message = new PendingInvitationsChangedPayload
        {
            FamilyId = familyId.Value,
            ChangeType = ChangeType.ADDED,
            Invitation = null
        };

        var subscriptions = new InvitationSubscriptions();

        // Act
        var results = new List<PendingInvitationsChangedPayload>();
        await foreach (var result in subscriptions.PendingInvitationsChanged(
            familyId.Value,
            userContext,
            userRepository,
            message,
            logger,
            CancellationToken.None))
        {
            results.Add(result);
        }

        // Assert
        results.Should().HaveCount(1);
        results[0].Should().Be(message);
    }

    [Theory, AutoNSubstituteData]
    public async Task PendingInvitationsChanged_ShouldYieldNothing_WhenUserIsMember(
        IUserContext userContext,
        IUserRepository userRepository,
        ILogger<InvitationSubscriptions> logger)
    {
        // Arrange
        var familyId = FamilyId.New();
        var currentUserId = UserId.New();
        var user = User.CreateFromOAuth(
            Email.From("member@example.com"),
            "external-user-id",
            "oauth-provider",
            familyId);

        // User is MEMBER (not OWNER or ADMIN)
        user.UpdateRole(FamilyRole.Member);

        userContext.UserId.Returns(currentUserId);
        userRepository.GetByIdAsync(currentUserId, Arg.Any<CancellationToken>())
            .Returns(user);

        var message = new PendingInvitationsChangedPayload
        {
            FamilyId = familyId.Value,
            ChangeType = ChangeType.ADDED,
            Invitation = null
        };

        var subscriptions = new InvitationSubscriptions();

        // Act
        var results = new List<PendingInvitationsChangedPayload>();
        await foreach (var result in subscriptions.PendingInvitationsChanged(
            familyId.Value,
            userContext,
            userRepository,
            message,
            logger,
            CancellationToken.None))
        {
            results.Add(result);
        }

        // Assert
        results.Should().BeEmpty();
    }

    [Theory, AutoNSubstituteData]
    public async Task PendingInvitationsChanged_ShouldYieldNothing_WhenUserNotFamilyMember(
        IUserContext userContext,
        IUserRepository userRepository,
        ILogger<InvitationSubscriptions> logger)
    {
        // Arrange
        var familyId = FamilyId.New();
        var currentUserId = UserId.New();
        var user = User.CreateFromOAuth(
            Email.From("nonmember@example.com"),
            "external-user-id",
            "oauth-provider",
            FamilyId.New());

        // User is NOT a member of any family (different familyId)
        userContext.UserId.Returns(currentUserId);
        userRepository.GetByIdAsync(currentUserId, Arg.Any<CancellationToken>())
            .Returns(user);

        var message = new PendingInvitationsChangedPayload
        {
            FamilyId = familyId.Value,
            ChangeType = ChangeType.ADDED,
            Invitation = null
        };

        var subscriptions = new InvitationSubscriptions();

        // Act
        var results = new List<PendingInvitationsChangedPayload>();
        await foreach (var result in subscriptions.PendingInvitationsChanged(
            familyId.Value,
            userContext,
            userRepository,
            message,
            logger,
            CancellationToken.None))
        {
            results.Add(result);
        }

        // Assert
        results.Should().BeEmpty();
    }

    [Theory, AutoNSubstituteData]
    public async Task PendingInvitationsChanged_ShouldYieldNothing_WhenUserNotFound(
        IUserContext userContext,
        IUserRepository userRepository,
        ILogger<InvitationSubscriptions> logger)
    {
        // Arrange
        var familyId = FamilyId.New();
        var currentUserId = UserId.New();

        userContext.UserId.Returns(currentUserId);
        userRepository.GetByIdAsync(currentUserId, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var message = new PendingInvitationsChangedPayload
        {
            FamilyId = familyId.Value,
            ChangeType = ChangeType.ADDED,
            Invitation = null
        };

        var subscriptions = new InvitationSubscriptions();

        // Act
        var results = new List<PendingInvitationsChangedPayload>();
        await foreach (var result in subscriptions.PendingInvitationsChanged(
            familyId.Value,
            userContext,
            userRepository,
            message,
            logger,
            CancellationToken.None))
        {
            results.Add(result);
        }

        // Assert
        results.Should().BeEmpty();
    }

    #endregion
}
