namespace FamilyHub.Api.Common.Widgets;

public sealed class WidgetRegistryInitializer(
    IWidgetRegistry registry,
    IEnumerable<IWidgetProvider> providers) : IHostedService
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
