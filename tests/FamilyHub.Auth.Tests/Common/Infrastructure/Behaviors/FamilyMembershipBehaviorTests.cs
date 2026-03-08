using System.Security.Claims;
using FamilyHub.Api.Common.Infrastructure.Behaviors;
using FamilyHub.Api.Common.Infrastructure.FamilyScope;
using FamilyHub.Api.Features.Auth.Domain.Entities;
using FamilyHub.Api.Features.Auth.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;
using Microsoft.AspNetCore.Http;

namespace FamilyHub.Auth.Tests.Common.Infrastructure.Behaviors;

public class FamilyMembershipBehaviorTests
{
    private static readonly ExternalUserId TestExternalUserId =
        ExternalUserId.From(Guid.NewGuid().ToString());

    private static readonly FamilyId TestFamilyId = FamilyId.New();

    #region Test Messages

    private record IgnoredCommand(string Value) : FamilyHub.Common.Application.ICommand<string>, IIgnoreFamilyMembership;

    private record FamilyScopedCommand(string Value, FamilyId FamilyId) : FamilyHub.Common.Application.ICommand<string>, IFamilyScoped;

    private record RegularCommand(string Value) : FamilyHub.Common.Application.ICommand<string>;

    #endregion

    #region Helpers

    private static User CreateUser(FamilyId? familyId = null)
    {
        var user = User.Register(
            Email.From("test@example.com"),
            UserName.From("Test User"),
            TestExternalUserId,
            emailVerified: true);

        if (familyId is not null)
        {
            user.AssignToFamily(familyId.Value);
        }

        return user;
    }

    private static IHttpContextAccessor CreateAuthenticatedAccessor(string subClaimValue)
    {
        var context = new DefaultHttpContext();
        var identity = new ClaimsIdentity(
            [new Claim("sub", subClaimValue)],
            authenticationType: "Bearer");
        context.User = new ClaimsPrincipal(identity);
        return new HttpContextAccessor { HttpContext = context };
    }

    private static IHttpContextAccessor CreateUnauthenticatedAccessor()
    {
        var context = new DefaultHttpContext();
        return new HttpContextAccessor { HttpContext = context };
    }

    private static IHttpContextAccessor CreateNullHttpContextAccessor()
    {
        return new HttpContextAccessor { HttpContext = null };
    }

    private static ValueTask<string> SuccessHandler(RegularCommand _, CancellationToken __) =>
        new("handler-executed");

    private static ValueTask<string> SuccessHandlerIgnored(IgnoredCommand _, CancellationToken __) =>
        new("handler-executed");

    private static ValueTask<string> SuccessHandlerScoped(FamilyScopedCommand _, CancellationToken __) =>
        new("handler-executed");

    #endregion

    // --- IIgnoreFamilyMembership tests ---

    [Fact]
    public async Task Should_skip_check_when_message_implements_IIgnoreFamilyMembership()
    {
        var accessor = CreateAuthenticatedAccessor(TestExternalUserId.Value);
        var userRepo = new FakeUserRepository(); // no user — would fail fallback check
        var behavior = new FamilyMembershipBehavior<IgnoredCommand, string>(accessor, userRepo);
        var command = new IgnoredCommand("test");

        var result = await behavior.Handle(command, SuccessHandlerIgnored, CancellationToken.None);

        result.Should().Be("handler-executed");
    }

    // --- Unauthenticated / no HttpContext tests ---

    [Fact]
    public async Task Should_skip_check_when_unauthenticated()
    {
        var accessor = CreateUnauthenticatedAccessor();
        var userRepo = new FakeUserRepository();
        var behavior = new FamilyMembershipBehavior<RegularCommand, string>(accessor, userRepo);
        var command = new RegularCommand("test");

        var result = await behavior.Handle(command, SuccessHandler, CancellationToken.None);

        result.Should().Be("handler-executed");
    }

    [Fact]
    public async Task Should_skip_check_when_no_httpcontext()
    {
        var accessor = CreateNullHttpContextAccessor();
        var userRepo = new FakeUserRepository();
        var behavior = new FamilyMembershipBehavior<RegularCommand, string>(accessor, userRepo);
        var command = new RegularCommand("test");

        var result = await behavior.Handle(command, SuccessHandler, CancellationToken.None);

        result.Should().Be("handler-executed");
    }

    [Fact]
    public async Task Should_skip_check_when_no_sub_claim()
    {
        var context = new DefaultHttpContext();
        var identity = new ClaimsIdentity([], authenticationType: "Bearer"); // no "sub" claim
        context.User = new ClaimsPrincipal(identity);
        var accessor = new HttpContextAccessor { HttpContext = context };
        var userRepo = new FakeUserRepository();
        var behavior = new FamilyMembershipBehavior<RegularCommand, string>(accessor, userRepo);
        var command = new RegularCommand("test");

        var result = await behavior.Handle(command, SuccessHandler, CancellationToken.None);

        result.Should().Be("handler-executed");
    }

    [Fact]
    public async Task Should_skip_check_when_user_not_found_in_db()
    {
        var accessor = CreateAuthenticatedAccessor(TestExternalUserId.Value);
        var userRepo = new FakeUserRepository(null); // user not found
        var behavior = new FamilyMembershipBehavior<RegularCommand, string>(accessor, userRepo);
        var command = new RegularCommand("test");

        var result = await behavior.Handle(command, SuccessHandler, CancellationToken.None);

        result.Should().Be("handler-executed");
    }

    // --- IFamilyScoped tests ---

    [Fact]
    public async Task Should_allow_family_scoped_command_when_user_belongs_to_that_family()
    {
        var user = CreateUser(TestFamilyId);
        var accessor = CreateAuthenticatedAccessor(TestExternalUserId.Value);
        var userRepo = new FakeUserRepository(user);
        var behavior = new FamilyMembershipBehavior<FamilyScopedCommand, string>(accessor, userRepo);
        var command = new FamilyScopedCommand("test", TestFamilyId);

        var result = await behavior.Handle(command, SuccessHandlerScoped, CancellationToken.None);

        result.Should().Be("handler-executed");
    }

    [Fact]
    public async Task Should_throw_when_family_scoped_and_user_belongs_to_different_family()
    {
        var user = CreateUser(FamilyId.New()); // user is in a DIFFERENT family
        var accessor = CreateAuthenticatedAccessor(TestExternalUserId.Value);
        var userRepo = new FakeUserRepository(user);
        var behavior = new FamilyMembershipBehavior<FamilyScopedCommand, string>(accessor, userRepo);
        var command = new FamilyScopedCommand("test", TestFamilyId);

        var act = async () => await behavior.Handle(command, SuccessHandlerScoped, CancellationToken.None);

        await act.Should().ThrowAsync<FamilyMembershipRequiredException>();
    }

    [Fact]
    public async Task Should_throw_when_family_scoped_and_user_has_no_family()
    {
        var user = CreateUser(familyId: null); // user has no family
        var accessor = CreateAuthenticatedAccessor(TestExternalUserId.Value);
        var userRepo = new FakeUserRepository(user);
        var behavior = new FamilyMembershipBehavior<FamilyScopedCommand, string>(accessor, userRepo);
        var command = new FamilyScopedCommand("test", TestFamilyId);

        var act = async () => await behavior.Handle(command, SuccessHandlerScoped, CancellationToken.None);

        await act.Should().ThrowAsync<FamilyMembershipRequiredException>();
    }

    // --- Fallback (untagged command — must throw InvalidOperationException) ---

    [Fact]
    public async Task Should_throw_InvalidOperationException_when_command_has_no_interface()
    {
        var user = CreateUser(FamilyId.New());
        var accessor = CreateAuthenticatedAccessor(TestExternalUserId.Value);
        var userRepo = new FakeUserRepository(user);
        var behavior = new FamilyMembershipBehavior<RegularCommand, string>(accessor, userRepo);
        var command = new RegularCommand("test");

        var act = async () => await behavior.Handle(command, SuccessHandler, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*must implement IFamilyScoped or IIgnoreFamilyMembership*");
    }

    [Fact]
    public async Task Should_throw_InvalidOperationException_even_when_user_has_no_family()
    {
        var user = CreateUser(familyId: null);
        var accessor = CreateAuthenticatedAccessor(TestExternalUserId.Value);
        var userRepo = new FakeUserRepository(user);
        var behavior = new FamilyMembershipBehavior<RegularCommand, string>(accessor, userRepo);
        var command = new RegularCommand("test");

        var act = async () => await behavior.Handle(command, SuccessHandler, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*must implement IFamilyScoped or IIgnoreFamilyMembership*");
    }

    // --- Error code verification ---

    [Fact]
    public async Task Should_throw_with_correct_error_code_for_family_scoped()
    {
        var user = CreateUser(familyId: null);
        var accessor = CreateAuthenticatedAccessor(TestExternalUserId.Value);
        var userRepo = new FakeUserRepository(user);
        var behavior = new FamilyMembershipBehavior<FamilyScopedCommand, string>(accessor, userRepo);
        var command = new FamilyScopedCommand("test", TestFamilyId);

        var act = async () => await behavior.Handle(command, SuccessHandlerScoped, CancellationToken.None);

        var exception = await act.Should().ThrowAsync<FamilyMembershipRequiredException>();
        exception.Which.ErrorCode.Should().Be("FAMILY_MEMBERSHIP_REQUIRED");
    }
}
