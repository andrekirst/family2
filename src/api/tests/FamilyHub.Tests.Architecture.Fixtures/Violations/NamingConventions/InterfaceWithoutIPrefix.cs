namespace FamilyHub.Tests.Architecture.Fixtures.Violations.NamingConventions;

/// <summary>
/// INTENTIONAL VIOLATION: Interface without 'I' prefix.
/// Used for negative testing of NamingConventionTests.Interfaces_ShouldStartWith_I
/// </summary>
public interface BadlyNamedService
{
    void DoSomething();
}
