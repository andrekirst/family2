# Standards for Dashboard Widget Configuration

The following standards apply to this work.

---

## backend/graphql-input-command

Separate Input DTOs (primitives) from Commands (Vogen). See ADR-003.

- Input DTOs in `Models/` with primitives
- Commands in `Commands/{Name}/` with Vogen types
- One MutationType per command (not centralized)
- Dispatch via `ICommandBus.SendAsync()`

**Relevance**: The `UpdateWidgetConfig` mutation already follows this pattern (`UpdateWidgetConfigRequest` -> `UpdateWidgetConfigCommand`). No new mutations needed.

---

## frontend/angular-components

All components are standalone (no NgModules). Use atomic design hierarchy.

- Always use `standalone: true`
- Import dependencies in `imports` array
- Use Angular Signals for state
- Follow atomic design for organization

**Relevance**: The new `WidgetSettingsPanel` component and modified widget components must use standalone pattern with signals.

---

## frontend/apollo-graphql

Use Apollo Client for GraphQL with typed operations.

- Use `inject(Apollo)` for dependency injection
- Handle errors with catchError
- Use typed operations (gql tagged templates)

**Relevance**: The `updateWidgetConfig` method in `DashboardService` uses Apollo mutations to persist config.

---

## backend/vogen-value-objects

Always use Vogen 8.0+ for domain value objects. Never use primitives in commands/domain.

- Always include `conversions: Conversions.EfCoreValueConverter`
- Implement `Validate()` for business rules
- Location: `Domain/ValueObjects/{Name}.cs`

**Relevance**: `WidgetTypeId`, `DashboardWidgetId`, `DashboardId` are all Vogen VOs used in the config update flow.
