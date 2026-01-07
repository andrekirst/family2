using System.Runtime.CompilerServices;

namespace FamilyHub.Tests.Integration;

/// <summary>
/// Module initializer that sets up test environment before ANY tests run.
/// This is necessary to configure Zitadel mock settings before Program.cs executes.
/// </summary>
internal static class TestEnvironmentSetup
{
    private static string? _mockPrivateKeyPath;

    [ModuleInitializer]
    internal static void Initialize()
    {
        // Set mock Zitadel configuration as environment variables
        // These MUST be set before WebApplication.CreateBuilder() in Program.cs
        Environment.SetEnvironmentVariable("Zitadel__Authority", "https://test.zitadel.cloud");
        Environment.SetEnvironmentVariable("Zitadel__ClientId", "test-client-id");
        Environment.SetEnvironmentVariable("Zitadel__ClientSecret", "test-client-secret");
        Environment.SetEnvironmentVariable("Zitadel__RedirectUri", "http://localhost:4200/auth/callback");
        Environment.SetEnvironmentVariable("Zitadel__Scopes", "openid profile email");
        Environment.SetEnvironmentVariable("Zitadel__Audience", "test-audience");
        Environment.SetEnvironmentVariable("Zitadel__ServiceAccountId", "123456789@test_project");
        Environment.SetEnvironmentVariable("Zitadel__PrivateKeyPath", CreateMockPrivateKeyFile());
        Environment.SetEnvironmentVariable("Zitadel__ServiceAccountKeyId", "test-key-id");
    }

    /// <summary>
    /// Creates a mock private key file for Zitadel configuration validation.
    /// The file is created once and reused across all tests.
    /// </summary>
    private static string CreateMockPrivateKeyFile()
    {
        if (_mockPrivateKeyPath != null && File.Exists(_mockPrivateKeyPath))
        {
            return _mockPrivateKeyPath;
        }

        // Create a temporary file with mock PEM content
        _mockPrivateKeyPath = Path.Combine(Path.GetTempPath(), $"mock-zitadel-key-{Guid.NewGuid()}.pem");

        // Write a mock RSA private key in PEM format
        // This is NOT a real key - just enough to pass file existence validation
        var mockPemContent = @"-----BEGIN RSA PRIVATE KEY-----
MIIEpAIBAAKCAQEAtest1234567890test1234567890test1234567890test1234
567890test1234567890test1234567890test1234567890test1234567890test12
34567890test1234567890test1234567890test1234567890test1234567890test
1234567890test1234567890test1234567890test1234567890test1234567890te
st1234567890test1234567890test1234567890test1234567890AgMBAAECggEAAA
AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAECgYEAtest12
34567890test1234567890test1234567890test1234567890test1234567890test
1234567890test1234567890test1234567890test1234567890ECgYEAtest123456
7890test1234567890test1234567890test1234567890test1234567890test1234
567890test1234567890test1234567890test1234567890=
-----END RSA PRIVATE KEY-----";

        File.WriteAllText(_mockPrivateKeyPath, mockPemContent);

        return _mockPrivateKeyPath;
    }
}
