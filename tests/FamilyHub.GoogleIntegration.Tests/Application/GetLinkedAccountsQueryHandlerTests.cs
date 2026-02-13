using FluentAssertions;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.GoogleIntegration.Application.Queries.GetLinkedAccounts;
using FamilyHub.Api.Features.GoogleIntegration.Domain.Entities;
using FamilyHub.Api.Features.GoogleIntegration.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;

namespace FamilyHub.GoogleIntegration.Tests.Application;

public class GetLinkedAccountsQueryHandlerTests
{
    [Fact]
    public async Task Handle_WithLinkedAccount_ShouldReturnDto()
    {
        var userId = UserId.New();
        var link = GoogleAccountLink.Create(
            userId,
            GoogleAccountId.From("google-sub"),
            Email.From("test@gmail.com"),
            EncryptedToken.From("enc-access"),
            EncryptedToken.From("enc-refresh"),
            DateTime.UtcNow.AddHours(1),
            GoogleScopes.From("openid email"));

        var repo = new FakeGoogleAccountLinkRepository(link);
        var handler = new GetLinkedAccountsQueryHandler(repo);

        var query = new GetLinkedAccountsQuery(userId);
        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(1);
        result.First().GoogleEmail.Should().Be("test@gmail.com");
        result.First().GoogleAccountId.Should().Be("google-sub");
        result.First().Status.Should().Be("Active");
    }

    [Fact]
    public async Task Handle_WithNoLinkedAccount_ShouldReturnEmpty()
    {
        var repo = new FakeGoogleAccountLinkRepository();
        var handler = new GetLinkedAccountsQueryHandler(repo);

        var query = new GetLinkedAccountsQuery(UserId.New());
        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().BeEmpty();
    }
}
