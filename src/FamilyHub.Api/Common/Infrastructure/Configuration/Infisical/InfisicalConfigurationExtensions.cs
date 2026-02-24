using Microsoft.Extensions.Configuration;

namespace FamilyHub.Api.Common.Infrastructure.Configuration.Infisical;

public static class InfisicalConfigurationExtensions
{
    public static IConfigurationBuilder AddInfisical(this IConfigurationBuilder builder)
    {
        // Bootstrap credentials come from environment variables (not IConfiguration)
        // to avoid circular dependency — this is the "secret zero" pattern.
        var clientId = System.Environment.GetEnvironmentVariable("INFISICAL_CLIENT_ID");
        var clientSecret = System.Environment.GetEnvironmentVariable("INFISICAL_CLIENT_SECRET");

        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            return builder; // Infisical not configured — skip silently

        var options = new InfisicalOptions
        {
            Url = System.Environment.GetEnvironmentVariable("INFISICAL_URL") ?? "http://localhost:8180",
            ProjectId = System.Environment.GetEnvironmentVariable("INFISICAL_PROJECT_ID") ?? "",
            Environment = System.Environment.GetEnvironmentVariable("INFISICAL_ENVIRONMENT") ?? "dev",
            ClientId = clientId,
            ClientSecret = clientSecret,
        };

        return builder.Add(new InfisicalConfigurationSource(options));
    }
}
