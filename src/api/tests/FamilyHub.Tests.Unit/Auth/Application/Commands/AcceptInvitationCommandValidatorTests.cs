using AutoFixture.Xunit2;
using FamilyHub.Modules.Auth.Application.Abstractions;
using FamilyHub.Modules.Auth.Application.Commands.AcceptInvitation;
using FamilyHub.Modules.Auth.Application.Services;
using FamilyHub.Modules.Auth.Domain;
using FamilyHub.Modules.Auth.Domain.ValueObjects;
using FamilyHub.Modules.Family.Application.Abstractions;
using FamilyHub.Modules.Family.Domain.Repositories;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using FamilyHub.Tests.Unit.Fixtures;
using FluentAssertions;
using NSubstitute;
using FamilyMemberInvitationAggregate = FamilyHub.Modules.Family.Domain.Aggregates.FamilyMemberInvitation;

namespace FamilyHub.Tests.Unit.Auth.Application.Commands;

/// <summary>
/// Unit tests for AcceptInvitationCommandValidator.
/// Tests all validation scenarios: token existence, status, expiration, email match, and family existence.
/// Uses IFamilyService (Anti-Corruption Layer) for cross-module family queries.
/// </summary>
public sealed class AcceptInvitationCommandValidatorTests
{
    #region Happy Path

    [Theory, AutoNSubstituteData]
    public async Task Validate_WithValidInvitation_ShouldPass(
        [Frozen] IFamilyMemberInvitationRepository invitationRepository,
        [Frozen] IFamilyService familyService,
        [Frozen] IUserContext userContext,
        [Frozen] IValidationCache validationCache)
    {
        // Arrange
        var familyId = FamilyId.New();
        var email = Email.From("test@example.com");
        var invitedByUserId = UserId.New();
        var invitation = FamilyMemberInvitationAggregate.CreateEmailInvitation(
            familyId, email, FamilyRole.Member, invitedByUserId);

        var familyDto = CreateFamilyDto(familyId, "Test Family", invitedByUserId);
        var user = User.CreateFromOAuth(email, "ext-123", "zitadel", familyId);
        var timeProvider = new FakeTimeProvider(DateTimeOffset.UtcNow);

        // Mock
        userContext.User.Returns(user);
        invitationRepository.GetByTokenAsync(invitation.Token, Arg.Any<CancellationToken>())
            .Returns(invitation);
        familyService.GetFamilyByIdAsync(familyId, Arg.Any<CancellationToken>())
            .Returns(familyDto);

        var validator = new AcceptInvitationCommandValidator(
            invitationRepository, familyService, userContext, timeProvider, validationCache);
        var command = new AcceptInvitationCommand(invitation.Token);

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory, AutoNSubstituteData]
    public async Task Validate_WithValidInvitation_ShouldCacheInvitationAndFamilyDto(
        [Frozen] IFamilyMemberInvitationRepository invitationRepository,
        [Frozen] IFamilyService familyService,
        [Frozen] IUserContext userContext,
        [Frozen] IValidationCache validationCache)
    {
        // Arrange
        var familyId = FamilyId.New();
        var email = Email.From("test@example.com");
        var invitedByUserId = UserId.New();
        var invitation = FamilyMemberInvitationAggregate.CreateEmailInvitation(
            familyId, email, FamilyRole.Member, invitedByUserId);

        var familyDto = CreateFamilyDto(familyId, "Test Family", invitedByUserId);
        var user = User.CreateFromOAuth(email, "ext-123", "zitadel", familyId);
        var timeProvider = new FakeTimeProvider(DateTimeOffset.UtcNow);

        // Mock
        userContext.User.Returns(user);
        invitationRepository.GetByTokenAsync(invitation.Token, Arg.Any<CancellationToken>())
            .Returns(invitation);
        familyService.GetFamilyByIdAsync(familyId, Arg.Any<CancellationToken>())
            .Returns(familyDto);

        var validator = new AcceptInvitationCommandValidator(
            invitationRepository, familyService, userContext, timeProvider, validationCache);
        var command = new AcceptInvitationCommand(invitation.Token);

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();

        // Verify cache was populated with FamilyDto (Anti-Corruption Layer boundary)
        validationCache.Received(1).Set(CacheKeyBuilder.FamilyMemberInvitation(invitation.Token.Value), invitation);
        validationCache.Received(1).Set(CacheKeyBuilder.Family(familyId.Value), familyDto);
    }

    #endregion

    #region Token Validation

    [Theory, AutoNSubstituteData]
    public async Task Validate_WithNonExistentToken_ShouldFailWithInvalidTokenMessage(
        [Frozen] IFamilyMemberInvitationRepository invitationRepository,
        [Frozen] IFamilyService familyService,
        [Frozen] IUserContext userContext,
        [Frozen] IValidationCache validationCache)
    {
        // Arrange
        var token = InvitationToken.Generate();
        var timeProvider = new FakeTimeProvider(DateTimeOffset.UtcNow);

        invitationRepository.GetByTokenAsync(token, Arg.Any<CancellationToken>())
            .Returns((FamilyMemberInvitationAggregate?)null);

        var validator = new AcceptInvitationCommandValidator(
            invitationRepository, familyService, userContext, timeProvider, validationCache);
        var command = new AcceptInvitationCommand(token);

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be("Invalid or expired invitation token.");
    }

    #endregion

    #region Status Validation

    [Theory, AutoNSubstituteData]
    public async Task Validate_WithAcceptedInvitation_ShouldFailWithStatusMessage(
        [Frozen] IFamilyMemberInvitationRepository invitationRepository,
        [Frozen] IFamilyService familyService,
        [Frozen] IUserContext userContext,
        [Frozen] IValidationCache validationCache)
    {
        // Arrange
        var familyId = FamilyId.New();
        var email = Email.From("test@example.com");
        var invitedByUserId = UserId.New();
        var invitation = FamilyMemberInvitationAggregate.CreateEmailInvitation(
            familyId, email, FamilyRole.Member, invitedByUserId);
        var timeProvider = new FakeTimeProvider(DateTimeOffset.UtcNow);

        // Accept the invitation to change status
        invitation.Accept(UserId.New());

        invitationRepository.GetByTokenAsync(invitation.Token, Arg.Any<CancellationToken>())
            .Returns(invitation);

        var validator = new AcceptInvitationCommandValidator(
            invitationRepository, familyService, userContext, timeProvider, validationCache);
        var command = new AcceptInvitationCommand(invitation.Token);

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Contain("Cannot accept invitation in accepted status");
    }

    [Theory, AutoNSubstituteData]
    public async Task Validate_WithCanceledInvitation_ShouldFailWithStatusMessage(
        [Frozen] IFamilyMemberInvitationRepository invitationRepository,
        [Frozen] IFamilyService familyService,
        [Frozen] IUserContext userContext,
        [Frozen] IValidationCache validationCache)
    {
        // Arrange
        var familyId = FamilyId.New();
        var email = Email.From("test@example.com");
        var invitedByUserId = UserId.New();
        var invitation = FamilyMemberInvitationAggregate.CreateEmailInvitation(
            familyId, email, FamilyRole.Member, invitedByUserId);
        var timeProvider = new FakeTimeProvider(DateTimeOffset.UtcNow);

        // Cancel the invitation
        invitation.Cancel(invitedByUserId);

        invitationRepository.GetByTokenAsync(invitation.Token, Arg.Any<CancellationToken>())
            .Returns(invitation);

        var validator = new AcceptInvitationCommandValidator(
            invitationRepository, familyService, userContext, timeProvider, validationCache);
        var command = new AcceptInvitationCommand(invitation.Token);

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Contain("Cannot accept invitation in canceled status");
    }

    #endregion

    #region Expiration Validation

    [Theory, AutoNSubstituteData]
    public async Task Validate_WithExpiredInvitation_ShouldFailWithExpirationMessage(
        [Frozen] IFamilyMemberInvitationRepository invitationRepository,
        [Frozen] IFamilyService familyService,
        [Frozen] IUserContext userContext,
        [Frozen] IValidationCache validationCache)
    {
        // Arrange
        var familyId = FamilyId.New();
        var email = Email.From("test@example.com");
        var invitedByUserId = UserId.New();
        var invitation = FamilyMemberInvitationAggregate.CreateEmailInvitation(
            familyId, email, FamilyRole.Member, invitedByUserId);

        // Set time provider to time AFTER expiration
        var expiredTime = invitation.ExpiresAt.AddDays(1);
        var timeProvider = new FakeTimeProvider(expiredTime);

        invitationRepository.GetByTokenAsync(invitation.Token, Arg.Any<CancellationToken>())
            .Returns(invitation);

        var validator = new AcceptInvitationCommandValidator(
            invitationRepository, familyService, userContext, timeProvider, validationCache);
        var command = new AcceptInvitationCommand(invitation.Token);

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be("Invitation has expired and cannot be accepted.");
    }

    #endregion

    #region Email Match Validation

    [Theory, AutoNSubstituteData]
    public async Task Validate_WithEmailMismatch_ShouldFailWithEmailMismatchMessage(
        [Frozen] IFamilyMemberInvitationRepository invitationRepository,
        [Frozen] IFamilyService familyService,
        [Frozen] IUserContext userContext,
        [Frozen] IValidationCache validationCache)
    {
        // Arrange
        var familyId = FamilyId.New();
        var invitationEmail = Email.From("invited@example.com");
        var userEmail = Email.From("different@example.com");
        var invitedByUserId = UserId.New();

        var invitation = FamilyMemberInvitationAggregate.CreateEmailInvitation(
            familyId, invitationEmail, FamilyRole.Member, invitedByUserId);

        var user = User.CreateFromOAuth(userEmail, "ext-123", "zitadel", familyId);
        var timeProvider = new FakeTimeProvider(DateTimeOffset.UtcNow);

        // Mock
        userContext.User.Returns(user);
        invitationRepository.GetByTokenAsync(invitation.Token, Arg.Any<CancellationToken>())
            .Returns(invitation);

        var validator = new AcceptInvitationCommandValidator(
            invitationRepository, familyService, userContext, timeProvider, validationCache);
        var command = new AcceptInvitationCommand(invitation.Token);

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be("Invitation email does not match authenticated user.");
    }

    #endregion

    #region Family Existence Validation

    [Theory, AutoNSubstituteData]
    public async Task Validate_WithNonExistentFamily_ShouldFailWithFamilyNotFoundMessage(
        [Frozen] IFamilyMemberInvitationRepository invitationRepository,
        [Frozen] IFamilyService familyService,
        [Frozen] IUserContext userContext,
        [Frozen] IValidationCache validationCache)
    {
        // Arrange
        var familyId = FamilyId.New();
        var email = Email.From("test@example.com");
        var invitedByUserId = UserId.New();

        var invitation = FamilyMemberInvitationAggregate.CreateEmailInvitation(
            familyId, email, FamilyRole.Member, invitedByUserId);

        var user = User.CreateFromOAuth(email, "ext-123", "zitadel", familyId);
        var timeProvider = new FakeTimeProvider(DateTimeOffset.UtcNow);

        // Mock - family does not exist (IFamilyService returns null)
        userContext.User.Returns(user);
        invitationRepository.GetByTokenAsync(invitation.Token, Arg.Any<CancellationToken>())
            .Returns(invitation);
        familyService.GetFamilyByIdAsync(familyId, Arg.Any<CancellationToken>())
            .Returns((FamilyDto?)null);

        var validator = new AcceptInvitationCommandValidator(
            invitationRepository, familyService, userContext, timeProvider, validationCache);
        var command = new AcceptInvitationCommand(invitation.Token);

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be("Family not found.");
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Creates a FamilyDto for testing purposes.
    /// This replaces the FamilyAggregate.Create() pattern used in previous tests,
    /// enforcing the Anti-Corruption Layer boundary in tests as well.
    /// </summary>
    private static FamilyDto CreateFamilyDto(FamilyId familyId, string name, UserId ownerId)
    {
        var now = DateTime.UtcNow;
        return new FamilyDto
        {
            Id = familyId,
            Name = FamilyName.From(name),
            OwnerId = ownerId,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    #endregion
}
