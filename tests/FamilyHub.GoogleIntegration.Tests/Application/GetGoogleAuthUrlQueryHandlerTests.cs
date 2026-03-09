using FluentAssertions;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.GoogleIntegration.Application.Queries.GetGoogleAuthUrl;
using FamilyHub.Api.Features.GoogleIntegration.Domain.Entities;
using FamilyHub.Api.Features.GoogleIntegration.Domain.Repositories;
using FamilyHub.Api.Features.GoogleIntegration.Infrastructure.Services;
using NSubstitute;

namespace FamilyHub.GoogleIntegration.Tests.Application;

public class GetGoogleAuthUrlQueryHandlerTests
{
    private const string TestConsentUrl = "https://accounts.google.com/o/oauth2/v2/auth?test=true";
    private const string TestState = "test-state";
    private const string TestCodeVerifier = "test-code-verifier";

    private static (GetGoogleAuthUrlQueryHandler Handler,
        IGoogleOAuthService OAuthService,
        IOAuthStateRepository StateRepo,
        IUnitOfWork UnitOfWork) CreateHandler()
    {
        var oauthService = Substitute.For<IGoogleOAuthService>();
        oauthService.BuildConsentUrl()
            .Returns((TestConsentUrl, TestState, TestCodeVerifier));

        var stateRepo = Substitute.For<IOAuthStateRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        var handler = new GetGoogleAuthUrlQueryHandler(oauthService, stateRepo, unitOfWork);
        return (handler, oauthService, stateRepo, unitOfWork);
    }

    [Fact]
    public async Task Handle_ShouldReturnAuthUrl()
    {
        var (handler, _, _, _) = CreateHandler();

        var query = new GetGoogleAuthUrlQuery(UserId.New());
        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().Be(TestConsentUrl);
    }

    [Fact]
    public async Task Handle_ShouldPersistOAuthState()
    {
        var userId = UserId.New();
        var (handler, _, stateRepo, _) = CreateHandler();

        var query = new GetGoogleAuthUrlQuery(userId);
        await handler.Handle(query, CancellationToken.None);

        await stateRepo.Received(1).AddAsync(
            Arg.Is<OAuthState>(s =>
                s.UserId == userId &&
                s.State == TestState &&
                s.CodeVerifier == TestCodeVerifier),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldCallSaveChanges()
    {
        var (handler, _, _, unitOfWork) = CreateHandler();

        var query = new GetGoogleAuthUrlQuery(UserId.New());
        await handler.Handle(query, CancellationToken.None);

        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
