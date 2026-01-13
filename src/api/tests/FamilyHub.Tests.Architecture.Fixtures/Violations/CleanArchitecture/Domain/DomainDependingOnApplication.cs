using FamilyHub.Tests.Architecture.Fixtures.Violations.CleanArchitecture.Application;

namespace FamilyHub.Tests.Architecture.Fixtures.Violations.CleanArchitecture.Domain;

/// <summary>
/// INTENTIONAL VIOLATION: Domain class depending on Application layer.
/// Used for negative testing of CleanArchitectureTests.DomainLayer_ShouldNotDependOn_ApplicationLayer
/// </summary>
public sealed class DomainDependingOnApplication(ApplicationService service)
{
    public void DoWork() => service.Execute();
}
