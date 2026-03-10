# Standards for Base Data - Federal States

The following standards apply to this work.

---

## architecture/ddd-modules

New `BaseData` module follows IModule pattern with `[ModuleOrder(1200)]` attribute. Self-contained feature folder at `Features/BaseData/` with Domain, Application, Infrastructure, Data, and Models subfolders.

---

## backend/graphql-input-command

GraphQL namespace type pattern: `BaseDataQuery` class added to `NamespaceTypes/`, entry point in `RootQuery`. Query types use `[ExtendObjectType(typeof(BaseDataQuery))]`. No mutations for this module (read-only).

---

## backend/vogen-value-objects

Three Vogen value objects:

- `FederalStateId` -- `[ValueObject<Guid>]` with empty GUID validation
- `Iso3166Code` -- `[ValueObject<string>]` with regex `^[A-Z]{2}-[A-Z]{1,3}$`
- `FederalStateName` -- `[ValueObject<string>]` with non-empty validation

All use `conversions: Conversions.EfCoreValueConverter` for EF Core integration.

---

## database/ef-core-migrations

DbUp SQL migration creates `base_data` schema and `federal_states` table with:

- `id` (uuid, PK, not auto-generated)
- `name` (varchar(100), not null)
- `iso3166_code` (varchar(10), not null, unique index)

EF Core configuration in `FederalStateConfiguration` maps to table with Vogen value converters.

---

## frontend/angular-components

- Standalone components with `OnPush` change detection
- Container/presentational split: `federal-states-page` (container) + `federal-state-list` (presentational)
- Signals for reactive state (`signal()`, `computed()`)
- `TopBarService` for page title configuration
- Lazy-loaded routes via `loadComponent`

---

## frontend/apollo-graphql

- Typed GraphQL operations in `graphql/base-data.operations.ts`
- Injectable service wrapping Apollo queries
- `Observable<T>` return types from service methods
