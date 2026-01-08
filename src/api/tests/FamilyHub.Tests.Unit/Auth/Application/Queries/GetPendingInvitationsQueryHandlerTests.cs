using FamilyHub.Modules.Family.Domain.ValueObjects;
using FamilyHub.Modules.Auth.Application.Abstractions;
using FamilyHub.Modules.Auth.Application.Queries.GetPendingInvitations;
using FamilyHub.Modules.Family.Domain.Repositories;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Tests.Unit.Auth.Application.Queries;

/// <summary>
/// Unit tests for GetPendingInvitationsQueryHandler.
/// Tests query handling logic, repository interactions, and domain to DTO mapping.
/// Uses NSubstitute for mocking with AutoFixture attribute-based dependency injection.
/// Uses FluentAssertions for readable, expressive test assertions.
/// </summary>
public class GetPendingInvitationsQueryHandlerTests
{
    #region Happy Path Tests

    [Theory, AutoNSubstituteData]
    public async Task Handle_ShouldReturnPendingInvitations_WhenInvitationsExist(
        [Frozen] IUserContext userContext,
        [Frozen] IFamilyMemberInvitationRepository repository,
        GetPendingInvitationsQueryHandler sut)
    {
        // Arrange
        var familyId = FamilyId.New();
        var email1 = Email.From("test1@example.com");
        var email2 = Email.From("test2@example.com");
        var invitedByUserId = UserId.New();

        var invitation1 = Modules.Family.Domain.FamilyMemberInvitation.CreateEmailInvitation(
            familyId,
            email1,
            FamilyRole.Member,
            invitedByUserId);

        var invitation2 = Modules.Family.Domain.FamilyMemberInvitation.CreateEmailInvitation(
            familyId,
            email2,
            FamilyRole.Admin,
            invitedByUserId,
            "Welcome to the family!");

        userContext.FamilyId.Returns(familyId);

        repository.GetPendingByFamilyIdAsync(familyId, Arg.Any<CancellationToken>())
            .Returns([invitation1, invitation2]);

        var query = new GetPendingInvitationsQuery();

        // Act
        var result = await sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Invitations.Should().HaveCount(2);

        // Verify first invitation mapping
        result.Invitations[0].Email.Should().Be("test1@example.com");
        result.Invitations[0].Role.Should().Be(FamilyRole.Member);
        result.Invitations[0].InvitedByUserId.Should().Be(invitedByUserId.Value);
        result.Invitations[0].Message.Should().BeNull();

        // Verify second invitation mapping
        result.Invitations[1].Email.Should().Be("test2@example.com");
        result.Invitations[1].Role.Should().Be(FamilyRole.Admin);
        result.Invitations[1].Message.Should().Be("Welcome to the family!");
    }

    [Theory, AutoNSubstituteData]
    public async Task Handle_ShouldReturnEmptyList_WhenNoInvitations(
        [Frozen] IUserContext userContext,
        [Frozen] IFamilyMemberInvitationRepository repository,
        GetPendingInvitationsQueryHandler sut)
    {
        // Arrange
        var familyId = FamilyId.New();

        userContext.FamilyId.Returns(familyId);

        repository.GetPendingByFamilyIdAsync(familyId, Arg.Any<CancellationToken>())
            .Returns([]);

        var query = new GetPendingInvitationsQuery();

        // Act
        var result = await sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Invitations.Should().BeEmpty();
    }

    [Theory, AutoNSubstituteData]
    public async Task Handle_ShouldMapDomainEntities_ToApplicationDTOs(
        [Frozen] IUserContext userContext,
        [Frozen] IFamilyMemberInvitationRepository repository,
        GetPendingInvitationsQueryHandler sut)
    {
        // Arrange
        var familyId = FamilyId.New();
        var email = Email.From("test@example.com");
        var invitedByUserId = UserId.New();

        var invitation = Modules.Family.Domain.FamilyMemberInvitation.CreateEmailInvitation(
            familyId,
            email,
            FamilyRole.Member,
            invitedByUserId,
            "Test message");

        userContext.FamilyId.Returns(familyId);

        repository.GetPendingByFamilyIdAsync(familyId, Arg.Any<CancellationToken>())
            .Returns([invitation]);

        var query = new GetPendingInvitationsQuery();

        // Act
        var result = await sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Invitations.Should().HaveCount(1);

        var dto = result.Invitations[0];
        dto.Id.Should().Be(invitation.Id.Value);
        dto.Email.Should().Be(email.Value);
        dto.Role.Should().Be(FamilyRole.Member);
        dto.Status.Should().Be(InvitationStatus.Pending);
        dto.InvitedByUserId.Should().Be(invitedByUserId.Value);
        dto.InvitedAt.Should().Be(invitation.CreatedAt);
        dto.ExpiresAt.Should().Be(invitation.ExpiresAt);
        dto.Message.Should().Be("Test message");
        dto.DisplayCode.Should().Be(invitation.DisplayCode.Value);
    }

    #endregion

    #region Repository Interaction Tests

    [Theory, AutoNSubstituteData]
    public async Task Handle_ShouldCallRepository_WithCorrectFamilyId(
        [Frozen] IUserContext userContext,
        [Frozen] IFamilyMemberInvitationRepository repository,
        GetPendingInvitationsQueryHandler sut)
    {
        // Arrange
        var familyId = FamilyId.New();

        userContext.FamilyId.Returns(familyId);

        repository.GetPendingByFamilyIdAsync(familyId, Arg.Any<CancellationToken>())
            .Returns([]);

        var query = new GetPendingInvitationsQuery();

        // Act
        await sut.Handle(query, CancellationToken.None);

        // Assert
        await repository.Received(1).GetPendingByFamilyIdAsync(
            familyId,
            Arg.Any<CancellationToken>());
    }

    [Theory, AutoNSubstituteData]
    public async Task Handle_WithCancellationToken_ShouldPassTokenToRepository(
        [Frozen] IUserContext userContext,
        [Frozen] IFamilyMemberInvitationRepository repository,
        GetPendingInvitationsQueryHandler sut)
    {
        // Arrange
        var familyId = FamilyId.New();
        var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;

        userContext.FamilyId.Returns(familyId);

        repository.GetPendingByFamilyIdAsync(familyId, cancellationToken)
            .Returns([]);

        var query = new GetPendingInvitationsQuery();

        // Act
        await sut.Handle(query, cancellationToken);

        // Assert
        await repository.Received(1).GetPendingByFamilyIdAsync(familyId, cancellationToken);
    }

    #endregion
}
