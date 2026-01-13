namespace FamilyHub.Tests.Architecture.Fixtures.Violations.DddPatterns.Domain;

/// <summary>
/// INTENTIONAL VIOLATION: Repository implementation in Domain namespace instead of Persistence.
/// Used for negative testing of DddPatternTests.RepositoryImplementations_ShouldResideIn_PersistenceLayer
/// </summary>
public sealed class BadRepository
{
    public void GetById(Guid id) { }
}
