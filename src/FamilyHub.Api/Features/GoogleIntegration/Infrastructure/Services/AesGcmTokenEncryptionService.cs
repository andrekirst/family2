using System.Security.Cryptography;
using FamilyHub.Api.Features.GoogleIntegration.Models;
using Microsoft.Extensions.Options;

namespace FamilyHub.Api.Features.GoogleIntegration.Infrastructure.Services;

public sealed class AesGcmTokenEncryptionService : ITokenEncryptionService
{
    private const int NonceSizeBytes = 12;
    private const int TagSizeBytes = 16;

    private readonly byte[] _key;

    public AesGcmTokenEncryptionService(IOptions<GoogleIntegrationOptions> options)
    {
        var keyBase64 = options.Value.EncryptionKey;
        if (string.IsNullOrWhiteSpace(keyBase64))
            throw new InvalidOperationException(
                "GoogleIntegration:EncryptionKey must be configured. " +
                "Generate with: Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))");

        _key = Convert.FromBase64String(keyBase64);
        if (_key.Length != 32)
            throw new InvalidOperationException("Encryption key must be exactly 256 bits (32 bytes).");
    }

    public string Encrypt(string plaintext)
    {
        var plaintextBytes = System.Text.Encoding.UTF8.GetBytes(plaintext);
        var nonce = RandomNumberGenerator.GetBytes(NonceSizeBytes);
        var ciphertext = new byte[plaintextBytes.Length];
        var tag = new byte[TagSizeBytes];

        using var aes = new AesGcm(_key, TagSizeBytes);
        aes.Encrypt(nonce, plaintextBytes, ciphertext, tag);

        // Format: nonce[12] + ciphertext[N] + tag[16]
        var result = new byte[NonceSizeBytes + ciphertext.Length + TagSizeBytes];
        nonce.CopyTo(result, 0);
        ciphertext.CopyTo(result, NonceSizeBytes);
        tag.CopyTo(result, NonceSizeBytes + ciphertext.Length);

        return Convert.ToBase64String(result);
    }

    public string Decrypt(string ciphertextBase64)
    {
        var combined = Convert.FromBase64String(ciphertextBase64);

        if (combined.Length < NonceSizeBytes + TagSizeBytes)
            throw new CryptographicException("Invalid ciphertext: too short.");

        var nonce = combined[..NonceSizeBytes];
        var ciphertext = combined[NonceSizeBytes..^TagSizeBytes];
        var tag = combined[^TagSizeBytes..];

        var plaintext = new byte[ciphertext.Length];

        using var aes = new AesGcm(_key, TagSizeBytes);
        aes.Decrypt(nonce, ciphertext, tag, plaintext);

        return System.Text.Encoding.UTF8.GetString(plaintext);
    }
}
