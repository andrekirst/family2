using FluentAssertions;
using FamilyHub.Api.Features.FileManagement.Infrastructure.Storage;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Infrastructure.Storage;

public class ChecksumCalculatorTests
{
    private readonly ChecksumCalculator _calculator = new();

    [Fact]
    public void Compute_EmptyData_ReturnsKnownHash()
    {
        // SHA-256 of empty input is a well-known constant
        var expected = "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855";

        var result = _calculator.Compute(ReadOnlySpan<byte>.Empty);

        result.Should().Be(expected);
    }

    [Fact]
    public void Compute_HelloWorld_ReturnsCorrectHash()
    {
        var data = "Hello, World!"u8.ToArray();
        // Known SHA-256 for "Hello, World!"
        var expected = "dffd6021bb2bd5b0af676290809ec3a53191dd81c7f70a4b28688a362182986f";

        var result = _calculator.Compute(data);

        result.Should().Be(expected);
    }

    [Fact]
    public void Compute_ReturnsLowercaseHex()
    {
        var data = "test"u8.ToArray();

        var result = _calculator.Compute(data);

        result.Should().MatchRegex("^[0-9a-f]{64}$");
    }

    [Fact]
    public void Verify_CorrectChecksum_ReturnsTrue()
    {
        var data = "Hello, World!"u8.ToArray();
        var checksum = _calculator.Compute(data);

        var result = _calculator.Verify(data, checksum);

        result.Should().BeTrue();
    }

    [Fact]
    public void Verify_IncorrectChecksum_ReturnsFalse()
    {
        var data = "Hello, World!"u8.ToArray();
        var wrongChecksum = "0000000000000000000000000000000000000000000000000000000000000000";

        var result = _calculator.Verify(data, wrongChecksum);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task ComputeAsync_Stream_ReturnsSameHashAsSpan()
    {
        var data = "Hello, World!"u8.ToArray();
        var expectedHash = _calculator.Compute(data);

        using var stream = new MemoryStream(data);
        var result = await _calculator.ComputeAsync(stream);

        result.Should().Be(expectedHash);
    }

    [Fact]
    public async Task ComputeAsync_ResetsStreamPosition()
    {
        var data = "test data"u8.ToArray();
        using var stream = new MemoryStream(data);

        await _calculator.ComputeAsync(stream);

        stream.Position.Should().Be(0);
    }

    [Fact]
    public void Compute_SameDataTwice_ReturnsSameResult()
    {
        var data = "deterministic"u8.ToArray();

        var hash1 = _calculator.Compute(data);
        var hash2 = _calculator.Compute(data);

        hash1.Should().Be(hash2);
    }

    [Fact]
    public void Compute_DifferentData_ReturnsDifferentResults()
    {
        var data1 = "hello"u8.ToArray();
        var data2 = "world"u8.ToArray();

        var hash1 = _calculator.Compute(data1);
        var hash2 = _calculator.Compute(data2);

        hash1.Should().NotBe(hash2);
    }
}
