# Selectable Seed Data — Shaping Notes

## Scope

As a developer of Family Hub, I want to have selectable seed data in the UI — at best on the start page where I can click on "Sign in with Keycloak" — so that I can quickly populate the database with test data during development without manually creating entities through GraphQL or Keycloak.

## Decisions

- **Both predefined scenarios and modular mix-and-match:** Developers get quick one-click scenarios (e.g., "Full Demo") plus the flexibility to combine individual datasets (e.g., just "Users" + "Calendar Events")
- **Dev-only via separate assembly:** `FamilyHub.Dev` project references `FamilyHub.Api` one-way. The Dev DLL is only built and copied in Debug configuration. `Program.cs` loads it dynamically via `Assembly.Load()`. In Release builds, the assembly doesn't exist and the `dev` field never appears in the GraphQL schema.
- **User chooses data handling:** Radio buttons for "Wipe & Reseed" (clean slate) vs "Additive" (layer on top of existing data)
- **Collapsible panel on login page:** Below the "Sign in with Keycloak" button, a dev tools panel with expand/collapse toggle. Only visible when `environment.production` is false (build-time constant, tree-shaken in production).
- **Backend GraphQL API (unauthenticated):** The seed data endpoints are on the login page (before authentication), so they have no `[Authorize]` attribute. Safety comes from the assembly not existing in production.
- **No Mediator pipeline:** Dev is a separate assembly; martinothamar/Mediator source generator won't discover handlers across assemblies. GraphQL resolvers call seed services directly — appropriate for dev tooling.
- **Domain factory methods for seed data:** Datasets use `User.Register()`, `Family.Create()`, etc. to ensure domain invariants are respected. `ClearDomainEvents()` prevents event handler side effects during seeding.
- **Dependency-ordered datasets:** `SeedDataRegistry` performs topological sort based on `DependsOn` declarations. `SeedContext` bag passes entity references between datasets (e.g., Users dataset stores created users, Family dataset reads them).

## Context

- **Visuals:** Simple collapsible panel near the sign-in button (no mockup provided)
- **References:** CreateFamily mutation pattern (MutationType + Command + Handler), LoginComponent inline template
- **Product alignment:** N/A (no `agent-os/product/` directory exists)

## Standards Applied

- **graphql-input-command** — Dev GraphQL types follow the `[ExtendObjectType]` pattern but skip the CQRS pipeline (direct service calls instead)
- **angular-components** — DevSeedPanelComponent is standalone, uses signals, inject() DI
- **apollo-graphql** — GraphQL operations use gql templates with Apollo Client
- **unit-testing** — xUnit + FluentAssertions for registry and applier tests
