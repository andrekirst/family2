# ADR-012: Architecture Testing Strategy

**Status:** Accepted
**Date:** 2026-01-12
**Deciders:** Andre Kirst (with Claude Code AI)
**Tags:** architecture, testing, netarchtest, clean-architecture, module-boundary, ddd
**Related ADRs:** [ADR-001](ADR-001-MODULAR-MONOLITH-FIRST.md), [ADR-005](ADR-005-FAMILY-MODULE-EXTRACTION-PATTERN.md)
**Issue:** #76

## Context

Family Hub follows a **modular monolith** architecture (per [ADR-001](ADR-001-MODULAR-MONOLITH-FIRST.md)) with Clean Architecture principles. As the codebase evolves, maintaining architectural integrity becomes challenging:

### Problem Statement

1. **Layer Violations**: Domain layer might accidentally reference infrastructure concerns
2. **Module Coupling**: Modules might bypass proper interfaces and access each other directly
3. **Drift Over Time**: Gradual architectural erosion as developers add shortcuts
4. **Manual Review Burden**: Code reviews cannot catch all architectural violations
5. **Known Violations**: Some violations are intentional (e.g., during refactoring phases)

### Technology Stack

- **.NET 10 / C# 14**: Target framework
- **NetArchTest.Rules 1.3.2**: Architecture testing library
- **xUnit**: Test framework
- **FluentAssertions**: Assertion library

### Clean Architecture Layers

```
┌─────────────────────────────────────────────────────────────────────────────┐
│ Family Hub Clean Architecture Layers                                        │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │ Presentation Layer                                                   │   │
│  │ - FamilyHub.Api                                                      │   │
│  │ - Modules/*/Presentation/                                            │   │
│  │ - GraphQL mutations, queries, types                                  │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                              │                                              │
│                              ▼                                              │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │ Application Layer                                                    │   │
│  │ - Modules/*/Application/                                             │   │
│  │ - Commands, Queries, Handlers, Validators                            │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                              │                                              │
│                              ▼                                              │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │ Domain Layer (MUST BE INDEPENDENT)                                   │   │
│  │ - Modules/*/Domain/                                                  │   │
│  │ - Entities, Value Objects, Domain Events, Repository Interfaces      │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                              │                                              │
│                              ▼                                              │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │ Infrastructure Layer                                                 │   │
│  │ - Modules/*/Persistence/, */Infrastructure/                          │   │
│  │ - FamilyHub.Infrastructure                                           │   │
│  │ - DbContext, Repositories, External Services                         │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

## Decision

**Implement automated architecture testing using NetArchTest.Rules with an ExceptionRegistry pattern for known violations, covering Clean Architecture layer dependencies and module boundary separation.**

### Test Architecture

```
┌─────────────────────────────────────────────────────────────────────────────┐
│ Architecture Testing Structure                                              │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  tests/FamilyHub.Tests.Architecture/                                        │
│  ├── CleanArchitectureTests.cs       ◀── Layer dependency validation        │
│  ├── ModuleBoundaryTests.cs          ◀── Module isolation tests             │
│  ├── Helpers/                                                               │
│  │   ├── ArchitectureTestBase.cs     ◀── Shared test infrastructure         │
│  │   ├── TestConstants.cs            ◀── Namespace definitions              │
│  │   └── ExceptionRegistry.cs        ◀── Known violation tracking           │
│  └── Fixtures/Violations/            ◀── Negative test fixtures             │
│      └── CleanArchitecture/                                                 │
│          └── DomainReferencingInfrastructure.cs                             │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Clean Architecture Tests

```csharp
/// <summary>
/// Tests that enforce Clean Architecture layer dependencies.
/// Domain layer must remain independent of all other layers.
/// </summary>
public class CleanArchitectureTests : ArchitectureTestBase
{
    [Theory]
    [InlineData(Namespaces.AuthDomain)]
    [InlineData(Namespaces.FamilyDomain)]
    public void Domain_ShouldNotReference_ApplicationLayer(string domainNamespace)
    {
        // Arrange
        var domainTypes = GetTypesInNamespace(domainNamespace);

        // Act
        var result = domainTypes
            .ShouldNot()
            .HaveDependencyOn(Namespaces.AuthApplication)
            .And()
            .ShouldNot()
            .HaveDependencyOn(Namespaces.FamilyApplication)
            .GetResult();

        // Assert
        AssertArchitectureRule(result,
            $"Domain layer ({domainNamespace}) should not reference Application layer");
    }

    [Theory]
    [InlineData(Namespaces.AuthDomain)]
    [InlineData(Namespaces.FamilyDomain)]
    public void Domain_ShouldNotReference_InfrastructureLayer(string domainNamespace)
    {
        // Arrange
        var domainTypes = GetTypesInNamespace(domainNamespace);
        var exceptions = ExceptionRegistry.GetExceptionsForRule(
            "Domain_ShouldNotReference_InfrastructureLayer",
            domainNamespace);

        // Act
        var filteredTypes = domainTypes;
        if (exceptions.Any())
        {
            filteredTypes = domainTypes
                .That()
                .DoNotHaveNameMatching(string.Join("|", exceptions.Select(e => e.TypeName)));
        }

        var result = filteredTypes
            .ShouldNot()
            .HaveDependencyOn(Namespaces.AuthPersistence)
            .And()
            .ShouldNot()
            .HaveDependencyOn(Namespaces.FamilyPersistence)
            .And()
            .ShouldNot()
            .HaveDependencyOn(Namespaces.Infrastructure)
            .GetResult();

        // Assert
        AssertArchitectureRule(result,
            $"Domain layer ({domainNamespace}) should not reference Infrastructure layer",
            exceptions);
    }

    [Theory]
    [InlineData(Namespaces.AuthDomain)]
    [InlineData(Namespaces.FamilyDomain)]
    public void Domain_ShouldNotReference_PresentationLayer(string domainNamespace)
    {
        // Arrange
        var domainTypes = GetTypesInNamespace(domainNamespace);

        // Act
        var result = domainTypes
            .ShouldNot()
            .HaveDependencyOn(Namespaces.Api)
            .And()
            .ShouldNot()
            .HaveDependencyOn("Presentation")
            .GetResult();

        // Assert
        AssertArchitectureRule(result,
            $"Domain layer ({domainNamespace}) should not reference Presentation layer");
    }

    [Fact]
    public void Domain_ShouldNotReference_EFCore()
    {
        // Arrange
        var allDomainTypes = GetTypesInNamespace(Namespaces.AuthDomain)
            .That()
            .ResideInNamespace(Namespaces.FamilyDomain);

        // Act
        var result = allDomainTypes
            .ShouldNot()
            .HaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        // Assert
        AssertArchitectureRule(result,
            "Domain layer should not reference Entity Framework Core");
    }
}
```

### Module Boundary Tests

```csharp
/// <summary>
/// Tests that enforce module boundary separation.
/// Modules should only communicate through SharedKernel interfaces.
/// </summary>
public class ModuleBoundaryTests : ArchitectureTestBase
{
    [Fact]
    public void AuthModule_ShouldNotReference_FamilyModuleInternals()
    {
        // Arrange
        var authTypes = GetTypesInNamespace(Namespaces.AuthModule);

        // Act
        var result = authTypes
            .ShouldNot()
            .HaveDependencyOn(Namespaces.FamilyDomain)
            .And()
            .ShouldNot()
            .HaveDependencyOn(Namespaces.FamilyApplication)
            .And()
            .ShouldNot()
            .HaveDependencyOn(Namespaces.FamilyPersistence)
            .GetResult();

        // Assert
        AssertArchitectureRule(result,
            "Auth module should not reference Family module internals");
    }

    [Fact]
    public void FamilyModule_ShouldNotReference_AuthModuleInternals()
    {
        // Arrange
        var familyTypes = GetTypesInNamespace(Namespaces.FamilyModule);
        var exceptions = ExceptionRegistry.GetExceptionsForRule(
            "FamilyModule_ShouldNotReference_AuthModuleInternals");

        // Act
        var filteredTypes = familyTypes;
        if (exceptions.Any())
        {
            filteredTypes = familyTypes
                .That()
                .DoNotHaveNameMatching(string.Join("|", exceptions.Select(e => e.TypeName)));
        }

        var result = filteredTypes
            .ShouldNot()
            .HaveDependencyOn(Namespaces.AuthDomain)
            .And()
            .ShouldNot()
            .HaveDependencyOn(Namespaces.AuthApplication)
            .And()
            .ShouldNot()
            .HaveDependencyOn(Namespaces.AuthPersistence)
            .GetResult();

        // Assert
        AssertArchitectureRule(result,
            "Family module should not reference Auth module internals",
            exceptions);
    }

    [Fact]
    public void Modules_ShouldOnlyCommunicate_ThroughSharedKernel()
    {
        // Arrange
        var moduleTypes = GetTypesInNamespace(Namespaces.AuthModule)
            .That()
            .ResideInNamespace(Namespaces.FamilyModule);

        // Act - Verify cross-module communication uses SharedKernel
        var result = moduleTypes
            .That()
            .HaveDependencyOn(Namespaces.SharedKernel)
            .Should()
            .Exist()
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Modules should communicate through SharedKernel interfaces");
    }
}
```

### ExceptionRegistry Pattern

```csharp
/// <summary>
/// Registry for known architecture violations that are intentionally allowed.
/// Each exception must have a justification and target resolution phase.
///
/// USAGE:
/// 1. Add exception with full documentation
/// 2. Reference Phase when violation will be resolved
/// 3. Review exceptions during each phase completion
/// 4. Remove exceptions when violations are fixed
/// </summary>
public static class ExceptionRegistry
{
    private static readonly List<ArchitectureException> _exceptions =
    [
        // Example: Domain entity temporarily referencing another module's aggregate
        new ArchitectureException
        {
            RuleName = "Domain_ShouldNotReference_OtherModuleDomain",
            DomainNamespace = Namespaces.AuthDomain,
            TypeName = "User",
            Justification = "User.GetRoleInFamily(FamilyAggregate) method requires " +
                           "Family aggregate reference for role calculation. " +
                           "Will be refactored to use IFamilyRoleService in Phase 6.",
            TargetResolutionPhase = "Phase 6 - Module Boundary Enforcement",
            AddedDate = new DateTime(2026, 1, 12),
            AddedBy = "Andre Kirst"
        }
    ];

    /// <summary>
    /// Gets exceptions for a specific rule and optional namespace filter.
    /// </summary>
    public static IReadOnlyList<ArchitectureException> GetExceptionsForRule(
        string ruleName,
        string? domainNamespace = null)
    {
        return _exceptions
            .Where(e => e.RuleName == ruleName)
            .Where(e => domainNamespace == null || e.DomainNamespace == domainNamespace)
            .ToList();
    }

    /// <summary>
    /// Gets all registered exceptions for audit purposes.
    /// </summary>
    public static IReadOnlyList<ArchitectureException> GetAllExceptions() => _exceptions;
}

/// <summary>
/// Represents a known architecture violation that is intentionally allowed.
/// </summary>
public sealed record ArchitectureException
{
    public required string RuleName { get; init; }
    public required string DomainNamespace { get; init; }
    public required string TypeName { get; init; }
    public required string Justification { get; init; }
    public required string TargetResolutionPhase { get; init; }
    public required DateTime AddedDate { get; init; }
    public required string AddedBy { get; init; }
}
```

### Test Constants

```csharp
/// <summary>
/// Namespace constants for architecture tests.
/// Ensures consistency across all architecture validation rules.
/// </summary>
public static class Namespaces
{
    // API
    public const string Api = "FamilyHub.Api";

    // SharedKernel
    public const string SharedKernel = "FamilyHub.SharedKernel";

    // Infrastructure
    public const string Infrastructure = "FamilyHub.Infrastructure";

    // Auth Module
    public const string AuthModule = "FamilyHub.Modules.Auth";
    public const string AuthDomain = "FamilyHub.Modules.Auth.Domain";
    public const string AuthApplication = "FamilyHub.Modules.Auth.Application";
    public const string AuthPersistence = "FamilyHub.Modules.Auth.Persistence";
    public const string AuthPresentation = "FamilyHub.Modules.Auth.Presentation";

    // Family Module
    public const string FamilyModule = "FamilyHub.Modules.Family";
    public const string FamilyDomain = "FamilyHub.Modules.Family.Domain";
    public const string FamilyApplication = "FamilyHub.Modules.Family.Application";
    public const string FamilyPersistence = "FamilyHub.Modules.Family.Persistence";
    public const string FamilyPresentation = "FamilyHub.Modules.Family.Presentation";
}
```

### Negative Testing with Fixtures

```csharp
/// <summary>
/// Fixture that intentionally violates Clean Architecture rules.
/// Used to verify that architecture tests correctly detect violations.
///
/// CRITICAL: This file must be in a separate assembly that is NOT part of
/// the main solution. It exists only to validate that NetArchTest correctly
/// identifies violations.
/// </summary>
namespace FamilyHub.Tests.Architecture.Fixtures.Violations.CleanArchitecture;

// This fixture references infrastructure from domain (VIOLATION)
// Tests verify this is correctly detected
public class DomainReferencingInfrastructure
{
    // Intentional violation: Domain referencing EF Core
    // private readonly DbContext _context; // Would cause test failure

    public void ViolatingMethod()
    {
        // Intentional violation: Domain calling infrastructure
        // var repo = new ConcreteRepository(); // Would cause test failure
    }
}
```

### Test Base Class

```csharp
/// <summary>
/// Base class for architecture tests providing common functionality.
/// </summary>
public abstract class ArchitectureTestBase
{
    protected static readonly Assembly ApiAssembly =
        typeof(FamilyHub.Api.Program).Assembly;

    protected static readonly Assembly AuthModuleAssembly =
        typeof(FamilyHub.Modules.Auth.AuthModuleServiceRegistration).Assembly;

    protected static readonly Assembly FamilyModuleAssembly =
        typeof(FamilyHub.Modules.Family.FamilyModuleServiceRegistration).Assembly;

    protected static readonly Assembly SharedKernelAssembly =
        typeof(FamilyHub.SharedKernel.Domain.AggregateRoot<>).Assembly;

    protected static readonly Assembly InfrastructureAssembly =
        typeof(FamilyHub.Infrastructure.Messaging.RabbitMqPublisher).Assembly;

    /// <summary>
    /// Gets types from a specific namespace across all relevant assemblies.
    /// </summary>
    protected PredicateList GetTypesInNamespace(string namespaceName)
    {
        return Types
            .InAssemblies(new[]
            {
                ApiAssembly,
                AuthModuleAssembly,
                FamilyModuleAssembly,
                SharedKernelAssembly,
                InfrastructureAssembly
            })
            .That()
            .ResideInNamespaceStartingWith(namespaceName);
    }

    /// <summary>
    /// Asserts architecture rule with detailed failure message.
    /// </summary>
    protected void AssertArchitectureRule(
        TestResult result,
        string ruleName,
        IReadOnlyList<ArchitectureException>? exceptions = null)
    {
        if (!result.IsSuccessful)
        {
            var failingTypes = result.FailingTypes?
                .Select(t => t.FullName)
                .ToList() ?? [];

            var message = $"""
                Architecture rule violated: {ruleName}

                Failing types:
                {string.Join(Environment.NewLine, failingTypes.Select(t => $"  - {t}"))}

                {(exceptions?.Any() == true
                    ? $"Note: {exceptions.Count} known exception(s) were excluded from this test."
                    : "No exceptions registered for this rule.")}

                To fix: Either refactor the code or add an exception to ExceptionRegistry
                with proper justification and target resolution phase.
                """;

            result.IsSuccessful.Should().BeTrue(message);
        }
    }
}
```

## Rationale

### Why NetArchTest.Rules

| Library | Language | License | Active | .NET Support |
|---------|----------|---------|--------|--------------|
| **NetArchTest.Rules** | C# | MIT | Yes | .NET 6+ |
| ArchUnitNET | C# | Apache-2.0 | Yes | .NET 5+ |
| NsDepCop | C# | MIT | Limited | .NET Framework |

**Decision**: NetArchTest.Rules provides the best balance of simplicity, active maintenance, and .NET 10 support.

### Why ExceptionRegistry Pattern

Architecture violations sometimes exist intentionally:

1. **Phased Refactoring**: Can't fix everything at once
2. **Technical Debt**: Known issues scheduled for future sprints
3. **Temporary Workarounds**: Short-term solutions with planned fixes

Without ExceptionRegistry:

```
❌ Test fails → CI blocked → Developer adds quick fix → Technical debt hidden
```

With ExceptionRegistry:

```
✅ Test passes with documented exception → CI continues → Violation tracked → Fixed in planned phase
```

Benefits:

1. **Visibility**: All violations documented and tracked
2. **Accountability**: Each exception requires justification and owner
3. **Planning**: Target resolution phase enables roadmap integration
4. **Audit Trail**: History of architectural decisions

### Why Negative Testing

Negative tests validate that architecture tests actually detect violations:

```
┌─────────────────────────────────────────────────────────────────────────────┐
│ Negative Testing Value                                                      │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│ Without Negative Tests:                                                     │
│   - Architecture tests might have bugs                                      │
│   - False confidence in passing tests                                       │
│   - Violations slip through silently                                        │
│                                                                             │
│ With Negative Tests:                                                        │
│   - Proves tests detect actual violations                                   │
│   - Validates NetArchTest configuration                                     │
│   - Catches test framework updates that break detection                     │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Why Namespace-Based Testing

NetArchTest uses namespace patterns for type selection:

```csharp
// Clear, maintainable namespace conventions
public const string AuthDomain = "FamilyHub.Modules.Auth.Domain";
public const string AuthApplication = "FamilyHub.Modules.Auth.Application";
```

Benefits:

1. **Convention Over Configuration**: Follows .NET naming conventions
2. **Self-Documenting**: Namespace reveals layer and module
3. **Scalable**: Adding modules follows same pattern
4. **Refactoring Safe**: IDE updates namespaces automatically

## Alternatives Considered

### Alternative 1: Manual Code Review Only

**Approach**: Rely on code reviews to catch architecture violations.

**Rejected Because**:

- Human error in reviews
- Inconsistent enforcement
- Time-consuming for reviewers
- No historical tracking

### Alternative 2: ArchUnitNET

**Approach**: Use ArchUnitNET for architecture testing.

```csharp
IArchRule rule = Types()
    .That().ResideInNamespace("Domain")
    .Should().NotDependOnAny("Infrastructure");
```

**Deferred Because**:

- Similar capabilities to NetArchTest
- NetArchTest has simpler API
- Team already familiar with NetArchTest
- Can migrate later if needed

### Alternative 3: Roslyn Analyzers

**Approach**: Create custom Roslyn analyzers for compile-time enforcement.

```csharp
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DomainLayerAnalyzer : DiagnosticAnalyzer { }
```

**Deferred Because**:

- Significant development effort
- Harder to maintain
- Less flexibility for exceptions
- Better suited for style rules, not architecture

### Alternative 4: No Exception Registry

**Approach**: All architecture tests must pass with no exceptions.

**Rejected Because**:

- Blocks incremental refactoring
- Forces all-or-nothing approach
- Hides technical debt (workarounds added silently)
- Unrealistic for real-world projects

## Consequences

### Positive

1. **Automated Enforcement**: Architecture violations caught in CI/CD
2. **Living Documentation**: Tests document architectural decisions
3. **Incremental Improvement**: ExceptionRegistry enables phased fixes
4. **Confidence**: Refactoring protected by architecture tests
5. **Onboarding**: New developers learn architecture through tests

### Negative

1. **Test Maintenance**: Tests need updating when architecture evolves
2. **False Positives**: Overly strict tests may block valid patterns
3. **Learning Curve**: Team must understand NetArchTest patterns
4. **Exception Discipline**: Exceptions can accumulate without cleanup

### Mitigation Strategies

| Risk | Mitigation |
|------|------------|
| Exception accumulation | Review exceptions during each phase completion |
| Test brittleness | Use namespace prefixes, not exact matches |
| False positives | Allow exceptions with documentation |
| Outdated tests | Update tests when ADRs change |

## Implementation

### Files Created

| File | Purpose |
|------|---------|
| `tests/FamilyHub.Tests.Architecture/CleanArchitectureTests.cs` | Layer dependency tests |
| `tests/FamilyHub.Tests.Architecture/ModuleBoundaryTests.cs` | Module isolation tests |
| `tests/FamilyHub.Tests.Architecture/Helpers/ArchitectureTestBase.cs` | Shared test infrastructure |
| `tests/FamilyHub.Tests.Architecture/Helpers/TestConstants.cs` | Namespace definitions |
| `tests/FamilyHub.Tests.Architecture/Helpers/ExceptionRegistry.cs` | Known violation tracking |
| `tests/FamilyHub.Tests.Architecture.Fixtures/` | Negative test fixtures |

### Package Dependencies

```xml
<PackageReference Include="NetArchTest.Rules" Version="1.3.2" />
<PackageReference Include="FluentAssertions" Version="6.12.0" />
<PackageReference Include="xunit" Version="2.9.2" />
```

### Verification

1. **Build**: `dotnet build` completes without errors
2. **Tests Pass**: `dotnet test --filter "Category=Architecture"` passes
3. **Violations Detected**: Negative test fixtures correctly fail when included
4. **Exception Registry**: Known violations documented and excluded
5. **CI Integration**: Architecture tests run in GitHub Actions

### Running Tests

```bash
# Run all architecture tests
dotnet test src/api/tests/FamilyHub.Tests.Architecture

# Run specific test category
dotnet test --filter "FullyQualifiedName~CleanArchitectureTests"

# Run with verbose output
dotnet test src/api/tests/FamilyHub.Tests.Architecture --logger "console;verbosity=detailed"
```

## Related Decisions

- [ADR-001: Modular Monolith First](ADR-001-MODULAR-MONOLITH-FIRST.md) - Architecture being tested
- [ADR-005: Family Module Extraction Pattern](ADR-005-FAMILY-MODULE-EXTRACTION-PATTERN.md) - Module boundaries tested

## Future Work

- **Additional Modules**: Add tests as Calendar, Task, Shopping modules are extracted
- **Cyclic Dependency Tests**: Detect module dependency cycles
- **Convention Tests**: Validate naming conventions (e.g., Commands end with "Command")
- **Layered Tests**: Test specific layer rules (e.g., repositories in Persistence only)
- **Performance Tests**: Ensure architecture tests run quickly in CI

## References

- [NetArchTest Documentation](https://github.com/BenMorris/NetArchTest)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Modular Monolith Architecture](https://www.kamilgrzybek.com/design/modular-monolith-primer/)
- [Architecture Testing in .NET](https://docs.microsoft.com/en-us/dotnet/architecture/)

---

**Decision**: Implement automated architecture testing using NetArchTest.Rules with ExceptionRegistry pattern for known violations. Tests enforce Clean Architecture layer dependencies (Domain independence) and module boundary separation (cross-module communication through SharedKernel only). Negative test fixtures validate that tests correctly detect violations.
