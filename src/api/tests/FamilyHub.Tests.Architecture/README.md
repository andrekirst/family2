# FamilyHub Architecture Tests

Architecture tests using [NetArchTest](https://github.com/BenMorris/NetArchTest) to enforce
architectural rules and coding conventions in the FamilyHub modular monolith.

## Overview

These tests run as part of CI/CD and **block PRs that violate architectural rules**.

## Test Summary

| Category | Positive Tests | Negative Tests | Total |
|----------|----------------|----------------|-------|
| Clean Architecture | 5 | 5 | 10 |
| Module Boundary | 4 | 4 | 8 |
| DDD Patterns | 4 | 4 | 8 |
| CQRS Patterns | 4 | 4 | 8 |
| Naming Conventions | 6 | 6 | 12 |
| Exception Tests | - | 5 | 5 |
| **Total** | **23** | **28** | **51** |

> **Note:** Additional tests from parameterized tests (Theory) increase runtime count to ~57.

## Test Categories

### 1. Module Boundary Tests (`ModuleBoundaryTests.cs`)

Ensures proper isolation between bounded contexts:

| Test | Purpose |
|------|---------|
| `FamilyModule_ShouldNotHaveDirectDependencyOn_AuthModuleDomain` | Family cannot depend on Auth internals |
| `AuthModule_DomainLayer_ShouldNotDependOn_FamilyModuleDomain` | Auth cannot depend on Family internals |
| `SharedKernel_ShouldNotDependOn_AnyModule` | SharedKernel has no module dependencies |
| `Modules_CanDependOn_SharedKernel` | Modules can use SharedKernel (positive test) |

### 2. Clean Architecture Tests (`CleanArchitectureTests.cs`)

Enforces layered architecture dependencies:

| Test | Purpose |
|------|---------|
| `DomainLayer_ShouldNotDependOn_ApplicationLayer` | Domain has no outward dependencies |
| `DomainLayer_ShouldNotDependOn_PersistenceLayer` | Domain is persistence-ignorant |
| `DomainLayer_ShouldNotDependOn_PresentationLayer` | Domain is delivery-mechanism agnostic |
| `ApplicationLayer_ShouldNotDependOn_PresentationLayer` | Use cases don't know about GraphQL |
| `ApplicationLayer_ShouldNotDependOn_PersistenceImplementations` | Depends on interfaces only |

**Layer Hierarchy:**

```
Presentation -> Application -> Domain <- Persistence
                    |
              SharedKernel
```

### 3. DDD Pattern Tests (`DddPatternTests.cs`)

Validates Domain-Driven Design patterns:

| Test | Purpose |
|------|---------|
| `AggregateRoots_ShouldInheritFrom_AggregateRootBase` | Aggregates use proper base class |
| `DomainEvents_ShouldInheritFrom_DomainEventBase` | Events use proper infrastructure |
| `RepositoryInterfaces_ShouldResideIn_DomainRepositoriesNamespace` | Interfaces in domain |
| `RepositoryImplementations_ShouldResideIn_PersistenceLayer` | Implementations in persistence |

### 4. CQRS Pattern Tests (`CqrsPatternTests.cs`)

Validates Command Query Responsibility Segregation with MediatR:

| Test | Purpose |
|------|---------|
| `Commands_ShouldImplement_IRequest` | Commands are MediatR requests |
| `Queries_ShouldImplement_IRequest` | Queries are MediatR requests |
| `CommandHandlers_ShouldImplement_IRequestHandler` | Handlers follow pattern |
| `Validators_ShouldInheritFrom_AbstractValidator` | FluentValidation is used |

### 5. Naming Convention Tests (`NamingConventionTests.cs`)

Enforces consistent naming:

| Test | Purpose |
|------|---------|
| `Interfaces_ShouldStartWith_I` | Standard .NET convention |
| `Commands_ShouldEndWith_Command` | CQRS command naming |
| `Queries_ShouldEndWith_Query` | CQRS query naming |
| `GraphQLInputs_ShouldEndWith_Input` | Hot Chocolate inputs |
| `DomainEvents_ShouldEndWith_Event` | Domain event naming |
| `GraphQLPayloads_ShouldEndWith_Payload` | Mutation result naming |

## Running Tests

```bash
# Run all architecture tests
dotnet test src/api/tests/FamilyHub.Tests.Architecture

# Run with verbose output
dotnet test src/api/tests/FamilyHub.Tests.Architecture --verbosity detailed

# Run specific test class
dotnet test src/api/tests/FamilyHub.Tests.Architecture --filter "FullyQualifiedName~ModuleBoundaryTests"
```

## Adding New Rules

1. Identify the architectural constraint to enforce
2. Choose appropriate test class (or create new one)
3. Write test using NetArchTest fluent API:

   ```csharp
   var result = Types.InAssembly(assembly)
       .That()
       .ResideInNamespace("SomeNamespace")
       .ShouldNot()
       .HaveDependencyOn("AnotherNamespace")
       .GetResult();

   result.IsSuccessful.Should().BeTrue(
       because: $"Description. Failing types: {string.Join(", ", result.FailingTypeNames ?? [])}");
   ```

4. Document rule in this README
5. Run tests locally before committing

## Negative Tests (Violation Fixtures)

Negative tests verify that architecture rules actually detect violations. They use a dedicated
fixtures project (`FamilyHub.Tests.Architecture.Fixtures`) containing intentionally violating types.

### Fixtures Project Structure

```
FamilyHub.Tests.Architecture.Fixtures/
└── Violations/
    ├── CleanArchitecture/     # Layer dependency violations
    ├── ModuleBoundary/        # Cross-module dependency violations
    ├── DddPatterns/           # DDD pattern violations
    ├── CqrsPatterns/          # MediatR pattern violations
    └── NamingConventions/     # Naming rule violations
```

### Negative Test Classes

| Class | Purpose |
|-------|---------|
| `CleanArchitectureNegativeTests.cs` | Verifies layer dependency detection |
| `ModuleBoundaryNegativeTests.cs` | Verifies cross-module detection |
| `DddPatternNegativeTests.cs` | Verifies DDD pattern detection |
| `CqrsPatternNegativeTests.cs` | Verifies CQRS pattern detection |
| `NamingConventionNegativeTests.cs` | Verifies naming rule detection |

## Documented Exceptions (Exception Registry)

Cross-module dependencies are tracked in `Helpers/ExceptionRegistry.cs` with metadata:

- **Phase**: When the exception was introduced
- **Reason**: Why the dependency exists
- **Ticket**: Reference to ADR or issue
- **PlannedRemoval**: When to resolve

### Current Phase 3 Exceptions (20 types)

The Auth module temporarily hosts Family module functionality during Phase 3 extraction:

| Layer | Count | Examples |
|-------|-------|----------|
| Domain | 1 | `User.GetRoleInFamily()` |
| Application | 12 | Command handlers, query handlers |
| Presentation | 6 | GraphQL type extensions, mappers |
| Infrastructure | 1 | Background jobs |

See `ExceptionRegistry.cs` for full details.

### Exception Tests (`ModuleBoundaryExceptionTests.cs`)

| Test | Purpose |
|------|---------|
| `DocumentedExceptions_ShouldStillViolate_OrBeRemoved` | Ensures exceptions are removed when fixed |
| `NewViolations_ShouldBeDocumented_OrFail` | Detects undocumented violations |
| `ExceptionRegistry_ShouldHaveCompleteMetadata` | Validates exception metadata |
| `FamilyModule_ShouldNotHaveUndocumentedDependenciesOn_AuthModule` | Family → Auth check |
| `SharedKernel_ShouldHaveNoUndocumentedModuleDependencies` | SharedKernel purity |

### Phase 4+ Features (Future)

The following cross-module dependencies are planned for Phase 4+:

- Event chain orchestration between modules
- Cross-module read models for reporting
- Saga coordinators for distributed transactions

## CI/CD Integration

Architecture tests run in the GitHub Actions CI pipeline after unit tests:

```yaml
- name: Architecture Tests
  run: dotnet test src/api/tests/FamilyHub.Tests.Architecture --no-build --verbosity normal
```

**Any architecture violation will block PR merge.**

## Related Documentation

- [ADR-001: Modular Monolith First](../../../docs/architecture/ADR-001-MODULAR-MONOLITH-FIRST.md)
- [ADR-003: GraphQL Input-Command Pattern](../../../docs/architecture/ADR-003-GRAPHQL-INPUT-COMMAND-PATTERN.md)
- [Module Extraction Guide](../../../docs/development/MODULE_EXTRACTION_QUICKSTART.md)
- [Coding Standards](../../../docs/development/CODING_STANDARDS.md)

## Troubleshooting

### Test Failures

1. Read the assertion message - it includes failing type names
2. Check if the dependency is intentional
3. If intentional, document as exception and update test
4. If unintentional, refactor to remove the dependency

### Common Issues

| Issue | Solution |
|-------|----------|
| "Types.InAssembly returned empty" | Ensure project references are correct |
| "Namespace not found" | Check namespace spelling in TestConstants.cs |
| "Assembly load failed" | Verify all projects build successfully first |

### Getting Help

- Check existing tests for patterns
- Review NetArchTest documentation: <https://github.com/BenMorris/NetArchTest>
- Ask in team chat or create a GitHub issue
