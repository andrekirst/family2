# Standards for Selectable Seed Data

The following standards apply to this work.

---

## backend/graphql-input-command

GraphQL Input->Command Pattern. Separate Input DTOs (primitives) from Commands (Vogen types). See ADR-003.

**How it applies:** Dev GraphQL types follow the `[ExtendObjectType]` namespace pattern (`DevQuery`, `DevMutation` extend `RootQuery`/`RootMutation`). However, since FamilyHub.Dev is a separate assembly and Mediator source generator is per-assembly, the dev resolvers call `SeedDataApplier` directly instead of dispatching through `ICommandBus`. Input DTOs (`ApplySeedDataRequest`) still use primitives; the service layer handles conversion.

Key pattern:

```csharp
[ExtendObjectType(typeof(DevMutation))]
public class ApplySeedDataMutationType
{
    public async Task<ApplySeedDataResult> ApplySeedData(
        ApplySeedDataRequest input,
        [Service] SeedDataApplier applier,
        CancellationToken ct)
    {
        return await applier.ApplyAsync(input.ScenarioId, input.DatasetIds, input.Mode, ct);
    }
}
```

---

## frontend/angular-components

All components are standalone (no NgModules). Use atomic design hierarchy.

**How it applies:** `DevSeedPanelComponent` is a standalone component with inline template, using `inject()` for DI and Angular Signals for state management (`isExpanded`, `isLoading`, `scenarios`, `datasets`, `selectedScenarioId`, `selectedDatasetIds`, `seedMode`, `resultMessage`).

Key patterns:

- `standalone: true` with explicit `imports` array
- `inject(Apollo)` for GraphQL client
- `signal()` for all reactive state
- `@if (condition) { ... }` control flow for conditional rendering

---

## frontend/apollo-graphql

Use Apollo Client for GraphQL with typed operations.

**How it applies:** GraphQL operations defined in `dev-seed.operations.ts` using `gql` tagged templates. The component uses `this.apollo.query()` and `this.apollo.mutate()` with `catchError()` for graceful degradation (if the dev query fails in production, the panel auto-hides).

Key patterns:

- `gql` template literals for queries and mutations
- `inject(Apollo)` for dependency injection
- `catchError(() => of(null))` for production fallback
- `fetchPolicy: 'network-only'` to always get fresh scenario data

---

## testing/unit-testing

xUnit + FluentAssertions with fake repository pattern.

**How it applies:** `FamilyHub.Dev.Tests` project tests the seed data infrastructure:

- **SeedDataRegistry:** Topological sort, circular dependency detection, lookup by ID
- **SeedDataApplier:** Wipe vs additive modes, dataset ordering, environment guard
- **Individual datasets:** Entity creation count, factory method usage, domain event clearing

Since seed data operates on `AppDbContext` directly (not through Mediator handlers), tests either use in-memory SQLite for integration-style tests or mock the DbContext for unit tests.

Key patterns:

- FluentAssertions for all assertions
- Arrange-Act-Assert pattern
- Test entity counts and properties after seeding
