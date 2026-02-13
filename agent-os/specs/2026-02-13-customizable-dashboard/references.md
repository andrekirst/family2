# References for Customizable Dashboard

## Similar Implementations

### IChainPlugin / IChainRegistry (EventChain Module)

- **Location:** `src/FamilyHub.EventChain.Infrastructure/Registry/`
- **Relevance:** Exact pattern to mirror for IWidgetProvider / IWidgetRegistry
- **Key patterns:**
  - `IChainPlugin` interface: `ModuleName`, `GetTriggers()`, `GetActions()`, `GetActionHandlers()`
  - `IChainRegistry` singleton: `RegisterPlugin()`, query methods
  - `TriggerDescriptor` / `ActionDescriptor` records for metadata
  - `ChainRegistry` collects all plugins at startup
- **What to borrow:** Plugin registration lifecycle, descriptor records, singleton registry with query methods

### Family Module (CQRS + GraphQL Patterns)

- **Location:** `src/FamilyHub.Api/Features/Family/`
- **Relevance:** Most complete module example for CQRS, GraphQL, domain model, and testing patterns
- **Key patterns:**
  - `Family` aggregate with factory methods (`Create()`) and domain events
  - Vogen value objects (`FamilyId`, `FamilyName`, `FamilyRole`)
  - Command subfolder layout: `Commands/{Name}/Command.cs, Handler.cs, MutationType.cs, Validator.cs`
  - `FamilyAuthorizationService` for permission enforcement
  - EF Core configuration with schema separation
  - Fake repositories in `tests/FamilyHub.TestCommon/Fakes/`
- **What to borrow:** Aggregate structure, command/query organization, test patterns

### EventChain Module (Module Registration)

- **Location:** `src/FamilyHub.Api/Features/EventChain/EventChainModule.cs`
- **Relevance:** Most sophisticated module registration example (singleton registry + hosted service + middleware)
- **Key patterns:**
  - `IChainRegistry` registered as singleton
  - `ChainSchedulerService` as hosted service
  - Pipeline composition in DI
- **What to borrow:** Singleton registry registration, hosted service pattern for initialization

### Existing Dashboard Component (Frontend)

- **Location:** `src/frontend/family-hub-web/src/app/features/dashboard/dashboard.component.ts`
- **Relevance:** Component to be rewritten — must preserve backward-compatible behavior
- **Key patterns:**
  - Signals-based state (`currentUser`, `isLoading`, `pendingInvitations`)
  - TopBarService integration for dynamic header
  - Conditional rendering based on family membership
  - Pending invitation accept/decline logic
- **What to preserve:** Welcome message, no-family state, invitation management

### Frontend Feature Architecture

- **Location:** `src/frontend/family-hub-web/src/app/`
- **Relevance:** How features are structured, routed, and provided
- **Key patterns:**
  - `app.routes.ts` — lazy-loaded routes with `loadChildren`
  - `app.config.ts` — feature providers spread into config
  - `{feature}.providers.ts` — `provide{Feature}Feature()` returning `Provider[]`
  - `{feature}.routes.ts` — `{FEATURE}_ROUTES` const
  - `FamilyPermissionService` — computed signals for permission checks
  - `TopBarService` — dynamic header configuration
- **What to borrow:** Provider registration pattern, permission filtering, TopBar integration

### GraphQL Namespace Types

- **Location:** `src/FamilyHub.Api/Common/Infrastructure/GraphQL/NamespaceTypes/`
- **Relevance:** How to add Dashboard namespace to GraphQL schema
- **Key patterns:**
  - `RootQuery` with `[Authorize]` namespace methods returning empty types
  - `RootMutation` same pattern
  - `[ExtendObjectType(typeof(XxxQuery))]` on handler types
- **What to borrow:** Add `DashboardQuery`/`DashboardMutation` namespace types
