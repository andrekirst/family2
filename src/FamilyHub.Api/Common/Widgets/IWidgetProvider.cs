namespace FamilyHub.Api.Common.Widgets;

public interface IWidgetProvider
{
    string ModuleName { get; }
    IReadOnlyList<WidgetDescriptor> GetWidgets();
}
