namespace FamilyHub.Api.Common.Widgets;

public sealed class WidgetRegistry : IWidgetRegistry
{
    private readonly List<WidgetDescriptor> _widgets = [];

    public void RegisterProvider(IWidgetProvider provider)
    {
        _widgets.AddRange(provider.GetWidgets());
    }

    public IReadOnlyList<WidgetDescriptor> GetAllWidgets() =>
        _widgets.AsReadOnly();

    public IReadOnlyList<WidgetDescriptor> GetWidgetsByModule(string moduleName) =>
        _widgets.Where(w => w.Module == moduleName).ToList().AsReadOnly();

    public WidgetDescriptor? GetWidget(string widgetTypeId) =>
        _widgets.FirstOrDefault(w => w.WidgetTypeId == widgetTypeId);

    public bool IsValidWidget(string widgetTypeId) =>
        _widgets.Any(w => w.WidgetTypeId == widgetTypeId);
}
