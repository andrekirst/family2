using FluentAssertions;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using Vogen;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Domain.ValueObjects;

public class StorageKeyTests
{
    [Fact]
    public void New_GeneratesUniqueKey()
    {
        var key1 = StorageKey.New();
        var key2 = StorageKey.New();

        key1.Value.Should().NotBe(key2.Value);
    }

    [Fact]
    public void From_ValidString_Succeeds()
    {
        var key = StorageKey.From("my-storage-key");

        key.Value.Should().Be("my-storage-key");
    }

    [Fact]
    public void From_EmptyString_ThrowsValidation()
    {
        var act = () => StorageKey.From("");

        act.Should().Throw<ValueObjectValidationException>();
    }

    [Fact]
    public void From_WhitespaceOnly_ThrowsValidation()
    {
        var act = () => StorageKey.From("   ");

        act.Should().Throw<ValueObjectValidationException>();
    }

    [Fact]
    public void New_ProducesGuidFormat()
    {
        var key = StorageKey.New();

        Guid.TryParse(key.Value, out _).Should().BeTrue();
    }
}
