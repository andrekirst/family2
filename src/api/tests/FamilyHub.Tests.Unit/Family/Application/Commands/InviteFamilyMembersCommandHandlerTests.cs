using FamilyHub.Modules.Family.Application.Commands.InviteFamilyMembers;
using FamilyHub.Modules.Family.Domain.Abstractions;
using FamilyHub.Modules.Family.Domain.Aggregates;
using FamilyHub.Modules.Family.Domain.Repositories;
using FamilyHub.SharedKernel.Application.Abstractions;
using FamilyHub.SharedKernel.Domain.Specifications;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace FamilyHub.Tests.Unit.Family.Application.Commands;

/// <summary>
/// Unit tests for InviteFamilyMembersCommandHandler.
/// Tests batch invitation processing with partial success support.
/// </summary>
/// <remarks>
/// IMPORTANT: NSubstitute does not work well with Vogen value objects when using Arg.Any&lt;T&gt;()
/// because of how Vogen generates implicit conversion operators. To work around this:
/// - Use .ReturnsForAnyArgs() without calling the method first
/// - Set up specific return values for specific inputs where needed
/// </remarks>
public sealed class InviteFamilyMembersCommandHandlerTests
{
    private readonly IUserContext _userContext;
    private readonly IFamilyRepository _familyRepository;
    private readonly IFamilyMemberInvitationRepository _invitationRepository;
    private readonly IUserLookupService _userLookupService;
    private readonly IFamilyUnitOfWork _familyUnitOfWork;
    private readonly ILogger<InviteFamilyMembersCommandHandler> _logger;

    private readonly FamilyId _familyId;
    private readonly UserId _currentUserId;
    private readonly Email _currentUserEmail;

    public InviteFamilyMembersCommandHandlerTests()
    {
        _userContext = Substitute.For<IUserContext>();
        _familyRepository = Substitute.For<IFamilyRepository>();
        _invitationRepository = Substitute.For<IFamilyMemberInvitationRepository>();
        _userLookupService = Substitute.For<IUserLookupService>();
        _familyUnitOfWork = Substitute.For<IFamilyUnitOfWork>();
        _logger = Substitute.For<ILogger<InviteFamilyMembersCommandHandler>>();

        _familyId = FamilyId.New();
        _currentUserId = UserId.New();
        _currentUserEmail = Email.From("admin@example.com");

        _userContext.UserId.Returns(_currentUserId);
        _userContext.Email.Returns(_currentUserEmail);
    }

    private void SetupFamilyExists()
    {
        var family = Modules.Family.Domain.Aggregates.Family.Create(
            FamilyName.From("Test Family"),
            _currentUserId);

        // Configure returns for any arguments using a real FamilyId instance
        _familyRepository.GetByIdAsync(_familyId, CancellationToken.None)
            .ReturnsForAnyArgs(Task.FromResult<Modules.Family.Domain.Aggregates.Family?>(family));
    }

    private void SetupFamilyNotExists()
    {
        _familyRepository.GetByIdAsync(_familyId, CancellationToken.None)
            .ReturnsForAnyArgs(Task.FromResult<Modules.Family.Domain.Aggregates.Family?>(null));
    }

    private void SetupAllowAllInvitations()
    {
        // Use real values to satisfy Vogen, then ReturnsForAnyArgs to match any call
        var dummyEmail = Email.From("dummy@test.com");

        // No one is already a member
        _userLookupService.IsEmailMemberOfFamilyAsync(_familyId, dummyEmail, CancellationToken.None)
            .ReturnsForAnyArgs(Task.FromResult(false));

        // No one belongs to another family
        _userLookupService.GetFamilyIdByEmailAsync(dummyEmail, CancellationToken.None)
            .ReturnsForAnyArgs(Task.FromResult<FamilyId?>(null));

        // No pending invitations - use FindOneAsync overload with specification
        _invitationRepository
            .FindOneAsync(null!, CancellationToken.None)
            .ReturnsForAnyArgs(Task.FromResult<FamilyMemberInvitation?>(null));
    }

    private InviteFamilyMembersCommandHandler CreateHandler()
    {
        return new InviteFamilyMembersCommandHandler(
            _userContext,
            _familyRepository,
            _invitationRepository,
            _userLookupService,
            _familyUnitOfWork,
            _logger);
    }

    #region Happy Path Tests

    [Fact]
    public async Task Handle_WithSingleValidInvitation_ShouldSucceed()
    {
        // Arrange
        SetupFamilyExists();
        SetupAllowAllInvitations();

        var command = new InviteFamilyMembersCommand(
            _familyId,
            [new InvitationRequest(Email.From("member@example.com"), FamilyRole.Member)],
            "Welcome to our family!");

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.SuccessfulInvitations.Should().HaveCount(1);
        result.FailedInvitations.Should().BeEmpty();
        result.AllSucceeded.Should().BeTrue();
        result.AnySucceeded.Should().BeTrue();
        result.TotalRequested.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WithMultipleValidInvitations_ShouldCreateAllInvitations()
    {
        // Arrange
        SetupFamilyExists();
        SetupAllowAllInvitations();

        var command = new InviteFamilyMembersCommand(
            _familyId,
            [
                new InvitationRequest(Email.From("member1@example.com"), FamilyRole.Member),
                new InvitationRequest(Email.From("member2@example.com"), FamilyRole.Member),
                new InvitationRequest(Email.From("admin@other.com"), FamilyRole.Admin)
            ]);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.SuccessfulInvitations.Should().HaveCount(3);
        result.FailedInvitations.Should().BeEmpty();
        result.TotalRequested.Should().Be(3);
    }

    [Fact]
    public async Task Handle_WithAllValidRoles_ShouldAcceptAdminMemberAndChild()
    {
        // Arrange
        SetupFamilyExists();
        SetupAllowAllInvitations();

        var command = new InviteFamilyMembersCommand(
            _familyId,
            [
                new InvitationRequest(Email.From("newadmin@example.com"), FamilyRole.Admin),
                new InvitationRequest(Email.From("member@example.com"), FamilyRole.Member),
                new InvitationRequest(Email.From("child@example.com"), FamilyRole.Child)
            ]);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.SuccessfulInvitations.Should().HaveCount(3);
        result.FailedInvitations.Should().BeEmpty();

        result.SuccessfulInvitations.Should().Contain(s => s.Role == FamilyRole.Admin);
        result.SuccessfulInvitations.Should().Contain(s => s.Role == FamilyRole.Member);
        result.SuccessfulInvitations.Should().Contain(s => s.Role == FamilyRole.Child);
    }

    #endregion

    #region Validation Error Tests

    [Fact]
    public async Task Handle_WhenFamilyNotFound_ShouldFailAllInvitations()
    {
        // Arrange
        SetupFamilyNotExists();

        var command = new InviteFamilyMembersCommand(
            _familyId,
            [new InvitationRequest(Email.From("member@example.com"), FamilyRole.Member)]);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.SuccessfulInvitations.Should().BeEmpty();
        result.FailedInvitations.Should().HaveCount(1);
        result.FailedInvitations.First().ErrorMessage.Should().Contain("Family not found");
    }

    [Fact]
    public async Task Handle_WithSelfInvite_ShouldFailWithSelfInviteError()
    {
        // Arrange
        SetupFamilyExists();
        SetupAllowAllInvitations();

        var command = new InviteFamilyMembersCommand(
            _familyId,
            [new InvitationRequest(_currentUserEmail, FamilyRole.Member)]);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.SuccessfulInvitations.Should().BeEmpty();
        result.FailedInvitations.Should().HaveCount(1);
        result.FailedInvitations.First().ErrorCode.Should().Be(InvitationErrorCode.SELF_INVITE);
        result.FailedInvitations.First().ErrorMessage.Should().Contain("Cannot invite yourself");
    }

    [Fact]
    public async Task Handle_WithOwnerRole_ShouldFailWithInvalidRoleError()
    {
        // Arrange
        SetupFamilyExists();
        SetupAllowAllInvitations();

        var command = new InviteFamilyMembersCommand(
            _familyId,
            [new InvitationRequest(Email.From("member@example.com"), FamilyRole.Owner)]);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.SuccessfulInvitations.Should().BeEmpty();
        result.FailedInvitations.Should().HaveCount(1);
        result.FailedInvitations.First().ErrorCode.Should().Be(InvitationErrorCode.INVALID_ROLE);
        result.FailedInvitations.First().ErrorMessage.Should().Contain("Cannot invite a member as OWNER");
    }

    [Fact]
    public async Task Handle_WithDuplicateEmailsInBatch_ShouldFailDuplicates()
    {
        // Arrange
        SetupFamilyExists();
        SetupAllowAllInvitations();

        var duplicateEmail = Email.From("duplicate@example.com");
        var command = new InviteFamilyMembersCommand(
            _familyId,
            [
                new InvitationRequest(duplicateEmail, FamilyRole.Member),
                new InvitationRequest(duplicateEmail, FamilyRole.Admin), // Duplicate
                new InvitationRequest(Email.From("unique@example.com"), FamilyRole.Member)
            ]);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.SuccessfulInvitations.Should().HaveCount(1); // Only unique@example.com
        result.FailedInvitations.Should().HaveCount(2); // Both duplicates fail
        result.FailedInvitations.Should().AllSatisfy(f =>
            f.ErrorCode.Should().Be(InvitationErrorCode.DUPLICATE_IN_BATCH));
    }

    #endregion

    #region Cross-Family and Duplicate Validation Tests

    [Fact]
    public async Task Handle_WhenEmailIsAlreadyMember_ShouldFailWithAlreadyMemberError()
    {
        // Arrange
        SetupFamilyExists();

        var existingMemberEmail = Email.From("existing@example.com");

        // Mock: This email is already a member of THIS family
        _userLookupService.IsEmailMemberOfFamilyAsync(_familyId, existingMemberEmail, CancellationToken.None)
            .Returns(Task.FromResult(true));

        // Other validations pass - use real values with ReturnsForAnyArgs
        _userLookupService.GetFamilyIdByEmailAsync(existingMemberEmail, CancellationToken.None)
            .ReturnsForAnyArgs(Task.FromResult<FamilyId?>(null));

        _invitationRepository.FindOneAsync(null!, CancellationToken.None)
            .ReturnsForAnyArgs(Task.FromResult<FamilyMemberInvitation?>(null));

        var command = new InviteFamilyMembersCommand(
            _familyId,
            [new InvitationRequest(existingMemberEmail, FamilyRole.Member)]);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.SuccessfulInvitations.Should().BeEmpty();
        result.FailedInvitations.Should().HaveCount(1);
        result.FailedInvitations.First().ErrorCode.Should().Be(InvitationErrorCode.ALREADY_MEMBER);
        result.FailedInvitations.First().Email.Should().Be(existingMemberEmail);
        result.FailedInvitations.First().ErrorMessage.Should().Contain("already a member of this family");
    }

    [Fact]
    public async Task Handle_WhenEmailIsMemberOfAnotherFamily_ShouldFailWithCrossFamilyError()
    {
        // Arrange
        SetupFamilyExists();

        var crossFamilyEmail = Email.From("crossfamily@example.com");
        var otherFamilyId = FamilyId.New();

        // Mock: Email is NOT a member of THIS family
        _userLookupService.IsEmailMemberOfFamilyAsync(_familyId, crossFamilyEmail, CancellationToken.None)
            .Returns(Task.FromResult(false));

        // Mock: Email IS a member of ANOTHER family
        _userLookupService.GetFamilyIdByEmailAsync(crossFamilyEmail, CancellationToken.None)
            .Returns(Task.FromResult<FamilyId?>(otherFamilyId));

        // Other validations pass
        _invitationRepository.FindOneAsync(null!, CancellationToken.None)
            .ReturnsForAnyArgs(Task.FromResult<FamilyMemberInvitation?>(null));

        var command = new InviteFamilyMembersCommand(
            _familyId,
            [new InvitationRequest(crossFamilyEmail, FamilyRole.Member)]);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.SuccessfulInvitations.Should().BeEmpty();
        result.FailedInvitations.Should().HaveCount(1);
        result.FailedInvitations.First().ErrorCode.Should().Be(InvitationErrorCode.MEMBER_OF_ANOTHER_FAMILY);
        result.FailedInvitations.First().Email.Should().Be(crossFamilyEmail);
        result.FailedInvitations.First().ErrorMessage.Should().Contain("already a member of another family");
    }

    [Fact]
    public async Task Handle_WhenPendingInvitationExists_ShouldFailWithDuplicateInvitationError()
    {
        // Arrange
        SetupFamilyExists();

        var invitedEmail = Email.From("pending@example.com");

        // Mock: Email is NOT already a member
        _userLookupService.IsEmailMemberOfFamilyAsync(_familyId, invitedEmail, CancellationToken.None)
            .Returns(Task.FromResult(false));

        _userLookupService.GetFamilyIdByEmailAsync(invitedEmail, CancellationToken.None)
            .Returns(Task.FromResult<FamilyId?>(null));

        // Mock: There IS a pending invitation for this email
        var existingInvitation = FamilyMemberInvitation.CreateEmailInvitation(
            _familyId,
            invitedEmail,
            FamilyRole.Member,
            _currentUserId,
            null);

        _invitationRepository.FindOneAsync(null!, CancellationToken.None)
            .ReturnsForAnyArgs(Task.FromResult<FamilyMemberInvitation?>(existingInvitation));

        var command = new InviteFamilyMembersCommand(
            _familyId,
            [new InvitationRequest(invitedEmail, FamilyRole.Member)]);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.SuccessfulInvitations.Should().BeEmpty();
        result.FailedInvitations.Should().HaveCount(1);
        result.FailedInvitations.First().ErrorCode.Should().Be(InvitationErrorCode.DUPLICATE_PENDING_INVITATION);
        result.FailedInvitations.First().Email.Should().Be(invitedEmail);
        result.FailedInvitations.First().ErrorMessage.Should().Contain("already has a pending invitation");
    }

    #endregion

    #region Partial Success Tests

    [Fact]
    public async Task Handle_WithMixedValidAndInvalidInvitations_ShouldReturnPartialSuccess()
    {
        // Arrange
        SetupFamilyExists();
        SetupAllowAllInvitations();

        // Command with: 1 valid, 1 self-invite (invalid), 1 owner role (invalid)
        var command = new InviteFamilyMembersCommand(
            _familyId,
            [
                new InvitationRequest(Email.From("valid@example.com"), FamilyRole.Member),
                new InvitationRequest(_currentUserEmail, FamilyRole.Member), // Self-invite
                new InvitationRequest(Email.From("owner@example.com"), FamilyRole.Owner) // Invalid role
            ]);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.SuccessfulInvitations.Should().HaveCount(1);
        result.FailedInvitations.Should().HaveCount(2);
        result.AllSucceeded.Should().BeFalse();
        result.AnySucceeded.Should().BeTrue();

        result.FailedInvitations.Should().Contain(f => f.ErrorCode == InvitationErrorCode.SELF_INVITE);
        result.FailedInvitations.Should().Contain(f => f.ErrorCode == InvitationErrorCode.INVALID_ROLE);
    }

    #endregion

    #region Result Structure Tests

    [Fact]
    public async Task Handle_SuccessfulInvitation_ShouldContainAllRequiredFields()
    {
        // Arrange
        SetupFamilyExists();
        SetupAllowAllInvitations();

        var requestEmail = Email.From("member@example.com");
        var command = new InviteFamilyMembersCommand(
            _familyId,
            [new InvitationRequest(requestEmail, FamilyRole.Admin)]);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.SuccessfulInvitations.Should().ContainSingle();
        var invitation = result.SuccessfulInvitations.First();

        invitation.InvitationId.Value.Should().NotBe(Guid.Empty);
        invitation.Email.Should().Be(requestEmail);
        invitation.Role.Should().Be(FamilyRole.Admin);
        invitation.Token.Value.Should().NotBeNullOrEmpty();
        invitation.DisplayCode.Value.Should().NotBeNullOrEmpty();
        invitation.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
        invitation.Status.Value.Should().Be("pending");
    }

    [Fact]
    public async Task Handle_FailedInvitation_ShouldContainEmailRoleAndError()
    {
        // Arrange
        SetupFamilyExists();
        SetupAllowAllInvitations();

        var command = new InviteFamilyMembersCommand(
            _familyId,
            [new InvitationRequest(_currentUserEmail, FamilyRole.Admin)]); // Self-invite

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.FailedInvitations.Should().ContainSingle();
        var failure = result.FailedInvitations.First();

        failure.Email.Should().Be(_currentUserEmail);
        failure.Role.Should().Be(FamilyRole.Admin);
        failure.ErrorCode.Should().Be(InvitationErrorCode.SELF_INVITE);
        failure.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    #endregion
}
