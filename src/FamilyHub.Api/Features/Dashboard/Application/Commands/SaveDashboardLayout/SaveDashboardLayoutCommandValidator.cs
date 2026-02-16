using FluentValidation;

namespace FamilyHub.Api.Features.Dashboard.Application.Commands.SaveDashboardLayout;

public sealed class SaveDashboardLayoutCommandValidator : AbstractValidator<SaveDashboardLayoutCommand>
{
    public SaveDashboardLayoutCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotNull()
            .WithMessage("Dashboard layout name is required");

        RuleFor(x => x.Widgets)
            .NotNull()
            .WithMessage("Widgets list is required");
    }
}
