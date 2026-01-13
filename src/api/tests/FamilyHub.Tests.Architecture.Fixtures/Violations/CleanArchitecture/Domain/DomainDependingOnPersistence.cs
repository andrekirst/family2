using FamilyHub.Tests.Architecture.Fixtures.Violations.CleanArchitecture.Persistence;

namespace FamilyHub.Tests.Architecture.Fixtures.Violations.CleanArchitecture.Domain;

/// <summary>
/// INTENTIONAL VIOLATION: Domain class depending on Persistence layer.
/// Used for negative testing of CleanArchitectureTests.DomainLayer_ShouldNotDependOn_PersistenceLayer
/// </summary>
public sealed class DomainDependingOnPersistence(PersistenceService service)
{
    public void DoWork() => service.SaveData();
}
