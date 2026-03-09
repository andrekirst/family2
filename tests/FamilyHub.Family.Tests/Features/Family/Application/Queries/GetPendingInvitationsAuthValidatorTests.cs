using FamilyHub.Api.Features.Family.Application.Queries.GetPendingInvitations;
using FamilyHub.Api.Features.Family.Application.Services;
using FamilyHub.Api.Features.Family.Domain.Entities;
using FamilyHub.Api.Features.Family.Domain.Repositories;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;
using FamilyHub.Api.Resources;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;
using NSubstitute;

namespace FamilyHub.Family.Tests.Features.Family.Application.Queries;

public class GetPendingInvitationsAuthValidatorTests
{
    [Theory]
    [InlineData(nameof(FamilyRole.Owner))]
    [InlineData(nameof(FamilyRole.Admin))]
    public async Task Validate_ShouldPass_WhenUserHasInvitePermission(string roleName)
    {
        // Arrange
        var familyId = FamilyId.New();
        var userId = UserId.New();
        var role = roleName == nameof(FamilyRole.Owner) ? FamilyRole.Owner : FamilyRole.Admin;
        var member = FamilyMember.Create(familyId, userId, role, DateTimeOffset.UtcNow);

        var memberRepo = Substitute.For<IFamilyMemberRepository>();
        memberRepo.GetByUserAndFamilyAsync(userId, familyId, Arg.Any<CancellationToken>())
            .Returns(member);

        var authService = new FamilyAuthorizationService(memberRepo);
        var validator = new GetPendingInvitationsAuthValidator(authService, new StubStringLocalizer<DomainErrors>());
        var query = new GetPendingInvitationsQuery { UserId = userId, FamilyId = familyId };

        // Act
        var result = await validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenUserLacksInvitePermission()
    {
        // Arrange — caller is a Member (not Owner/Admin), so CanInvite() is false
        var familyId = FamilyId.New();
        var userId = UserId.New();
        var member = FamilyMember.Create(familyId, userId, FamilyRole.Member, DateTimeOffset.UtcNow);

        var memberRepo = Substitute.For<IFamilyMemberRepository>();
        memberRepo.GetByUserAndFamilyAsync(userId, familyId, Arg.Any<CancellationToken>())
            .Returns(member);

        var authService = new FamilyAuthorizationService(memberRepo);
        var validator = new GetPendingInvitationsAuthValidator(authService, new StubStringLocalizer<DomainErrors>());
        var query = new GetPendingInvitationsQuery { UserId = userId, FamilyId = familyId };

        // Act
        var result = await validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorCode == DomainErrorCodes.InsufficientPermissionToViewInvitations);
    }
}
