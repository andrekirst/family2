namespace FamilyHub.Api.Common.Search;

public sealed class CommandPaletteRegistryInitializer(
    ICommandPaletteRegistry registry,
    IEnumerable<ICommandPaletteProvider> providers) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        foreach (var provider in providers)
        {
            registry.RegisterProvider(provider);
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
