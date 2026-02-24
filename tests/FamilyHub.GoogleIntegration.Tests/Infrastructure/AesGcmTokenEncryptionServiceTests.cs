using System.Security.Cryptography;
using FluentAssertions;
using FamilyHub.Api.Features.GoogleIntegration.Infrastructure.Services;
using FamilyHub.Api.Features.GoogleIntegration.Models;
using Microsoft.Extensions.Options;

namespace FamilyHub.GoogleIntegration.Tests.Infrastructure;

public class AesGcmTokenEncryptionServiceTests
{
    private static AesGcmTokenEncryptionService CreateService()
    {
        var key = RandomNumberGenerator.GetBytes(32);
        var keyBase64 = Convert.ToBase64String(key);
        var options = Options.Create(new GoogleIntegrationOptions { EncryptionKey = keyBase64 });
        return new AesGcmTokenEncryptionService(options);
    }

    [Fact]
    public void EncryptDecrypt_ShouldRoundTrip()
    {
        var service = CreateService();
        var plaintext = "ya29.a0AfH6SMBx_some_long_access_token";

        var encrypted = service.Encrypt(plaintext);
        var decrypted = service.Decrypt(encrypted);

        decrypted.Should().Be(plaintext);
    }

    [Fact]
    public void Encrypt_ShouldProduceDifferentOutputsForSameInput()
    {
        var service = CreateService();
        var plaintext = "same-token";

        var encrypted1 = service.Encrypt(plaintext);
        var encrypted2 = service.Encrypt(plaintext);

        // Different nonces should produce different ciphertexts
        encrypted1.Should().NotBe(encrypted2);
    }

    [Fact]
    public void Decrypt_WithWrongKey_ShouldThrow()
    {
        var service1 = CreateService();
        var service2 = CreateService(); // different key

        var encrypted = service1.Encrypt("secret-token");

        var act = () => service2.Decrypt(encrypted);
        act.Should().Throw<CryptographicException>();
    }

    [Fact]
    public void Decrypt_WithTamperedData_ShouldThrow()
    {
        var service = CreateService();
        var encrypted = service.Encrypt("token");

        // Tamper with the ciphertext
        var bytes = Convert.FromBase64String(encrypted);
        bytes[15] ^= 0xFF; // flip a byte in the ciphertext area
        var tampered = Convert.ToBase64String(bytes);

        var act = () => service.Decrypt(tampered);
        act.Should().Throw<CryptographicException>();
    }

    [Fact]
    public void Constructor_WithEmptyKey_ShouldThrow()
    {
        var options = Options.Create(new GoogleIntegrationOptions { EncryptionKey = "" });
        var act = () => new AesGcmTokenEncryptionService(options);
        act.Should().Throw<InvalidOperationException>().WithMessage("*must be configured*");
    }

    [Fact]
    public void Constructor_WithWrongKeySize_ShouldThrow()
    {
        var key = RandomNumberGenerator.GetBytes(16); // 128-bit, not 256-bit
        var options = Options.Create(new GoogleIntegrationOptions
        {
            EncryptionKey = Convert.ToBase64String(key)
        });

        var act = () => new AesGcmTokenEncryptionService(options);
        act.Should().Throw<InvalidOperationException>().WithMessage("*256 bits*");
    }
}
