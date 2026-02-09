using FamilyHub.Api.Features.Family.Domain.ValueObjects;
using FluentAssertions;

namespace FamilyHub.UnitTests.Features.Family.Domain;

/// <summary>
/// Unit tests for FamilyRole value object permission methods.
/// Validates the Owner > Admin > Member permission hierarchy.
/// </summary>
public class FamilyRoleTests
{
    [Fact]
    public void Owner_ShouldHaveAllPermissions()
    {
        var role = FamilyRole.Owner;

        role.CanInvite().Should().BeTrue();
        role.CanRevokeInvitation().Should().BeTrue();
        role.CanRemoveMembers().Should().BeTrue();
        role.CanEditFamily().Should().BeTrue();
        role.CanDeleteFamily().Should().BeTrue();
        role.CanManageRoles().Should().BeTrue();
    }

    [Fact]
    public void Admin_ShouldHaveOperationalPermissions()
    {
        var role = FamilyRole.Admin;

        role.CanInvite().Should().BeTrue();
        role.CanRevokeInvitation().Should().BeTrue();
        role.CanRemoveMembers().Should().BeTrue();
        role.CanEditFamily().Should().BeTrue();
    }

    [Fact]
    public void Admin_ShouldNotHaveOwnerOnlyPermissions()
    {
        var role = FamilyRole.Admin;

        role.CanDeleteFamily().Should().BeFalse();
        role.CanManageRoles().Should().BeFalse();
    }

    [Fact]
    public void Member_ShouldHaveNoPermissions()
    {
        var role = FamilyRole.Member;

        role.CanInvite().Should().BeFalse();
        role.CanRevokeInvitation().Should().BeFalse();
        role.CanRemoveMembers().Should().BeFalse();
        role.CanEditFamily().Should().BeFalse();
        role.CanDeleteFamily().Should().BeFalse();
        role.CanManageRoles().Should().BeFalse();
    }

    [Fact]
    public void GetPermissions_Owner_ShouldReturnAllPermissionStrings()
    {
        var permissions = FamilyRole.Owner.GetPermissions();

        permissions.Should().BeEquivalentTo([
            "family:invite",
            "family:revoke-invitation",
            "family:remove-members",
            "family:edit",
            "family:delete",
            "family:manage-roles"
        ]);
    }

    [Fact]
    public void GetPermissions_Admin_ShouldReturnOperationalPermissions()
    {
        var permissions = FamilyRole.Admin.GetPermissions();

        permissions.Should().BeEquivalentTo([
            "family:invite",
            "family:revoke-invitation",
            "family:remove-members",
            "family:edit"
        ]);
    }

    [Fact]
    public void GetPermissions_Member_ShouldReturnEmptyList()
    {
        var permissions = FamilyRole.Member.GetPermissions();

        permissions.Should().BeEmpty();
    }
}
