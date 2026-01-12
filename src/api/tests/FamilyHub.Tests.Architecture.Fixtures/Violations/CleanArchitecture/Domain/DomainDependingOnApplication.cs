using FamilyHub.Tests.Architecture.Fixtures.Violations.CleanArchitecture.Application;

namespace FamilyHub.Tests.Architecture.Fixtures.Violations.CleanArchitecture.Domain;

/// <summary>
/// INTENTIONAL VIOLATION: Domain class depending on Application layer.
/// Used for negative testing of CleanArchitectureTests.DomainLayer_ShouldNotDependOn_ApplicationLayer
/// </summary>
public sealed class DomainDependingOnApplication
{
    private readonly ApplicationService _service;

    public DomainDependingOnApplication(ApplicationService service)
    {
        _service = service;
    }

    public void DoWork() => _service.Execute();
}
