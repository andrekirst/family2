using AutoFixture.Xunit2;
using FamilyHub.Modules.Auth.Application.Abstractions;
using FamilyHub.Modules.Auth.Application.Behaviors;
using FamilyHub.Modules.Auth.Domain;
using FamilyHub.Modules.Auth.Domain.ValueObjects;
using FamilyHub.SharedKernel.Application.Abstractions.Authorization;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Security.Claims;

namespace FamilyHub.Tests.Unit.Auth.Application.Behaviors;

/// <summary>
/// Unit tests for AuthorizationBehavior.
/// Tests authorization logic, family context validation, role-based authorization, and edge cases.
/// Uses NSubstitute for mocking with AutoFixture attribute-based dependency injection.
/// Uses FluentAssertions for readable, expressive test assertions.
/// </summary>
public sealed partial class AuthorizationBehaviorTests
{
    #region Test Request Types

    public record PublicTestQuery() : IRequest<string>, IPublicQuery;
    public record AuthenticatedOnlyCommand() : IRequest<string>, IRequireAuthentication;
    public record FamilyContextCommand() : IRequest<string>, IRequireAuthentication, IRequireFamilyContext;
    public record OwnerOnlyCommand() : IRequest<string>, IRequireAuthentication, IRequireOwnerRole;
    public record AdminOnlyCommand() : IRequest<string>, IRequireAuthentication, IRequireAdminRole;
    public record OwnerOrAdminCommand() : IRequest<string>, IRequireAuthentication, IRequireOwnerOrAdminRole;

    #endregion

    #region Public Query Handling

    [Theory, AutoNSubstituteData]
    public async Task Handle_WithPublicQuery_ShouldSkipAuthorizationAndCallNext(
        [Frozen] IUserContext userContext,
        [Frozen] IAuthorizationService authorizationService,
        ILogger<AuthorizationBehavior<PublicTestQuery, string>> logger)
    {
        // Arrange
        var request = new PublicTestQuery();
        var expectedResult = "public_result";
        var next = Substitute.For<RequestHandlerDelegate<string>>();
        next().Returns(expectedResult);

        var behavior = new AuthorizationBehavior<PublicTestQuery, string>(
            userContext,
            authorizationService,
            logger);

        // Act
        var result = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        await next.Received(1)();
        authorizationService.ReceivedCalls().Should().BeEmpty();
    }

    #endregion

    #region Family Context Validation

    [Theory, AutoNSubstituteData]
    public async Task Handle_WithFamilyContextRequired_WhenUserHasFamily_ShouldCallNext(
        [Frozen] IUserContext userContext,
        [Frozen] IAuthorizationService authorizationService,
        ILogger<AuthorizationBehavior<FamilyContextCommand, string>> logger,
        User user)
    {
        // Arrange
        var request = new FamilyContextCommand();
        var expectedResult = "family_context_result";
        var familyId = FamilyId.From(Guid.NewGuid());

        userContext.User.Returns(user);
        userContext.UserId.Returns(user.Id);
        userContext.FamilyId.Returns(familyId);
        userContext.Role.Returns(user.Role);
        userContext.Email.Returns(user.Email);

        var next = Substitute.For<RequestHandlerDelegate<string>>();
        next().Returns(expectedResult);

        var behavior = new AuthorizationBehavior<FamilyContextCommand, string>(
            userContext,
            authorizationService,
            logger);

        // Act
        var result = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        await next.Received(1)();
    }

    [Theory, AutoNSubstituteData]
    public async Task Handle_WithFamilyContextRequired_WhenUserHasNoFamily_ShouldThrowUnauthorizedAccessException(
        [Frozen] IUserContext userContext,
        [Frozen] IAuthorizationService authorizationService,
        ILogger<AuthorizationBehavior<FamilyContextCommand, string>> logger,
        User user)
    {
        // Arrange
        var request = new FamilyContextCommand();
        var emptyFamilyId = FamilyId.From(Guid.Empty);

        userContext.User.Returns(user);
        userContext.UserId.Returns(user.Id);
        userContext.FamilyId.Returns(emptyFamilyId);
        userContext.Role.Returns(user.Role);
        userContext.Email.Returns(user.Email);

        var next = Substitute.For<RequestHandlerDelegate<string>>();
        next().Returns("should_not_reach");

        var behavior = new AuthorizationBehavior<FamilyContextCommand, string>(
            userContext,
            authorizationService,
            logger);

        // Act
        var act = () => behavior.Handle(request, next, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*does not belong to a family*");
        await next.DidNotReceive()();
    }

    #endregion

    #region Owner Role Authorization

    [Theory, AutoNSubstituteData]
    public async Task Handle_WithOwnerRoleRequired_WhenUserIsOwner_ShouldCallNext(
        [Frozen] IUserContext userContext,
        [Frozen] IAuthorizationService authorizationService,
        ILogger<AuthorizationBehavior<OwnerOnlyCommand, string>> logger)
    {
        // Arrange
        var request = new OwnerOnlyCommand();
        var expectedResult = "owner_result";
        var userId = UserId.New();
        var email = Email.From("owner@example.com");
        var familyId = FamilyId.From(Guid.NewGuid());

        var user = User.CreateFromOAuth(email, "ext-123", "zitadel", familyId);

        userContext.User.Returns(user);
        userContext.UserId.Returns(userId);
        userContext.FamilyId.Returns(familyId);
        userContext.Role.Returns(FamilyRole.Owner);
        userContext.Email.Returns(email);

        var successResult = AuthorizationResult.Success();
        authorizationService.AuthorizeAsync(
            Arg.Any<ClaimsPrincipal>(),
            Arg.Any<object?>(),
            "RequireOwner")
            .Returns(successResult);

        var next = Substitute.For<RequestHandlerDelegate<string>>();
        next().Returns(expectedResult);

        var behavior = new AuthorizationBehavior<OwnerOnlyCommand, string>(
            userContext,
            authorizationService,
            logger);

        // Act
        var result = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        await authorizationService.Received(1).AuthorizeAsync(
            Arg.Is<ClaimsPrincipal>(p =>
                p.FindFirst(ClaimTypes.NameIdentifier)!.Value == userId.Value.ToString() &&
                p.FindFirst(ClaimTypes.Email)!.Value == email.Value &&
                p.FindFirst(ClaimTypes.Role)!.Value == FamilyRole.Owner.Value),
            Arg.Any<object?>(),
            "RequireOwner");
        await next.Received(1)();
    }

    [Theory, AutoNSubstituteData]
    public async Task Handle_WithOwnerRoleRequired_WhenUserIsNotOwner_ShouldThrowUnauthorizedAccessException(
        [Frozen] IUserContext userContext,
        [Frozen] IAuthorizationService authorizationService,
        ILogger<AuthorizationBehavior<OwnerOnlyCommand, string>> logger)
    {
        // Arrange
        var request = new OwnerOnlyCommand();
        var userId = UserId.New();
        var email = Email.From("member@example.com");
        var familyId = FamilyId.From(Guid.NewGuid());

        var user = User.CreateFromOAuth(email, "ext-456", "zitadel", familyId);
        user.UpdateRole(FamilyRole.Member);

        userContext.User.Returns(user);
        userContext.UserId.Returns(userId);
        userContext.FamilyId.Returns(familyId);
        userContext.Role.Returns(FamilyRole.Member);
        userContext.Email.Returns(email);

        var failureResult = AuthorizationResult.Failed();
        authorizationService.AuthorizeAsync(
            Arg.Any<ClaimsPrincipal>(),
            Arg.Any<object?>(),
            "RequireOwner")
            .Returns(failureResult);

        var next = Substitute.For<RequestHandlerDelegate<string>>();
        next().Returns("should_not_reach");

        var behavior = new AuthorizationBehavior<OwnerOnlyCommand, string>(
            userContext,
            authorizationService,
            logger);

        // Act
        var act = () => behavior.Handle(request, next, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*does not have the required role*RequireOwner*");
        await authorizationService.Received(1).AuthorizeAsync(
            Arg.Any<ClaimsPrincipal>(),
            Arg.Any<object?>(),
            "RequireOwner");
        await next.DidNotReceive()();
    }

    #endregion

    #region Admin Role Authorization

    [Theory, AutoNSubstituteData]
    public async Task Handle_WithAdminRoleRequired_WhenUserIsAdmin_ShouldCallNext(
        [Frozen] IUserContext userContext,
        [Frozen] IAuthorizationService authorizationService,
        ILogger<AuthorizationBehavior<AdminOnlyCommand, string>> logger)
    {
        // Arrange
        var request = new AdminOnlyCommand();
        var expectedResult = "admin_result";
        var userId = UserId.New();
        var email = Email.From("admin@example.com");
        var familyId = FamilyId.From(Guid.NewGuid());

        var user = User.CreateFromOAuth(email, "ext-789", "zitadel", familyId);
        user.UpdateRole(FamilyRole.Admin);

        userContext.User.Returns(user);
        userContext.UserId.Returns(userId);
        userContext.FamilyId.Returns(familyId);
        userContext.Role.Returns(FamilyRole.Admin);
        userContext.Email.Returns(email);

        var successResult = AuthorizationResult.Success();
        authorizationService.AuthorizeAsync(
            Arg.Any<ClaimsPrincipal>(),
            Arg.Any<object?>(),
            "RequireAdmin")
            .Returns(successResult);

        var next = Substitute.For<RequestHandlerDelegate<string>>();
        next().Returns(expectedResult);

        var behavior = new AuthorizationBehavior<AdminOnlyCommand, string>(
            userContext,
            authorizationService,
            logger);

        // Act
        var result = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        await authorizationService.Received(1).AuthorizeAsync(
            Arg.Is<ClaimsPrincipal>(p => p.FindFirst(ClaimTypes.Role)!.Value == FamilyRole.Admin.Value),
            Arg.Any<object?>(),
            "RequireAdmin");
        await next.Received(1)();
    }

    [Theory, AutoNSubstituteData]
    public async Task Handle_WithAdminRoleRequired_WhenUserIsNotAdmin_ShouldThrowUnauthorizedAccessException(
        [Frozen] IUserContext userContext,
        [Frozen] IAuthorizationService authorizationService,
        ILogger<AuthorizationBehavior<AdminOnlyCommand, string>> logger)
    {
        // Arrange
        var request = new AdminOnlyCommand();
        var userId = UserId.New();
        var email = Email.From("member@example.com");
        var familyId = FamilyId.From(Guid.NewGuid());

        var user = User.CreateFromOAuth(email, "ext-999", "zitadel", familyId);
        user.UpdateRole(FamilyRole.Member);

        userContext.User.Returns(user);
        userContext.UserId.Returns(userId);
        userContext.FamilyId.Returns(familyId);
        userContext.Role.Returns(FamilyRole.Member);
        userContext.Email.Returns(email);

        var failureResult = AuthorizationResult.Failed();
        authorizationService.AuthorizeAsync(
            Arg.Any<ClaimsPrincipal>(),
            Arg.Any<object?>(),
            "RequireAdmin")
            .Returns(failureResult);

        var next = Substitute.For<RequestHandlerDelegate<string>>();
        next().Returns("should_not_reach");

        var behavior = new AuthorizationBehavior<AdminOnlyCommand, string>(
            userContext,
            authorizationService,
            logger);

        // Act
        var act = () => behavior.Handle(request, next, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*does not have the required role*RequireAdmin*");
        await next.DidNotReceive()();
    }

    #endregion

    #region Owner Or Admin Role Authorization

    [Theory, AutoNSubstituteData]
    public async Task Handle_WithOwnerOrAdminRequired_WhenUserIsOwner_ShouldCallNext(
        [Frozen] IUserContext userContext,
        [Frozen] IAuthorizationService authorizationService,
        ILogger<AuthorizationBehavior<OwnerOrAdminCommand, string>> logger)
    {
        // Arrange
        var request = new OwnerOrAdminCommand();
        var expectedResult = "owner_or_admin_result";
        var userId = UserId.New();
        var email = Email.From("owner@example.com");
        var familyId = FamilyId.From(Guid.NewGuid());

        var user = User.CreateFromOAuth(email, "ext-111", "zitadel", familyId);

        userContext.User.Returns(user);
        userContext.UserId.Returns(userId);
        userContext.FamilyId.Returns(familyId);
        userContext.Role.Returns(FamilyRole.Owner);
        userContext.Email.Returns(email);

        var successResult = AuthorizationResult.Success();
        authorizationService.AuthorizeAsync(
            Arg.Any<ClaimsPrincipal>(),
            Arg.Any<object?>(),
            "RequireOwnerOrAdmin")
            .Returns(successResult);

        var next = Substitute.For<RequestHandlerDelegate<string>>();
        next().Returns(expectedResult);

        var behavior = new AuthorizationBehavior<OwnerOrAdminCommand, string>(
            userContext,
            authorizationService,
            logger);

        // Act
        var result = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        await authorizationService.Received(1).AuthorizeAsync(
            Arg.Is<ClaimsPrincipal>(p => p.FindFirst(ClaimTypes.Role)!.Value == FamilyRole.Owner.Value),
            Arg.Any<object?>(),
            "RequireOwnerOrAdmin");
        await next.Received(1)();
    }

    [Theory, AutoNSubstituteData]
    public async Task Handle_WithOwnerOrAdminRequired_WhenUserIsAdmin_ShouldCallNext(
        [Frozen] IUserContext userContext,
        [Frozen] IAuthorizationService authorizationService,
        ILogger<AuthorizationBehavior<OwnerOrAdminCommand, string>> logger)
    {
        // Arrange
        var request = new OwnerOrAdminCommand();
        var expectedResult = "owner_or_admin_result";
        var userId = UserId.New();
        var email = Email.From("admin@example.com");
        var familyId = FamilyId.From(Guid.NewGuid());

        var user = User.CreateFromOAuth(email, "ext-222", "zitadel", familyId);
        user.UpdateRole(FamilyRole.Admin);

        userContext.User.Returns(user);
        userContext.UserId.Returns(userId);
        userContext.FamilyId.Returns(familyId);
        userContext.Role.Returns(FamilyRole.Admin);
        userContext.Email.Returns(email);

        var successResult = AuthorizationResult.Success();
        authorizationService.AuthorizeAsync(
            Arg.Any<ClaimsPrincipal>(),
            Arg.Any<object?>(),
            "RequireOwnerOrAdmin")
            .Returns(successResult);

        var next = Substitute.For<RequestHandlerDelegate<string>>();
        next().Returns(expectedResult);

        var behavior = new AuthorizationBehavior<OwnerOrAdminCommand, string>(
            userContext,
            authorizationService,
            logger);

        // Act
        var result = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        await authorizationService.Received(1).AuthorizeAsync(
            Arg.Is<ClaimsPrincipal>(p => p.FindFirst(ClaimTypes.Role)!.Value == FamilyRole.Admin.Value),
            Arg.Any<object?>(),
            "RequireOwnerOrAdmin");
        await next.Received(1)();
    }

    [Theory, AutoNSubstituteData]
    public async Task Handle_WithOwnerOrAdminRequired_WhenUserIsMember_ShouldThrowUnauthorizedAccessException(
        [Frozen] IUserContext userContext,
        [Frozen] IAuthorizationService authorizationService,
        ILogger<AuthorizationBehavior<OwnerOrAdminCommand, string>> logger)
    {
        // Arrange
        var request = new OwnerOrAdminCommand();
        var userId = UserId.New();
        var email = Email.From("member@example.com");
        var familyId = FamilyId.From(Guid.NewGuid());

        var user = User.CreateFromOAuth(email, "ext-333", "zitadel", familyId);
        user.UpdateRole(FamilyRole.Member);

        userContext.User.Returns(user);
        userContext.UserId.Returns(userId);
        userContext.FamilyId.Returns(familyId);
        userContext.Role.Returns(FamilyRole.Member);
        userContext.Email.Returns(email);

        var failureResult = AuthorizationResult.Failed();
        authorizationService.AuthorizeAsync(
            Arg.Any<ClaimsPrincipal>(),
            Arg.Any<object?>(),
            "RequireOwnerOrAdmin")
            .Returns(failureResult);

        var next = Substitute.For<RequestHandlerDelegate<string>>();
        next().Returns("should_not_reach");

        var behavior = new AuthorizationBehavior<OwnerOrAdminCommand, string>(
            userContext,
            authorizationService,
            logger);

        // Act
        var act = () => behavior.Handle(request, next, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*does not have the required role*RequireOwnerOrAdmin*");
        await next.DidNotReceive()();
    }

    #endregion

    #region Edge Cases

    [Theory, AutoNSubstituteData]
    public async Task Handle_WithNoRoleMarkers_ShouldSkipRoleCheckAndCallNext(
        [Frozen] IUserContext userContext,
        [Frozen] IAuthorizationService authorizationService,
        ILogger<AuthorizationBehavior<AuthenticatedOnlyCommand, string>> logger,
        User user)
    {
        // Arrange
        var request = new AuthenticatedOnlyCommand();
        var expectedResult = "authenticated_result";

        userContext.User.Returns(user);
        userContext.UserId.Returns(user.Id);
        userContext.FamilyId.Returns(user.FamilyId);
        userContext.Role.Returns(user.Role);
        userContext.Email.Returns(user.Email);

        var next = Substitute.For<RequestHandlerDelegate<string>>();
        next().Returns(expectedResult);

        var behavior = new AuthorizationBehavior<AuthenticatedOnlyCommand, string>(
            userContext,
            authorizationService,
            logger);

        // Act
        var result = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        await next.Received(1)();
        authorizationService.ReceivedCalls().Should().BeEmpty();
    }

    [Theory, AutoNSubstituteData]
    public async Task Handle_ShouldCreateClaimsPrincipalWithCorrectClaims(
        [Frozen] IUserContext userContext,
        [Frozen] IAuthorizationService authorizationService,
        ILogger<AuthorizationBehavior<OwnerOnlyCommand, string>> logger)
    {
        // Arrange
        var request = new OwnerOnlyCommand();
        var userId = UserId.New();
        var email = Email.From("test@example.com");
        var familyId = FamilyId.From(Guid.NewGuid());

        var user = User.CreateFromOAuth(email, "ext-444", "zitadel", familyId);

        userContext.User.Returns(user);
        userContext.UserId.Returns(userId);
        userContext.FamilyId.Returns(familyId);
        userContext.Role.Returns(FamilyRole.Owner);
        userContext.Email.Returns(email);

        ClaimsPrincipal? capturedPrincipal = null;
        var successResult = AuthorizationResult.Success();
        authorizationService.AuthorizeAsync(
            Arg.Do<ClaimsPrincipal>(p => capturedPrincipal = p),
            Arg.Any<object?>(),
            "RequireOwner")
            .Returns(successResult);

        var next = Substitute.For<RequestHandlerDelegate<string>>();
        next().Returns("result");

        var behavior = new AuthorizationBehavior<OwnerOnlyCommand, string>(
            userContext,
            authorizationService,
            logger);

        // Act
        await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        capturedPrincipal.Should().NotBeNull();
        capturedPrincipal!.FindFirst(ClaimTypes.NameIdentifier)!.Value.Should().Be(userId.Value.ToString());
        capturedPrincipal!.FindFirst(ClaimTypes.Email)!.Value.Should().Be(email.Value);
        capturedPrincipal!.FindFirst(ClaimTypes.Role)!.Value.Should().Be(FamilyRole.Owner.Value);
    }

    #endregion
}
