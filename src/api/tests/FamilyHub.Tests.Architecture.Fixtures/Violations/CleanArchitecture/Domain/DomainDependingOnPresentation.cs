using FamilyHub.Tests.Architecture.Fixtures.Violations.CleanArchitecture.Presentation;

namespace FamilyHub.Tests.Architecture.Fixtures.Violations.CleanArchitecture.Domain;

/// <summary>
/// INTENTIONAL VIOLATION: Domain class depending on Presentation layer.
/// Used for negative testing of CleanArchitectureTests.DomainLayer_ShouldNotDependOn_PresentationLayer
/// </summary>
public sealed class DomainDependingOnPresentation
{
    private readonly PresentationController _controller;

    public DomainDependingOnPresentation(PresentationController controller)
    {
        _controller = controller;
    }

    public void DoWork() => _controller.HandleRequest();
}
