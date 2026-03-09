using FluentAssertions;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.GoogleIntegration.Application.Queries.GetCalendarSyncStatus;
using FamilyHub.Api.Features.GoogleIntegration.Domain.Entities;
using FamilyHub.Api.Features.GoogleIntegration.Domain.Repositories;
using FamilyHub.Api.Features.GoogleIntegration.Domain.ValueObjects;
using NSubstitute;

namespace FamilyHub.GoogleIntegration.Tests.Application;

public class GetCalendarSyncStatusQueryHandlerTests
{
    [Fact]
    public async Task Handle_WithLinkedCalendarAccount_ShouldReturnLinkedStatus()
    {
        var userId = UserId.New();
        var link = GoogleAccountLink.Create(
            userId,
            GoogleAccountId.From("sub"),
            Email.From("test@gmail.com"),
            EncryptedToken.From("enc"), EncryptedToken.From("enc"),
            DateTime.UtcNow.AddHours(1),
            GoogleScopes.From("openid https://www.googleapis.com/auth/calendar.readonly"));

        var repo = Substitute.For<IGoogleAccountLinkRepository>();
        repo.GetByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(link);

        var handler = new GetCalendarSyncStatusQueryHandler(repo);

        var result = await handler.Handle(
            new GetCalendarSyncStatusQuery(userId), CancellationToken.None);

        result.IsLinked.Should().BeTrue();
        result.HasCalendarScope.Should().BeTrue();
        result.Status.Should().Be("Active");
    }

    [Fact]
    public async Task Handle_WithNoLinkedAccount_ShouldReturnNotLinked()
    {
        var userId = UserId.New();
        var repo = Substitute.For<IGoogleAccountLinkRepository>();
        repo.GetByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns((GoogleAccountLink?)null);

        var handler = new GetCalendarSyncStatusQueryHandler(repo);

        var result = await handler.Handle(
            new GetCalendarSyncStatusQuery(userId), CancellationToken.None);

        result.IsLinked.Should().BeFalse();
        result.Status.Should().Be("NotLinked");
    }
}
