namespace FamilyHub.Tests.Architecture.Fixtures.Violations.NamingConventions;

/// <summary>
/// INTENTIONAL VIOLATION: Interface without 'I' prefix.
/// Used for negative testing of NamingConventionTests.Interfaces_ShouldStartWith_I
/// </summary>
#pragma warning disable IDE1006 // Naming rule violation - intentional for testing
public interface BadlyNamedService
#pragma warning restore IDE1006
{
    void DoSomething();
}
