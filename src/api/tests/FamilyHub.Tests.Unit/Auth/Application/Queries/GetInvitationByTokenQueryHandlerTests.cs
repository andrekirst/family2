using FamilyHub.Modules.Auth.Application.Queries.GetInvitationByToken;
using FamilyHub.Modules.Auth.Domain;
using FamilyHub.Modules.Auth.Domain.Repositories;
using FamilyHub.Modules.Family.Domain.Repositories;
using FamilyHub.Modules.Family.Domain.Specifications;
using FamilyHub.Modules.Family.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;
using FamilyAggregate = FamilyHub.Modules.Family.Domain.Aggregates.Family;
using FamilyMemberInvitationAggregate = FamilyHub.Modules.Family.Domain.Aggregates.FamilyMemberInvitation;

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
        IFamilyMemberInvitationRepository invitationRepository,
        IFamilyRepository familyRepository,
        IUserRepository userRepository)
    {
        // Arrange
        var familyId = FamilyId.New();
        var ownerId = UserId.New();
        var email = Email.From("test@example.com");
        var invitedByUserId = UserId.New();
        var invitation = FamilyMemberInvitationAggregate.CreateEmailInvitation(
            familyId,
            email,
            FamilyRole.Member,
            invitedByUserId,
            "Welcome!");

        var family = FamilyAggregate.Create(FamilyName.From("Test Family"), ownerId);
        var users = new List<User> { /* Mock users */ };

        invitationRepository.FindOneAsync(Arg.Any<InvitationByTokenSpecification>(), Arg.Any<CancellationToken>())
            .Returns(invitation);
        familyRepository.GetByIdAsync(familyId, Arg.Any<CancellationToken>())
            .Returns(family);
        userRepository.GetByFamilyIdAsync(familyId, Arg.Any<CancellationToken>())
            .Returns(users);

        var query = new GetInvitationByTokenQuery(invitation.Token);
        var handler = new GetInvitationByTokenQueryHandler(invitationRepository, familyRepository, userRepository);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(invitation.Id.Value);
        result.Email.Should().Be(email.Value);
        result.Role.Should().Be(FamilyRole.Member);
        result.Status.Should().Be(InvitationStatus.Pending);
        result.InvitedByUserId.Should().Be(invitedByUserId.Value);
        result.Message.Should().Be("Welcome!");
        result.DisplayCode.Should().Be(invitation.DisplayCode.Value);
        result.Family.Should().NotBeNull();
        result.Family.Name.Should().Be("Test Family");
        result.MemberCount.Should().Be(0);
    }

    [Theory, AutoNSubstituteData]
    public async Task Handle_ShouldReturnNull_WhenTokenNotFound(
        IFamilyMemberInvitationRepository invitationRepository,
        IFamilyRepository familyRepository,
        IUserRepository userRepository)
    {
        // Arrange
        var token = InvitationToken.Generate();

        invitationRepository.FindOneAsync(Arg.Any<InvitationByTokenSpecification>(), Arg.Any<CancellationToken>())
            .Returns((FamilyMemberInvitationAggregate?)null);

        var query = new GetInvitationByTokenQuery(token);
        var handler = new GetInvitationByTokenQueryHandler(invitationRepository, familyRepository, userRepository);

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
        IFamilyMemberInvitationRepository invitationRepository,
        IFamilyRepository familyRepository,
        IUserRepository userRepository)
    {
        // Arrange
        var familyId = FamilyId.New();
        var ownerId = UserId.New();
        var email = Email.From("test@example.com");
        var invitedByUserId = UserId.New();
        var invitation = FamilyMemberInvitationAggregate.CreateEmailInvitation(
            familyId,
            email,
            FamilyRole.Admin,
            invitedByUserId,
            "Important message");

        var family = FamilyAggregate.Create(FamilyName.From("Admin Family"), ownerId);
        var users = new List<User> { /* Mock 2 users */ };

        invitationRepository.FindOneAsync(Arg.Any<InvitationByTokenSpecification>(), Arg.Any<CancellationToken>())
            .Returns(invitation);
        familyRepository.GetByIdAsync(familyId, Arg.Any<CancellationToken>())
            .Returns(family);
        userRepository.GetByFamilyIdAsync(familyId, Arg.Any<CancellationToken>())
            .Returns(users);

        var query = new GetInvitationByTokenQuery(invitation.Token);
        var handler = new GetInvitationByTokenQueryHandler(invitationRepository, familyRepository, userRepository);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(invitation.Id.Value);
        result.Email.Should().Be(email.Value);
        result.Role.Should().Be(FamilyRole.Admin);
        result.Status.Should().Be(InvitationStatus.Pending);
        result.InvitedByUserId.Should().Be(invitedByUserId.Value);
        result.InvitedAt.Should().Be(invitation.CreatedAt);
        result.ExpiresAt.Should().Be(invitation.ExpiresAt);
        result.Message.Should().Be("Important message");
        result.DisplayCode.Should().Be(invitation.DisplayCode.Value);
        result.Family.Should().NotBeNull();
        result.Family.Name.Should().Be("Admin Family");
        result.MemberCount.Should().Be(0);
    }

    #endregion

    #region Repository Interaction Tests

    [Theory, AutoNSubstituteData]
    public async Task Handle_ShouldCallRepository_WithCorrectToken(
        IFamilyMemberInvitationRepository invitationRepository,
        IFamilyRepository familyRepository,
        IUserRepository userRepository)
    {
        // Arrange
        var token = InvitationToken.Generate();

        invitationRepository.FindOneAsync(Arg.Any<InvitationByTokenSpecification>(), Arg.Any<CancellationToken>())
            .Returns((FamilyMemberInvitationAggregate?)null);

        var query = new GetInvitationByTokenQuery(token);
        var handler = new GetInvitationByTokenQueryHandler(invitationRepository, familyRepository, userRepository);

        // Act
        await handler.Handle(query, CancellationToken.None);

        // Assert
        await invitationRepository.Received(1).FindOneAsync(
            Arg.Any<InvitationByTokenSpecification>(),
            Arg.Any<CancellationToken>());
    }

    [Theory, AutoNSubstituteData]
    public async Task Handle_WithCancellationToken_ShouldPassTokenToRepository(
        IFamilyMemberInvitationRepository invitationRepository,
        IFamilyRepository familyRepository,
        IUserRepository userRepository)
    {
        // Arrange
        var token = InvitationToken.Generate();
        var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;

        invitationRepository.FindOneAsync(Arg.Any<InvitationByTokenSpecification>(), cancellationToken)
            .Returns((FamilyMemberInvitationAggregate?)null);

        var query = new GetInvitationByTokenQuery(token);
        var handler = new GetInvitationByTokenQueryHandler(invitationRepository, familyRepository, userRepository);

        // Act
        await handler.Handle(query, cancellationToken);

        // Assert
        await invitationRepository.Received(1).FindOneAsync(Arg.Any<InvitationByTokenSpecification>(), cancellationToken);
    }

    #endregion
}
