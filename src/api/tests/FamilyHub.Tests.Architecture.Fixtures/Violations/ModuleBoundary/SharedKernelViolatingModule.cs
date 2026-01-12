using FamilyHub.Tests.Architecture.Fixtures.Violations.ModuleBoundary.Auth;

namespace FamilyHub.Tests.Architecture.Fixtures.Violations.ModuleBoundary.SharedKernel;

/// <summary>
/// INTENTIONAL VIOLATION: SharedKernel type depending on a module.
/// Used for negative testing of ModuleBoundaryTests.SharedKernel_ShouldNotDependOn_AnyModule
/// </summary>
public sealed class SharedKernelViolatingModule
{
    private readonly AuthModuleType _authType;

    public SharedKernelViolatingModule(AuthModuleType authType)
    {
        _authType = authType;
    }

    public void DoWork() => _authType.AuthOperation();
}
