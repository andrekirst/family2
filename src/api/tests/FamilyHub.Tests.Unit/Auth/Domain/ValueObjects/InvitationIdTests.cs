using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Tests.Unit.Auth.Domain.ValueObjects;

/// <summary>
/// Unit tests for InvitationId value object.
/// </summary>
public class InvitationIdTests
{
    [Fact]
    public void New_ShouldGenerateUniqueGuid()
    {
        // Act
        var id1 = InvitationId.New();
        var id2 = InvitationId.New();

        // Assert
        id1.Value.Should().NotBe(Guid.Empty);
        id2.Value.Should().NotBe(Guid.Empty);
        id1.Should().NotBe(id2);
    }

    [Fact]
    public void From_WithValidGuid_ShouldCreateInvitationId()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var id = InvitationId.From(guid);

        // Assert
        id.Value.Should().Be(guid);
    }

    [Fact]
    public void From_WithGuidEmpty_ShouldAllowForEfCore()
    {
        // Arrange & Act
        var id = InvitationId.From(Guid.Empty);

        // Assert
        id.Value.Should().Be(Guid.Empty);
    }

    [Fact]
    public void Equals_WithSameValue_ShouldBeEqual()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var id1 = InvitationId.From(guid);
        var id2 = InvitationId.From(guid);

        // Act & Assert
        id1.Should().Be(id2);
        (id1 == id2).Should().BeTrue();
    }
}
