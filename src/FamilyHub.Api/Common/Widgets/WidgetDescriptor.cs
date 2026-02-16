namespace FamilyHub.Api.Common.Widgets;

public sealed record WidgetDescriptor(
    string WidgetTypeId,
    string Module,
    string Name,
    string Description,
    int DefaultWidth,
    int DefaultHeight,
    int MinWidth,
    int MinHeight,
    int MaxWidth,
    int MaxHeight,
    string? ConfigSchema,
    IReadOnlyList<string> RequiredPermissions);
