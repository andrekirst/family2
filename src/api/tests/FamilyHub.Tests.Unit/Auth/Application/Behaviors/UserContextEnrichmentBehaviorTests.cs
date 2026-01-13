using AutoFixture.Xunit2;
using FamilyHub.Modules.Auth.Application.Abstractions;
using FamilyHub.Modules.Auth.Application.Behaviors;
using FamilyHub.Modules.Auth.Application.Services;
using FamilyHub.Modules.Auth.Domain;
using FamilyHub.Modules.Auth.Domain.Repositories;
using FamilyHub.SharedKernel.Application.Abstractions.Authorization;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace FamilyHub.Tests.Unit.Auth.Application.Behaviors;

/// <summary>
/// Unit tests for UserContextEnrichmentBehavior.
/// Tests user context enrichment logic, public query handling, authentication validation, and edge cases.
/// Uses NSubstitute for mocking with AutoFixture attribute-based dependency injection.
/// Uses FluentAssertions for readable, expressive test assertions.
/// </summary>
public sealed class UserContextEnrichmentBehaviorTests
{
    #region Test Request Types

    public record PublicTestQuery : IRequest<string>, IPublicQuery;
    public record AuthenticatedTestCommand : IRequest<string>, IRequireAuthentication;
    public record UnannotatedTestQuery : IRequest<string>;

    #endregion

    #region Public Query Handling

    [Theory, AutoNSubstituteData]
    public async Task Handle_WithPublicQuery_ShouldSkipEnrichmentAndCallNextDirectly(
        [Frozen] ICurrentUserService currentUserService,
        [Frozen] IUserRepository userRepository,
        [Frozen] IUserContext userContext,
        ILogger<UserContextEnrichmentBehavior<PublicTestQuery, string>> logger)
    {
        // Arrange
        var request = new PublicTestQuery();
        var expectedResult = "public_result";
        var next = Substitute.For<RequestHandlerDelegate<string>>();
        next().Returns(expectedResult);

        var behavior = new UserContextEnrichmentBehavior<PublicTestQuery, string>(
            currentUserService,
            userRepository,
            userContext,
            logger);

        // Act
        var result = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        await next.Received(1)();
        currentUserService.ReceivedCalls().Should().BeEmpty();
        userRepository.ReceivedCalls().Should().BeEmpty();
    }

    #endregion

    #region Authentication Required Handling

    [Theory, AutoNSubstituteData]
    public async Task Handle_WithAuthenticationRequired_ShouldLoadUserAndPopulateContext(
        [Frozen] ICurrentUserService currentUserService,
        [Frozen] IUserRepository userRepository,
        ILogger<UserContextEnrichmentBehavior<AuthenticatedTestCommand, string>> logger,
        User user)
    {
        // Arrange
        var request = new AuthenticatedTestCommand();
        var expectedResult = "authenticated_result";
        var userId = user.Id;

        currentUserService.GetUserIdAsync(Arg.Any<CancellationToken>())
            .Returns(userId);

        userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        var userContextService = new UserContextService();
        var next = Substitute.For<RequestHandlerDelegate<string>>();
        next().Returns(expectedResult);

        var behavior = new UserContextEnrichmentBehavior<AuthenticatedTestCommand, string>(
            currentUserService,
            userRepository,
            userContextService,
            logger);

        // Act
        var result = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        userContextService.User.Should().Be(user);
        userContextService.User.Id.Should().Be(userId);
        await currentUserService.Received(1).GetUserIdAsync(Arg.Any<CancellationToken>());
        await userRepository.Received(1).GetByIdAsync(userId, Arg.Any<CancellationToken>());
        await next.Received(1)();
    }

    [Theory, AutoNSubstituteData]
    public async Task Handle_WithAuthenticationRequired_WhenUserNotFound_ShouldThrowUnauthorizedAccessException(
        [Frozen] ICurrentUserService currentUserService,
        [Frozen] IUserRepository userRepository,
        [Frozen] IUserContext userContext,
        ILogger<UserContextEnrichmentBehavior<AuthenticatedTestCommand, string>> logger)
    {
        // Arrange
        var request = new AuthenticatedTestCommand();
        var userId = UserId.New();

        currentUserService.GetUserIdAsync(Arg.Any<CancellationToken>())
            .Returns(userId);

        userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var next = Substitute.For<RequestHandlerDelegate<string>>();
        next().Returns("should_not_reach");

        var behavior = new UserContextEnrichmentBehavior<AuthenticatedTestCommand, string>(
            currentUserService,
            userRepository,
            userContext,
            logger);

        // Act
        var act = () => behavior.Handle(request, next, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage($"*{userId.Value}*not found*");
        await currentUserService.Received(1).GetUserIdAsync(Arg.Any<CancellationToken>());
        await userRepository.Received(1).GetByIdAsync(userId, Arg.Any<CancellationToken>());
        await next.DidNotReceive()();
    }

    [Theory, AutoNSubstituteData]
    public async Task Handle_WithNoMarkerInterface_ShouldSkipEnrichmentAndCallNext(
        [Frozen] ICurrentUserService currentUserService,
        [Frozen] IUserRepository userRepository,
        [Frozen] IUserContext userContext,
        ILogger<UserContextEnrichmentBehavior<UnannotatedTestQuery, string>> logger)
    {
        // Arrange
        var request = new UnannotatedTestQuery();
        var expectedResult = "unannotated_result";
        var next = Substitute.For<RequestHandlerDelegate<string>>();
        next().Returns(expectedResult);

        var behavior = new UserContextEnrichmentBehavior<UnannotatedTestQuery, string>(
            currentUserService,
            userRepository,
            userContext,
            logger);

        // Act
        var result = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        await next.Received(1)();
        currentUserService.ReceivedCalls().Should().BeEmpty();
        userRepository.ReceivedCalls().Should().BeEmpty();
    }

    #endregion

    #region Edge Cases

    [Theory, AutoNSubstituteData]
    public async Task Handle_ShouldPassCancellationTokenToServices(
        [Frozen] ICurrentUserService currentUserService,
        [Frozen] IUserRepository userRepository,
        ILogger<UserContextEnrichmentBehavior<AuthenticatedTestCommand, string>> logger,
        User user)
    {
        // Arrange
        var request = new AuthenticatedTestCommand();
        var userId = user.Id;
        var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;

        currentUserService.GetUserIdAsync(cancellationToken)
            .Returns(userId);

        userRepository.GetByIdAsync(userId, cancellationToken)
            .Returns(user);

        var userContextService = new UserContextService();
        var next = Substitute.For<RequestHandlerDelegate<string>>();
        next().Returns("result");

        var behavior = new UserContextEnrichmentBehavior<AuthenticatedTestCommand, string>(
            currentUserService,
            userRepository,
            userContextService,
            logger);

        // Act
        await behavior.Handle(request, next, cancellationToken);

        // Assert
        await currentUserService.Received(1).GetUserIdAsync(cancellationToken);
        await userRepository.Received(1).GetByIdAsync(userId, cancellationToken);
    }

    #endregion
}
