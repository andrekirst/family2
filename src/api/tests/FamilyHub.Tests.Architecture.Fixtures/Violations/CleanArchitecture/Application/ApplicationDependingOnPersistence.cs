using FamilyHub.Tests.Architecture.Fixtures.Violations.CleanArchitecture.Persistence;

namespace FamilyHub.Tests.Architecture.Fixtures.Violations.CleanArchitecture.Application;

/// <summary>
/// INTENTIONAL VIOLATION: Application class depending directly on Persistence implementations.
/// Used for negative testing of CleanArchitectureTests.ApplicationLayer_ShouldNotDependOn_PersistenceImplementations
/// </summary>
public sealed class ApplicationDependingOnPersistence
{
    private readonly PersistenceService _service;

    public ApplicationDependingOnPersistence(PersistenceService service)
    {
        _service = service;
    }

    public void DoWork() => _service.SaveData();
}
