using FamilyHub.Tests.Architecture.Fixtures.Violations.CleanArchitecture.Presentation;

namespace FamilyHub.Tests.Architecture.Fixtures.Violations.CleanArchitecture.Application;

/// <summary>
/// INTENTIONAL VIOLATION: Application class depending on Presentation layer.
/// Used for negative testing of CleanArchitectureTests.ApplicationLayer_ShouldNotDependOn_PresentationLayer
/// </summary>
public sealed class ApplicationDependingOnPresentation
{
    private readonly PresentationController _controller;

    public ApplicationDependingOnPresentation(PresentationController controller)
    {
        _controller = controller;
    }

    public void DoWork() => _controller.HandleRequest();
}
