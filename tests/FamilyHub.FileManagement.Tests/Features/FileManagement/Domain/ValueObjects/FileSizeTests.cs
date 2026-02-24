using FluentAssertions;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using Vogen;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Domain.ValueObjects;

public class FileSizeTests
{
    [Fact]
    public void From_Zero_Succeeds()
    {
        var size = FileSize.From(0);

        size.Value.Should().Be(0);
    }

    [Fact]
    public void From_PositiveValue_Succeeds()
    {
        var size = FileSize.From(1024);

        size.Value.Should().Be(1024);
    }

    [Fact]
    public void From_NegativeValue_ThrowsValidation()
    {
        var act = () => FileSize.From(-1);

        act.Should().Throw<ValueObjectValidationException>();
    }

    [Fact]
    public void ToHumanReadable_Bytes_FormatsCorrectly()
    {
        var size = FileSize.From(512);

        size.ToHumanReadable().Should().Be("512 B");
    }

    [Fact]
    public void ToHumanReadable_Kilobytes_FormatsCorrectly()
    {
        var size = FileSize.From(1536); // 1.5 KB

        size.ToHumanReadable().Should().Be("1.5 KB");
    }

    [Fact]
    public void ToHumanReadable_Megabytes_FormatsCorrectly()
    {
        var size = FileSize.From(5 * 1024 * 1024); // 5 MB

        size.ToHumanReadable().Should().Be("5.0 MB");
    }

    [Fact]
    public void ToHumanReadable_Gigabytes_FormatsCorrectly()
    {
        var size = FileSize.From(2L * 1024 * 1024 * 1024); // 2 GB

        size.ToHumanReadable().Should().Be("2.0 GB");
    }
}
