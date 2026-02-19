using Microsoft.Extensions.Configuration;

namespace FamilyHub.Api.Common.Infrastructure.Configuration.Infisical;

public sealed class InfisicalConfigurationSource(InfisicalOptions options) : IConfigurationSource
{
    internal InfisicalOptions Options { get; } = options;

    public IConfigurationProvider Build(IConfigurationBuilder builder) =>
        new InfisicalConfigurationProvider(Options);
}
