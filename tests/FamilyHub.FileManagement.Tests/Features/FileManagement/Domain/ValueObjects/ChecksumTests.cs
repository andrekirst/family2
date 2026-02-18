using FluentAssertions;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using Vogen;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Domain.ValueObjects;

public class ChecksumTests
{
    [Fact]
    public void From_Valid64CharHex_Succeeds()
    {
        var hash = "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855";

        var checksum = Checksum.From(hash);

        checksum.Value.Should().Be(hash);
    }

    [Fact]
    public void From_EmptyString_ThrowsValidation()
    {
        var act = () => Checksum.From("");

        act.Should().Throw<ValueObjectValidationException>();
    }

    [Fact]
    public void From_WrongLength_ThrowsValidation()
    {
        var act = () => Checksum.From("abc123");

        act.Should().Throw<ValueObjectValidationException>();
    }

    [Fact]
    public void From_UppercaseHex_ThrowsValidation()
    {
        var act = () => Checksum.From("E3B0C44298FC1C149AFBF4C8996FB92427AE41E4649B934CA495991B7852B855");

        act.Should().Throw<ValueObjectValidationException>();
    }
}
