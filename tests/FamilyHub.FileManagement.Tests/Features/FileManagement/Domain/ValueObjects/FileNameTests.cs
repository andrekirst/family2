using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FluentAssertions;
using Vogen;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Domain.ValueObjects;

public class FileNameTests
{
    [Fact]
    public void From_ValidName_ShouldSucceed()
    {
        var name = FileName.From("document.pdf");

        name.Value.Should().Be("document.pdf");
    }

    [Fact]
    public void From_MaxLength_ShouldSucceed()
    {
        var longName = new string('a', 255);
        var name = FileName.From(longName);

        name.Value.Should().HaveLength(255);
    }

    [Fact]
    public void From_Empty_ShouldThrow()
    {
        var act = () => FileName.From(string.Empty);

        act.Should().Throw<ValueObjectValidationException>();
    }

    [Fact]
    public void From_TooLong_ShouldThrow()
    {
        var act = () => FileName.From(new string('a', 256));

        act.Should().Throw<ValueObjectValidationException>();
    }

    [Fact]
    public void From_ContainsForwardSlash_ShouldThrow()
    {
        var act = () => FileName.From("path/file.txt");

        act.Should().Throw<ValueObjectValidationException>();
    }

    [Fact]
    public void From_ContainsBackslash_ShouldThrow()
    {
        var act = () => FileName.From("path\\file.txt");

        act.Should().Throw<ValueObjectValidationException>();
    }

    [Fact]
    public void From_ContainsNullByte_ShouldThrow()
    {
        var act = () => FileName.From("file\0.txt");

        act.Should().Throw<ValueObjectValidationException>();
    }
}
