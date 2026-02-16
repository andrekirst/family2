namespace FamilyHub.Api.Common.Widgets;

public interface IWidgetRegistry
{
    void RegisterProvider(IWidgetProvider provider);
    IReadOnlyList<WidgetDescriptor> GetAllWidgets();
    IReadOnlyList<WidgetDescriptor> GetWidgetsByModule(string moduleName);
    WidgetDescriptor? GetWidget(string widgetTypeId);
    bool IsValidWidget(string widgetTypeId);
}
