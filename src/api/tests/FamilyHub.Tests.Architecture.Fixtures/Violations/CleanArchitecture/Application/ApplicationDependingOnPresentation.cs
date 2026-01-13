using FamilyHub.Tests.Architecture.Fixtures.Violations.CleanArchitecture.Presentation;

namespace FamilyHub.Tests.Architecture.Fixtures.Violations.CleanArchitecture.Application;

/// <summary>
/// INTENTIONAL VIOLATION: Application class depending on Presentation layer.
/// Used for negative testing of CleanArchitectureTests.ApplicationLayer_ShouldNotDependOn_PresentationLayer
/// </summary>
public sealed class ApplicationDependingOnPresentation(PresentationController controller)
{
    public void DoWork() => controller.HandleRequest();
}
