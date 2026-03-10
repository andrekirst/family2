# Base Data - Federal States (Bundeslaender) -- Shaping Notes

**Feature**: Read-only base reference data for German federal states with ISO 3166-2 codes
**Created**: 2026-03-10
**GitHub Issue**: #227

---

## Scope

Introduce a new "Base Data" module providing system-wide reference data, starting with German federal states (Bundeslaender). The module is read-only (no CRUD mutations), with data seeded from a JSON file via `IHostedService`.

Two GraphQL queries:

- `federalStates` -- list all 16 German federal states
- `federalStateByIso3166(code: String!)` -- lookup by ISO 3166-2 code (e.g., "DE-SN")

Frontend: simple table at "Base Data > Federal States" with Name and ISO code columns.

## Decisions

| Decision | Notes |
|----------|-------|
| Entity naming: `FederalState` | Germany-specific for now. Will generalize when other countries added |
| Plain entity, not AggregateRoot | Immutable reference data needs no domain events or RowVersion |
| No `IRequireFamily` | System-wide data, not family-scoped |
| JSON file + IHostedService seeder | Runs in all environments, idempotent |
| `[ModuleOrder(1200)]` | After Search(1100) |
| Sidebar icon: `ICONS.DOCUMENT` | No globe/map icon available; adequate for MVP |
| ISO 3166-2 validation: `^[A-Z]{2}-[A-Z]{1,3}$` | Covers German codes; may loosen for other countries later |

## Context

- **Visuals:** Simple table/list -- Name column + ISO 3166-2 Code column
- **References:** School module (`Features/School/`) as structural reference; DevDataSeeder for seeding pattern
- **Product alignment:** Phase 1 MVP. Base data is foundational for location-aware features (calendar, school, etc.)

## Standards Applied

- **architecture/ddd-modules** -- IModule pattern, feature-folder layout
- **backend/graphql-input-command** -- GraphQL namespace types, QueryType extensions
- **backend/vogen-value-objects** -- FederalStateId, Iso3166Code, FederalStateName as Vogen VOs
- **database/ef-core-migrations** -- DbUp SQL migration for schema + table creation
- **frontend/angular-components** -- Standalone components, OnPush, signals
- **frontend/apollo-graphql** -- Apollo service with typed GQL operations
