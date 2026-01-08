using FamilyHub.Modules.Auth.Application.Queries.GetInvitationByToken;
using FamilyHub.Modules.Auth.Domain;
using FamilyHub.Modules.Auth.Domain.Repositories;
using FamilyHub.Modules.Auth.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace FamilyHub.Tests.Unit.Auth.Application.Queries;

/// <summary>
/// Unit tests for GetInvitationByTokenQueryValidator.
/// Tests validation logic including async database checks for invitation existence and status.
/// Uses NSubstitute for mocking with AutoFixture attribute-based dependency injection.
/// Uses FluentAssertions for readable, expressive test assertions.
/// </summary>
public class GetInvitationByTokenQueryValidatorTests
{
    #region Validation Tests

    [Theory, AutoNSubstituteData]
    public async Task Validate_WithValidPendingInvitation_ShouldPass(
        IFamilyMemberInvitationRepository repository)
    {
        // Arrange
        var familyId = FamilyId.New();
        var email = Email.From("test@example.com");
        var invitedByUserId = UserId.New();
        var invitation = FamilyMemberInvitation.CreateEmailInvitation(
            familyId,
            email,
            FamilyRole.Member,
            invitedByUserId);

        repository.GetByTokenAsync(invitation.Token, Arg.Any<CancellationToken>())
            .Returns(invitation);

        var validator = new GetInvitationByTokenQueryValidator(repository);
        var query = new GetInvitationByTokenQuery(invitation.Token);

        // Act
        var result = await validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory, AutoNSubstituteData]
    public async Task Validate_WhenInvitationNotFound_ShouldFail(
        IFamilyMemberInvitationRepository repository)
    {
        // Arrange
        var token = InvitationToken.Generate();

        repository.GetByTokenAsync(token, Arg.Any<CancellationToken>())
            .Returns((FamilyMemberInvitation?)null);

        var validator = new GetInvitationByTokenQueryValidator(repository);
        var query = new GetInvitationByTokenQuery(token);

        // Act
        var result = await validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].ErrorMessage.Should().Be("Invitation not found or not pending");
        result.Errors[0].PropertyName.Should().Be("Token");
    }

    [Theory, AutoNSubstituteData]
    public async Task Validate_WhenInvitationNotPending_ShouldFail(
        IFamilyMemberInvitationRepository repository)
    {
        // Arrange
        var familyId = FamilyId.New();
        var email = Email.From("test@example.com");
        var invitedByUserId = UserId.New();
        var invitation = FamilyMemberInvitation.CreateEmailInvitation(
            familyId,
            email,
            FamilyRole.Member,
            invitedByUserId);

        // Mark invitation as accepted (not pending)
        invitation.MarkAsAccepted(UserId.New());

        repository.GetByTokenAsync(invitation.Token, Arg.Any<CancellationToken>())
            .Returns(invitation);

        var validator = new GetInvitationByTokenQueryValidator(repository);
        var query = new GetInvitationByTokenQuery(invitation.Token);

        // Act
        var result = await validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].ErrorMessage.Should().Be("Invitation not found or not pending");
        result.Errors[0].PropertyName.Should().Be("Token");
    }

    #endregion
}
