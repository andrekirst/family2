using FamilyHub.Tests.Architecture.Fixtures.Violations.ModuleBoundary.Family;

namespace FamilyHub.Tests.Architecture.Fixtures.Violations.ModuleBoundary.Auth;

/// <summary>
/// INTENTIONAL VIOLATION: Auth module type depending on Family module.
/// Used for negative testing of ModuleBoundaryTests.AuthModule_DomainLayer_ShouldNotDependOn_FamilyModuleDomain
/// </summary>
public sealed class AuthViolatingFamily
{
    private readonly FamilyModuleType _familyType;

    public AuthViolatingFamily(FamilyModuleType familyType)
    {
        _familyType = familyType;
    }

    public void DoWork() => _familyType.FamilyOperation();
}
