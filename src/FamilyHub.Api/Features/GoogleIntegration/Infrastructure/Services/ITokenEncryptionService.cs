namespace FamilyHub.Api.Features.GoogleIntegration.Infrastructure.Services;

public interface ITokenEncryptionService
{
    string Encrypt(string plaintext);
    string Decrypt(string ciphertext);
}
