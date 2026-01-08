using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Tests.Unit.SharedKernel.Domain.ValueObjects;

/// <summary>
/// Unit tests for FamilyRole value object.
/// </summary>
public class FamilyRoleTests
{
    [Fact]
    public void Owner_ShouldHaveCorrectValue()
    {
        // Act
        var role = FamilyRole.Owner;

        // Assert
        role.Value.Should().Be("owner");
    }

    [Fact]
    public void Admin_ShouldHaveCorrectValue()
    {
        // Act
        var role = FamilyRole.Admin;

        // Assert
        role.Value.Should().Be("admin");
    }

    [Fact]
    public void Member_ShouldHaveCorrectValue()
    {
        // Act
        var role = FamilyRole.Member;

        // Assert
        role.Value.Should().Be("member");
    }

    [Fact]
    public void Child_ShouldHaveCorrectValue()
    {
        // Act
        var role = FamilyRole.Child;

        // Assert
        role.Value.Should().Be("child");
    }

    [Theory]
    [InlineData("owner")]
    [InlineData("admin")]
    [InlineData("member")]
    [InlineData("child")]
    public void From_WithValidRole_ShouldCreateFamilyRole(string roleValue)
    {
        // Act
        var role = FamilyRole.From(roleValue);

        // Assert
        role.Value.Should().Be(roleValue);
    }

    [Theory]
    [InlineData("Owner")]
    [InlineData("ADMIN")]
    [InlineData("MeMbEr")]
    [InlineData("CHILD")]
    public void From_WithMixedCase_ShouldNormalizeToLowerCase(string roleValue)
    {
        // Act
        var role = FamilyRole.From(roleValue);

        // Assert
        role.Value.Should().Be(roleValue.ToLowerInvariant());
    }

    [Theory]
    [InlineData("  owner  ")]
    [InlineData("\tadmin\t")]
    [InlineData(" member ")]
    public void From_WithWhitespace_ShouldTrimAndNormalize(string roleValue)
    {
        // Act
        var role = FamilyRole.From(roleValue);

        // Assert
        role.Value.Should().NotContain(" ");
        role.Value.Should().NotContain("\t");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void From_WithEmptyOrWhitespace_ShouldThrowException(string roleValue)
    {
        // Act
        var act = () => FamilyRole.From(roleValue);

        // Assert
        act.Should().Throw<Vogen.ValueObjectValidationException>()
            .WithMessage("*Family role cannot be empty*");
    }

    [Theory]
    [InlineData("guest")]
    [InlineData("superadmin")]
    [InlineData("moderator")]
    [InlineData("invalid")]
    public void From_WithInvalidRole_ShouldThrowException(string roleValue)
    {
        // Act
        var act = () => FamilyRole.From(roleValue);

        // Assert
        act.Should().Throw<Vogen.ValueObjectValidationException>()
            .WithMessage("*Invalid family role*");
    }

    [Fact]
    public void Equals_WithSameRole_ShouldBeEqual()
    {
        // Arrange
        var role1 = FamilyRole.From("owner");
        var role2 = FamilyRole.From("owner");

        // Act & Assert
        role1.Should().Be(role2);
        (role1 == role2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentRoles_ShouldNotBeEqual()
    {
        // Arrange
        var role1 = FamilyRole.From("owner");
        var role2 = FamilyRole.From("admin");

        // Act & Assert
        role1.Should().NotBe(role2);
        (role1 != role2).Should().BeTrue();
    }

    [Fact]
    public void Owner_ShouldBeEqualToOwnerFromString()
    {
        // Arrange
        var ownerStatic = FamilyRole.Owner;
        var ownerFromString = FamilyRole.From("owner");

        // Act & Assert
        ownerStatic.Should().Be(ownerFromString);
    }

    [Fact]
    public void ToString_ShouldReturnRoleValue()
    {
        // Arrange
        var role = FamilyRole.Owner;

        // Act
        var result = role.ToString();

        // Assert
        result.Should().Be("owner");
    }

    [Theory]
    [InlineData("owner", "owner", true)]
    [InlineData("admin", "admin", true)]
    [InlineData("owner", "admin", false)]
    [InlineData("member", "child", false)]
    public void EqualityOperator_ShouldCompareCorrectly(string role1Value, string role2Value, bool expectedEqual)
    {
        // Arrange
        var role1 = FamilyRole.From(role1Value);
        var role2 = FamilyRole.From(role2Value);

        // Act & Assert
        (role1 == role2).Should().Be(expectedEqual);
        (role1 != role2).Should().Be(!expectedEqual);
    }

    [Fact]
    public void GetHashCode_WithSameRole_ShouldReturnSameHashCode()
    {
        // Arrange
        var role1 = FamilyRole.From("owner");
        var role2 = FamilyRole.From("owner");

        // Act & Assert
        role1.GetHashCode().Should().Be(role2.GetHashCode());
    }
}
