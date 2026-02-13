using FluentAssertions;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.GoogleIntegration.Application.Queries.GetCalendarSyncStatus;
using FamilyHub.Api.Features.GoogleIntegration.Domain.Entities;
using FamilyHub.Api.Features.GoogleIntegration.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;

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

        var repo = new FakeGoogleAccountLinkRepository(link);
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
        var repo = new FakeGoogleAccountLinkRepository();
        var handler = new GetCalendarSyncStatusQueryHandler(repo);

        var result = await handler.Handle(
            new GetCalendarSyncStatusQuery(UserId.New()), CancellationToken.None);

        result.IsLinked.Should().BeFalse();
        result.Status.Should().Be("NotLinked");
    }
}
