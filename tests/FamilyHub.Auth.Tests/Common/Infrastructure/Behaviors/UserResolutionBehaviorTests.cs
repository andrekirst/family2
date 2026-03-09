using FamilyHub.Api.Common.Infrastructure.Behaviors;
using FamilyHub.Api.Common.Infrastructure.FamilyScope;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;
using Mediator;

namespace FamilyHub.Auth.Tests.Common.Infrastructure.Behaviors;

public class UserResolutionBehaviorTests
{
    #region Test message types

    private record AnonymousCommand(string Value) : FamilyHub.Common.Application.ICommand<string>, IAnonymousOperation;

    private record UserCommand : FamilyHub.Common.Application.ICommand<string>, IRequireUser
    {
        public UserId UserId { get; init; }
    }

    private record FamilyCommand : FamilyHub.Common.Application.ICommand<string>, IRequireFamily
    {
        public UserId UserId { get; init; }
        public FamilyId FamilyId { get; init; }
    }

    private record UnmarkedCommand(string Value) : FamilyHub.Common.Application.ICommand<string>;

    #endregion

    #region Fake ICurrentUserContext

    private sealed class FakeCurrentUserContext : ICurrentUserContext
    {
        public bool IsAuthenticated { get; set; }
        public CurrentUserInfo? UserInfo { get; set; }
        public RawClaimsInfo? RawClaims { get; set; }

        public Task<CurrentUserInfo> GetCurrentUserAsync()
        {
            if (UserInfo is null)
                throw new UnauthorizedAccessException("No user info configured");
            return Task.FromResult(UserInfo);
        }

        public RawClaimsInfo GetRawClaims()
        {
            if (RawClaims is null)
                throw new UnauthorizedAccessException("No raw claims configured");
            return RawClaims;
        }
    }

    #endregion

    private static ValueTask<string> SuccessHandler<TMessage>(TMessage _, CancellationToken __) =>
        new("handler-executed");

    [Fact]
    public async Task Should_skip_anonymous_operations()
    {
        var userContext = new FakeCurrentUserContext { IsAuthenticated = true };
        var behavior = new UserResolutionBehavior<AnonymousCommand, string>(userContext);
        var command = new AnonymousCommand("test");

        var result = await behavior.Handle(
            command,
            SuccessHandler,
            CancellationToken.None);

        result.Should().Be("handler-executed");
    }

    [Fact]
    public async Task Should_skip_unauthenticated_requests()
    {
        var userContext = new FakeCurrentUserContext { IsAuthenticated = false };
        var behavior = new UserResolutionBehavior<UserCommand, string>(userContext);
        var command = new UserCommand();

        var result = await behavior.Handle(
            command,
            SuccessHandler,
            CancellationToken.None);

        result.Should().Be("handler-executed");
    }

    [Fact]
    public async Task Should_populate_UserId_for_IRequireUser_message()
    {
        var expectedUserId = UserId.New();
        var userContext = new FakeCurrentUserContext
        {
            IsAuthenticated = true,
            UserInfo = new CurrentUserInfo(
                expectedUserId,
                ExternalUserId.From("ext-123"),
                FamilyId: null),
        };
        var behavior = new UserResolutionBehavior<UserCommand, string>(userContext);
        var command = new UserCommand();

        UserCommand? capturedCommand = null;
        ValueTask<string> CapturingHandler(UserCommand cmd, CancellationToken _)
        {
            capturedCommand = cmd;
            return new("ok");
        }

        await behavior.Handle(command, CapturingHandler, CancellationToken.None);

        capturedCommand.Should().NotBeNull();
        // UserResolutionBehavior sets the property via reflection on the original object
        command.UserId.Should().Be(expectedUserId);
    }

    [Fact]
    public async Task Should_populate_UserId_and_FamilyId_for_IRequireFamily_message()
    {
        var expectedUserId = UserId.New();
        var expectedFamilyId = FamilyId.New();
        var userContext = new FakeCurrentUserContext
        {
            IsAuthenticated = true,
            UserInfo = new CurrentUserInfo(
                expectedUserId,
                ExternalUserId.From("ext-456"),
                expectedFamilyId),
        };
        var behavior = new UserResolutionBehavior<FamilyCommand, string>(userContext);
        var command = new FamilyCommand();

        await behavior.Handle(command, SuccessHandler, CancellationToken.None);

        command.UserId.Should().Be(expectedUserId);
        command.FamilyId.Should().Be(expectedFamilyId);
    }

    [Fact]
    public async Task Should_throw_FamilyMembershipRequiredException_when_user_has_no_family()
    {
        var userContext = new FakeCurrentUserContext
        {
            IsAuthenticated = true,
            UserInfo = new CurrentUserInfo(
                UserId.New(),
                ExternalUserId.From("ext-789"),
                FamilyId: null),
        };
        var behavior = new UserResolutionBehavior<FamilyCommand, string>(userContext);
        var command = new FamilyCommand();

        var act = async () => await behavior.Handle(
            command,
            SuccessHandler,
            CancellationToken.None);

        await act.Should().ThrowAsync<FamilyMembershipRequiredException>();
    }

    [Fact]
    public async Task Should_throw_InvalidOperationException_for_unmarked_command()
    {
        var userContext = new FakeCurrentUserContext
        {
            IsAuthenticated = true,
            UserInfo = new CurrentUserInfo(
                UserId.New(),
                ExternalUserId.From("ext-000"),
                FamilyId: null),
        };
        var behavior = new UserResolutionBehavior<UnmarkedCommand, string>(userContext);
        var command = new UnmarkedCommand("test");

        var act = async () => await behavior.Handle(
            command,
            SuccessHandler,
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*must implement*IAnonymousOperation*");
    }
}
