using FamilyHub.Tests.Architecture.Fixtures.Violations.ModuleBoundary.Auth;

namespace FamilyHub.Tests.Architecture.Fixtures.Violations.ModuleBoundary.Family;

/// <summary>
/// INTENTIONAL VIOLATION: Family module type depending on Auth module.
/// Used for negative testing of ModuleBoundaryTests.FamilyModule_ShouldNotHaveDirectDependencyOn_AuthModuleDomain
/// </summary>
public sealed class FamilyViolatingAuth
{
    private readonly AuthModuleType _authType;

    public FamilyViolatingAuth(AuthModuleType authType)
    {
        _authType = authType;
    }

    public void DoWork() => _authType.AuthOperation();
}
