using FluentAssertions;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.GoogleIntegration.Application.Queries.GetGoogleAuthUrl;
using FamilyHub.TestCommon.Fakes;

namespace FamilyHub.GoogleIntegration.Tests.Application;

public class GetGoogleAuthUrlQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnAuthUrl()
    {
        var oauthService = new FakeGoogleOAuthService();
        var stateRepo = new FakeOAuthStateRepository();
        var handler = new GetGoogleAuthUrlQueryHandler(oauthService, stateRepo);

        var query = new GetGoogleAuthUrlQuery(UserId.New());
        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().Be(oauthService.ConsentUrl);
    }

    [Fact]
    public async Task Handle_ShouldPersistOAuthState()
    {
        var userId = UserId.New();
        var oauthService = new FakeGoogleOAuthService();
        var stateRepo = new FakeOAuthStateRepository();
        var handler = new GetGoogleAuthUrlQueryHandler(oauthService, stateRepo);

        var query = new GetGoogleAuthUrlQuery(userId);
        await handler.Handle(query, CancellationToken.None);

        stateRepo.AddedStates.Should().HaveCount(1);
        stateRepo.AddedStates[0].UserId.Should().Be(userId);
        stateRepo.AddedStates[0].State.Should().Be("test-state");
        stateRepo.AddedStates[0].CodeVerifier.Should().Be("test-code-verifier");
    }

    [Fact]
    public async Task Handle_ShouldCallSaveChanges()
    {
        var oauthService = new FakeGoogleOAuthService();
        var stateRepo = new FakeOAuthStateRepository();
        var handler = new GetGoogleAuthUrlQueryHandler(oauthService, stateRepo);

        var query = new GetGoogleAuthUrlQuery(UserId.New());
        await handler.Handle(query, CancellationToken.None);

        stateRepo.SaveChangesCount.Should().Be(1);
    }
}
