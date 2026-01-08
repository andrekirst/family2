using FamilyDomain = FamilyHub.Modules.Family.Domain;
using FamilyHub.Modules.Family.Domain.ValueObjects;
using FamilyHub.Modules.Auth.Application.Queries.GetInvitationByToken;
using FamilyHub.Modules.Auth.Domain;
using FamilyHub.Modules.Family.Domain.Repositories;
using FamilyHub.Modules.Auth.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace FamilyHub.Tests.Unit.Auth.Application.Queries;

/// <summary>
/// Unit tests for GetInvitationByTokenQueryHandler.
/// Tests query handling logic, repository interactions, business rules, and domain to DTO mapping.
/// Uses NSubstitute for mocking with AutoFixture attribute-based dependency injection.
/// Uses FluentAssertions for readable, expressive test assertions.
/// </summary>
public class GetInvitationByTokenQueryHandlerTests
{
    #region Happy Path Tests

    [Theory, AutoNSubstituteData]
    public async Task Handle_ShouldReturnInvitation_WhenTokenValid(
        IFamilyMemberInvitationRepository repository)
    {
        // Arrange
        var familyId = FamilyId.New();
        var email = Email.From("test@example.com");
        var invitedByUserId = UserId.New();
        var invitation = FamilyDomain.FamilyMemberInvitation.CreateEmailInvitation(
            familyId,
            email,
            FamilyRole.Member,
            invitedByUserId,
            "Welcome!");

        repository.GetByTokenAsync(invitation.Token, Arg.Any<CancellationToken>())
            .Returns(invitation);

        var query = new GetInvitationByTokenQuery(invitation.Token);
        var handler = new GetInvitationByTokenQueryHandler(repository);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(invitation.Id.Value);
        result.Email.Should().Be(email.Value);
        result.Role.Should().Be(FamilyRole.Member);
        result.Status.Should().Be(InvitationStatus.Pending);
        result.InvitedByUserId.Should().Be(invitedByUserId.Value);
        result.Message.Should().Be("Welcome!");
        result.DisplayCode.Should().Be(invitation.DisplayCode.Value);
    }

    [Theory, AutoNSubstituteData]
    public async Task Handle_ShouldReturnNull_WhenTokenNotFound(
        IFamilyMemberInvitationRepository repository)
    {
        // Arrange
        var token = InvitationToken.Generate();

        repository.GetByTokenAsync(token, Arg.Any<CancellationToken>())
            .Returns((FamilyDomain.FamilyMemberInvitation?)null);

        var query = new GetInvitationByTokenQuery(token);
        var handler = new GetInvitationByTokenQueryHandler(repository);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    // NOTE: Status validation (pending check) is now handled by GetInvitationByTokenQueryValidator,
    // not in the handler. The handler trusts that validation has already occurred.
    // See GetInvitationByTokenQueryValidatorTests.Validate_WhenInvitationNotPending_ShouldFail for status validation tests.

    [Theory, AutoNSubstituteData]
    public async Task Handle_ShouldMapDomainEntity_ToApplicationDTO(
        IFamilyMemberInvitationRepository repository)
    {
        // Arrange
        var familyId = FamilyId.New();
        var email = Email.From("test@example.com");
        var invitedByUserId = UserId.New();
        var invitation = FamilyDomain.FamilyMemberInvitation.CreateEmailInvitation(
            familyId,
            email,
            FamilyRole.Admin,
            invitedByUserId,
            "Important message");

        repository.GetByTokenAsync(invitation.Token, Arg.Any<CancellationToken>())
            .Returns(invitation);

        var query = new GetInvitationByTokenQuery(invitation.Token);
        var handler = new GetInvitationByTokenQueryHandler(repository);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(invitation.Id.Value);
        result.Email.Should().Be(email.Value);
        result.Role.Should().Be(FamilyRole.Admin);
        result.Status.Should().Be(InvitationStatus.Pending);
        result.InvitedByUserId.Should().Be(invitedByUserId.Value);
        result.InvitedAt.Should().Be(invitation.CreatedAt);
        result.ExpiresAt.Should().Be(invitation.ExpiresAt);
        result.Message.Should().Be("Important message");
        result.DisplayCode.Should().Be(invitation.DisplayCode.Value);
    }

    #endregion

    #region Repository Interaction Tests

    [Theory, AutoNSubstituteData]
    public async Task Handle_ShouldCallRepository_WithCorrectToken(
        IFamilyMemberInvitationRepository repository)
    {
        // Arrange
        var token = InvitationToken.Generate();

        repository.GetByTokenAsync(token, Arg.Any<CancellationToken>())
            .Returns((FamilyDomain.FamilyMemberInvitation?)null);

        var query = new GetInvitationByTokenQuery(token);
        var handler = new GetInvitationByTokenQueryHandler(repository);

        // Act
        await handler.Handle(query, CancellationToken.None);

        // Assert
        await repository.Received(1).GetByTokenAsync(
            token,
            Arg.Any<CancellationToken>());
    }

    [Theory, AutoNSubstituteData]
    public async Task Handle_WithCancellationToken_ShouldPassTokenToRepository(
        IFamilyMemberInvitationRepository repository)
    {
        // Arrange
        var token = InvitationToken.Generate();
        var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;

        repository.GetByTokenAsync(token, cancellationToken)
            .Returns((FamilyDomain.FamilyMemberInvitation?)null);

        var query = new GetInvitationByTokenQuery(token);
        var handler = new GetInvitationByTokenQueryHandler(repository);

        // Act
        await handler.Handle(query, cancellationToken);

        // Assert
        await repository.Received(1).GetByTokenAsync(token, cancellationToken);
    }

    #endregion
}
