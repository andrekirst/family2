using FamilyHub.Api.Common.Infrastructure.Validation;
using FluentValidation;

namespace FamilyHub.Api.Features.Dashboard.Application.Commands.AddWidget;

public sealed class AddWidgetCommandValidator : AbstractValidator<AddWidgetCommand>, IInputValidator<AddWidgetCommand>
{
    public AddWidgetCommandValidator()
    {
        RuleFor(x => x.DashboardId).NotNull().WithMessage("Dashboard ID is required");
        RuleFor(x => x.WidgetType).NotNull().WithMessage("Widget type is required");
        RuleFor(x => x.Width).GreaterThan(0).WithMessage("Width must be positive");
        RuleFor(x => x.Height).GreaterThan(0).WithMessage("Height must be positive");
    }
}
