namespace FamilyHub.Tests.Architecture.Fixtures.Violations.DddPatterns.Application;

/// <summary>
/// INTENTIONAL VIOLATION: Repository interface in Application namespace instead of Domain.Repositories.
/// Used for negative testing of DddPatternTests.RepositoryInterfaces_ShouldResideIn_DomainRepositoriesNamespace
/// </summary>
public interface IBadRepository
{
    void GetById(Guid id);
}
