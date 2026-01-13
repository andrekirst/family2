using FamilyHub.Modules.Auth.Domain.Specifications;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using FamilyHub.Tests.Unit.Builders;
using FamilyHub.Tests.Unit.Fixtures;
using FluentAssertions;

namespace FamilyHub.Tests.Unit.Specifications.Auth;

/// <summary>
/// Unit tests for UserByEmailSpecification.
/// </summary>
public class UserByEmailSpecificationTests
{
    [Fact]
    public void IsSatisfiedBy_MatchingEmail_ReturnsTrue()
    {
        // Arrange
        var email = Email.From("test@example.com");
        var user = new UserBuilder()
            .WithEmail(email)
            .Build();
        var spec = new UserByEmailSpecification(email);

        // Act & Assert
        user.ShouldSatisfy(spec);
    }

    [Fact]
    public void IsSatisfiedBy_NonMatchingEmail_ReturnsFalse()
    {
        // Arrange
        var email = Email.From("test@example.com");
        var user = new UserBuilder()
            .WithEmail("other@example.com")
            .Build();
        var spec = new UserByEmailSpecification(email);

        // Act & Assert
        user.ShouldNotSatisfy(spec);
    }

    [Fact]
    public void IsSatisfiedBy_DeletedUser_ReturnsFalse()
    {
        // Arrange
        var email = Email.From("test@example.com");
        var user = new UserBuilder()
            .WithEmail(email)
            .Build();
        user.Delete(); // Soft-delete the user
        var spec = new UserByEmailSpecification(email);

        // Act & Assert
        user.ShouldNotSatisfy(spec);
    }

    [Fact]
    public void ToExpression_ProducesValidExpression()
    {
        // Arrange
        var email = Email.From("test@example.com");
        var spec = new UserByEmailSpecification(email);

        // Act & Assert
        spec.ShouldHaveValidExpression();
    }

    [Fact]
    public void IgnoreQueryFilters_ReturnsFalse()
    {
        // Arrange
        var email = Email.From("test@example.com");
        var spec = new UserByEmailSpecification(email);

        // Act & Assert
        spec.IgnoreQueryFilters.Should().BeFalse();
    }
}

/// <summary>
/// Unit tests for UserByExternalProviderSpecification.
/// </summary>
public class UserByExternalProviderSpecificationTests
{
    [Fact]
    public void IsSatisfiedBy_MatchingProviderAndId_ReturnsTrue()
    {
        // Arrange
        var externalUserId = "user123";
        var externalProvider = "zitadel";
        var user = new UserBuilder()
            .WithExternalUserId(externalUserId)
            .WithExternalProvider(externalProvider)
            .Build();
        var spec = new UserByExternalProviderSpecification(externalProvider, externalUserId);

        // Act & Assert
        user.ShouldSatisfy(spec);
    }

    [Fact]
    public void IsSatisfiedBy_NonMatchingProvider_ReturnsFalse()
    {
        // Arrange
        var externalUserId = "user123";
        var user = new UserBuilder()
            .WithExternalUserId(externalUserId)
            .WithExternalProvider("zitadel")
            .Build();
        var spec = new UserByExternalProviderSpecification("google", externalUserId);

        // Act & Assert
        user.ShouldNotSatisfy(spec);
    }

    [Fact]
    public void IsSatisfiedBy_NonMatchingUserId_ReturnsFalse()
    {
        // Arrange
        var externalProvider = "zitadel";
        var user = new UserBuilder()
            .WithExternalUserId("user123")
            .WithExternalProvider(externalProvider)
            .Build();
        var spec = new UserByExternalProviderSpecification(externalProvider, "differentUser");

        // Act & Assert
        user.ShouldNotSatisfy(spec);
    }

    [Fact]
    public void IsSatisfiedBy_DeletedUser_ReturnsFalse()
    {
        // Arrange
        var externalUserId = "user123";
        var externalProvider = "zitadel";
        var user = new UserBuilder()
            .WithExternalUserId(externalUserId)
            .WithExternalProvider(externalProvider)
            .Build();
        user.Delete();
        var spec = new UserByExternalProviderSpecification(externalProvider, externalUserId);

        // Act & Assert
        user.ShouldNotSatisfy(spec);
    }

    [Fact]
    public void Constructor_NullProvider_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => new UserByExternalProviderSpecification(null!, "userId");
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_NullUserId_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => new UserByExternalProviderSpecification("provider", null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ToExpression_ProducesValidExpression()
    {
        // Arrange
        var spec = new UserByExternalProviderSpecification("zitadel", "user123");

        // Act & Assert
        spec.ShouldHaveValidExpression();
    }
}

/// <summary>
/// Unit tests for UsersByFamilySpecification.
/// </summary>
public class UsersByFamilySpecificationTests
{
    [Fact]
    public void IsSatisfiedBy_UserInFamily_ReturnsTrue()
    {
        // Arrange
        var familyId = FamilyId.New();
        var user = new UserBuilder()
            .WithFamilyId(familyId)
            .Build();
        var spec = new UsersByFamilySpecification(familyId);

        // Act & Assert
        user.ShouldSatisfy(spec);
    }

    [Fact]
    public void IsSatisfiedBy_UserInDifferentFamily_ReturnsFalse()
    {
        // Arrange
        var familyId1 = FamilyId.New();
        var familyId2 = FamilyId.New();
        var user = new UserBuilder()
            .WithFamilyId(familyId1)
            .Build();
        var spec = new UsersByFamilySpecification(familyId2);

        // Act & Assert
        user.ShouldNotSatisfy(spec);
    }

    [Fact]
    public void IsSatisfiedBy_DeletedUser_ReturnsFalse()
    {
        // Arrange
        var familyId = FamilyId.New();
        var user = new UserBuilder()
            .WithFamilyId(familyId)
            .Build();
        user.Delete();
        var spec = new UsersByFamilySpecification(familyId);

        // Act & Assert
        user.ShouldNotSatisfy(spec);
    }

    [Fact]
    public void Fixture_ShouldMatchExactly_ActiveUsersInFamily()
    {
        // Arrange
        var familyId = FamilyId.New();
        var otherFamilyId = FamilyId.New();

        var user1 = new UserBuilder().WithFamilyId(familyId).WithEmail("user1@test.com").Build();
        var user2 = new UserBuilder().WithFamilyId(familyId).WithEmail("user2@test.com").Build();
        var user3 = new UserBuilder().WithFamilyId(otherFamilyId).WithEmail("user3@test.com").Build();
        var deletedUser = new UserBuilder().WithFamilyId(familyId).WithEmail("deleted@test.com").Build();
        deletedUser.Delete();

        var fixture = SpecificationTestExtensions.CreateSpecificationFixture(user1, user2, user3, deletedUser);
        var spec = new UsersByFamilySpecification(familyId);

        // Act & Assert
        fixture.ShouldMatchExactly(spec, user1, user2);
    }
}

/// <summary>
/// Unit tests for IncludeSoftDeletedUserSpecification.
/// </summary>
public class IncludeSoftDeletedUserSpecificationTests
{
    [Fact]
    public void IsSatisfiedBy_ActiveUserMatchingEmail_ReturnsTrue()
    {
        // Arrange
        var email = Email.From("test@example.com");
        var user = new UserBuilder()
            .WithEmail(email)
            .Build();
        var spec = new IncludeSoftDeletedUserSpecification(email);

        // Act & Assert
        user.ShouldSatisfy(spec);
    }

    [Fact]
    public void IsSatisfiedBy_DeletedUserMatchingEmail_ReturnsTrue()
    {
        // Arrange
        var email = Email.From("test@example.com");
        var user = new UserBuilder()
            .WithEmail(email)
            .Build();
        user.Delete();
        var spec = new IncludeSoftDeletedUserSpecification(email);

        // Act - Note: In-memory IsSatisfiedBy doesn't use IgnoreQueryFilters
        // The expression itself matches, IgnoreQueryFilters affects EF Core
        var result = spec.IsSatisfiedBy(user);

        // Assert - The expression matches the email regardless of DeletedAt
        result.Should().BeTrue();
    }

    [Fact]
    public void IgnoreQueryFilters_ReturnsTrue()
    {
        // Arrange
        var email = Email.From("test@example.com");
        var spec = new IncludeSoftDeletedUserSpecification(email);

        // Act & Assert
        spec.IgnoreQueryFilters.Should().BeTrue();
    }
}

/// <summary>
/// Unit tests for UserByIdSpecification.
/// </summary>
public class UserByIdSpecificationTests
{
    [Fact]
    public void IsSatisfiedBy_MatchingId_ReturnsTrue()
    {
        // Arrange
        var user = new UserBuilder().Build();
        var spec = new UserByIdSpecification(user.Id);

        // Act & Assert
        user.ShouldSatisfy(spec);
    }

    [Fact]
    public void IsSatisfiedBy_NonMatchingId_ReturnsFalse()
    {
        // Arrange
        var user = new UserBuilder().Build();
        var differentId = UserId.New();
        var spec = new UserByIdSpecification(differentId);

        // Act & Assert
        user.ShouldNotSatisfy(spec);
    }

    [Fact]
    public void ToExpression_ProducesValidExpression()
    {
        // Arrange
        var spec = new UserByIdSpecification(UserId.New());

        // Act & Assert
        spec.ShouldHaveValidExpression();
    }
}
