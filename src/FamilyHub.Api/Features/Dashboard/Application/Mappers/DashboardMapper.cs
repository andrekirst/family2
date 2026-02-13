using FamilyHub.Api.Common.Widgets;
using FamilyHub.Api.Features.Dashboard.Domain.Entities;
using FamilyHub.Api.Features.Dashboard.Models;

namespace FamilyHub.Api.Features.Dashboard.Application.Mappers;

public static class DashboardMapper
{
    public static DashboardLayoutDto ToDto(DashboardLayout layout)
    {
        return new DashboardLayoutDto
        {
            Id = layout.Id.Value,
            Name = layout.Name.Value,
            IsShared = layout.IsShared,
            CreatedAt = layout.CreatedAt,
            UpdatedAt = layout.UpdatedAt,
            Widgets = layout.Widgets.Select(ToDto).ToList()
        };
    }

    public static DashboardWidgetDto ToDto(DashboardWidget widget)
    {
        return new DashboardWidgetDto
        {
            Id = widget.Id.Value,
            WidgetType = widget.WidgetType.Value,
            X = widget.X,
            Y = widget.Y,
            Width = widget.Width,
            Height = widget.Height,
            SortOrder = widget.SortOrder,
            ConfigJson = widget.ConfigJson
        };
    }

    public static WidgetDescriptorDto ToDto(WidgetDescriptor descriptor)
    {
        return new WidgetDescriptorDto
        {
            WidgetTypeId = descriptor.WidgetTypeId,
            Module = descriptor.Module,
            Name = descriptor.Name,
            Description = descriptor.Description,
            DefaultWidth = descriptor.DefaultWidth,
            DefaultHeight = descriptor.DefaultHeight,
            MinWidth = descriptor.MinWidth,
            MinHeight = descriptor.MinHeight,
            MaxWidth = descriptor.MaxWidth,
            MaxHeight = descriptor.MaxHeight,
            ConfigSchema = descriptor.ConfigSchema,
            RequiredPermissions = descriptor.RequiredPermissions.ToList()
        };
    }
}
