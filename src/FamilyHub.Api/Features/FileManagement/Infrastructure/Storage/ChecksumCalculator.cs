using System.Security.Cryptography;

namespace FamilyHub.Api.Features.FileManagement.Infrastructure.Storage;

/// <summary>
/// Computes and verifies SHA-256 checksums for file integrity.
/// </summary>
public interface IChecksumCalculator
{
    /// <summary>
    /// Computes SHA-256 hash of the data, returned as lowercase hex string.
    /// </summary>
    string Compute(ReadOnlySpan<byte> data);

    /// <summary>
    /// Computes SHA-256 hash from a stream. Resets stream position after reading.
    /// </summary>
    Task<string> ComputeAsync(Stream data, CancellationToken ct = default);

    /// <summary>
    /// Verifies that data matches the expected checksum.
    /// </summary>
    bool Verify(ReadOnlySpan<byte> data, string expectedChecksum);
}

public sealed class ChecksumCalculator : IChecksumCalculator
{
    public string Compute(ReadOnlySpan<byte> data)
    {
        Span<byte> hash = stackalloc byte[SHA256.HashSizeInBytes];
        SHA256.HashData(data, hash);
        return Convert.ToHexStringLower(hash);
    }

    public async Task<string> ComputeAsync(Stream data, CancellationToken ct = default)
    {
        var startPosition = data.CanSeek ? data.Position : 0;

        var hash = await SHA256.HashDataAsync(data, ct);

        if (data.CanSeek)
            data.Position = startPosition;

        return Convert.ToHexStringLower(hash);
    }

    public bool Verify(ReadOnlySpan<byte> data, string expectedChecksum)
    {
        var actual = Compute(data);
        return string.Equals(actual, expectedChecksum, StringComparison.Ordinal);
    }
}
