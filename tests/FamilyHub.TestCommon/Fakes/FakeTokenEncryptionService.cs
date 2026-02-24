using FamilyHub.Api.Features.GoogleIntegration.Infrastructure.Services;

namespace FamilyHub.TestCommon.Fakes;

public class FakeTokenEncryptionService : ITokenEncryptionService
{
    public string Encrypt(string plaintext) => $"encrypted:{plaintext}";
    public string Decrypt(string ciphertext) => ciphertext.Replace("encrypted:", "");
}
