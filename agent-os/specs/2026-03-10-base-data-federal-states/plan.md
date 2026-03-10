# Base Data - Federal States (Bundeslaender)

**Created**: 2026-03-10
**GitHub Issue**: #227
**Spec**: `agent-os/specs/2026-03-10-base-data-federal-states/`

## Context

Family Hub needs base reference data for German federal states (Bundeslaender) with ISO 3166-2 codes (e.g., "Sachsen" / "DE-SN"). This is the first "Base Data" module -- read-only, system-wide data not scoped to any family. Germany first, other countries planned later.

The feature adds:

- A new `BaseData` backend module with two GraphQL queries (no mutations)
- A JSON seed file with the 16 German federal states, loaded via `IHostedService`
- A new frontend page at "Base Data > Federal States" with a simple table
- A new sidebar navigation entry

## Key Design Decisions

| Decision | Rationale |
|----------|-----------|
| `FederalState` entity (not AggregateRoot) | Immutable reference data -- no domain events, no RowVersion needed |
| No `IRequireFamily` on queries | System-wide data, not family-scoped. Still requires `[Authorize]` |
| JSON file + `IHostedService` seeder | Runs in all environments. Idempotent (checks `AnyAsync` before insert) |
| `IReadOnlyQuery<T>` without `IRequireFamily` | Pipeline skips transactions/sanitization for read-only queries automatically |
| `ICONS.DOCUMENT` for sidebar icon | No globe icon exists. DOCUMENT is adequate for MVP |
| `[ModuleOrder(1200)]` | After Search(1100), no dependencies on other modules |

## Files to Modify (7 existing files)

| File | Change |
|------|--------|
| `src/FamilyHub.Api/Common/Infrastructure/GraphQL/NamespaceTypes/RootQuery.cs` | Add `[Authorize] public BaseDataQuery BaseData() => new();` |
| `src/FamilyHub.Api/Common/Database/AppDbContext.cs` | Add `DbSet<FederalState> FederalStates` |
| `src/FamilyHub.Api/FamilyHub.Api.csproj` | Add `<EmbeddedResource>` for seed JSON |
| `src/FamilyHub.Api/FamilyHub.slnx` | Add test project reference |
| `src/frontend/family-hub-web/src/app/app.routes.ts` | Add `base-data` lazy route |
| `src/frontend/family-hub-web/src/app/app.config.ts` | Add `...provideBaseDataFeature()` |
| `src/frontend/family-hub-web/src/app/shared/layout/sidebar/sidebar.component.ts` | Add Base Data nav item |

## Files to Create (~30 new files)

### Backend: Domain Layer

| File | Purpose |
|------|---------|
| `Features/BaseData/Domain/ValueObjects/FederalStateId.cs` | Vogen `[ValueObject<Guid>]` |
| `Features/BaseData/Domain/ValueObjects/Iso3166Code.cs` | Vogen `[ValueObject<string>]` -- validate `^[A-Z]{2}-[A-Z]{1,3}$` |
| `Features/BaseData/Domain/ValueObjects/FederalStateName.cs` | Vogen `[ValueObject<string>]` -- non-empty, max 100 |
| `Features/BaseData/Domain/Entities/FederalState.cs` | Plain entity. Props: Id, Name, Iso3166Code. Static `Create()` factory |
| `Features/BaseData/Domain/Repositories/IFederalStateRepository.cs` | `GetAllAsync`, `GetByIso3166CodeAsync`, `AnyAsync`, `AddRangeAsync` |

### Backend: Application Layer

| File | Purpose |
|------|---------|
| `Features/BaseData/Models/FederalStateDto.cs` | `Guid Id, string Name, string Iso3166Code` |
| `Features/BaseData/Application/Mappers/FederalStateMapper.cs` | Static `ToDto` / `ToDtoList` |
| `Features/BaseData/Application/Queries/GetFederalStates/GetFederalStatesQuery.cs` | `IReadOnlyQuery<List<FederalStateDto>>` -- no IRequireFamily |
| `Features/BaseData/Application/Queries/GetFederalStates/GetFederalStatesQueryHandler.cs` | `IQueryHandler` -- calls repo, maps to DTOs |
| `Features/BaseData/Application/Queries/GetFederalStates/QueryType.cs` | `[ExtendObjectType(typeof(BaseDataQuery))]` |
| `Features/BaseData/Application/Queries/GetFederalStateByIso3166/GetFederalStateByIso3166Query.cs` | `IReadOnlyQuery<FederalStateDto?>` with `string Code` |
| `Features/BaseData/Application/Queries/GetFederalStateByIso3166/GetFederalStateByIso3166QueryHandler.cs` | Converts string to `Iso3166Code`, queries repo |
| `Features/BaseData/Application/Queries/GetFederalStateByIso3166/QueryType.cs` | `[ExtendObjectType(typeof(BaseDataQuery))]` |

### Backend: Infrastructure & Data

| File | Purpose |
|------|---------|
| `Features/BaseData/Data/FederalStateConfiguration.cs` | EF Core config -- table `federal_states`, schema `base_data` |
| `Features/BaseData/Data/Seeds/federal-states.json` | 16 German states JSON seed data |
| `Features/BaseData/Infrastructure/Repositories/FederalStateRepository.cs` | EF Core implementation |
| `Features/BaseData/Infrastructure/BaseDataSeeder.cs` | `IHostedService` -- reads embedded JSON, inserts if empty |
| `Features/BaseData/BaseDataModule.cs` | `[ModuleOrder(1200)] IModule` |
| `Common/Infrastructure/GraphQL/NamespaceTypes/BaseDataQuery.cs` | Empty namespace type class |
| `Database/Migrations/base_data/20260310000000_create-base-data-schema.sql` | SQL: schema + table + unique index |

### Frontend

| File | Purpose |
|------|---------|
| `features/base-data/base-data.routes.ts` | Routes with lazy component load |
| `features/base-data/base-data.providers.ts` | `provideBaseDataFeature()` |
| `features/base-data/services/base-data.service.ts` | Apollo service |
| `features/base-data/graphql/base-data.operations.ts` | GQL queries |
| `features/base-data/components/federal-states-page/federal-states-page.component.ts` | Container component |
| `features/base-data/components/federal-states-page/federal-states-page.component.html` | Page template |
| `features/base-data/components/federal-state-list/federal-state-list.component.ts` | Presentational component |
| `features/base-data/components/federal-state-list/federal-state-list.component.html` | Table template |

### Tests

| File | Purpose |
|------|---------|
| `tests/FamilyHub.BaseData.Tests/FamilyHub.BaseData.Tests.csproj` | Test project |
| `tests/FamilyHub.BaseData.Tests/Domain/FederalStateTests.cs` | Entity + VO tests |
| `tests/FamilyHub.BaseData.Tests/Domain/Iso3166CodeTests.cs` | ISO code validation tests |
| `tests/FamilyHub.BaseData.Tests/Application/GetFederalStatesQueryHandlerTests.cs` | List query handler tests |
| `tests/FamilyHub.BaseData.Tests/Application/GetFederalStateByIso3166QueryHandlerTests.cs` | Lookup query handler tests |
| `tests/FamilyHub.BaseData.Tests/Infrastructure/BaseDataSeederTests.cs` | Seeder idempotency tests |

## Implementation Tasks

### Task 1: Save Spec, Commit, and Create GitHub Issue

1. Write spec files to `agent-os/specs/2026-03-10-base-data-federal-states/`
2. Create GitHub issue via `gh issue create`
3. Update spec files with issue number
4. Git commit and push

### Task 2: Backend Domain Layer

Create value objects (FederalStateId, Iso3166Code, FederalStateName), entity (FederalState), and repository interface.

### Task 3: Backend Infrastructure & Data

Create EF Core configuration, SQL migration, repository implementation, JSON seed file, and BaseDataSeeder IHostedService.

### Task 4: Backend Application Layer (Queries + GraphQL)

Create DTOs, mapper, two query handlers, GraphQL namespace type (BaseDataQuery), and QueryType extensions. Wire up in RootQuery.

### Task 5: Backend Module Registration

Create BaseDataModule, add DbSet to AppDbContext, add to .slnx.

### Task 6: Frontend Feature Setup

Create routes, providers, GraphQL operations, and service. Wire into app.routes.ts and app.config.ts.

### Task 7: Frontend Components

Create federal-states-page (container) and federal-state-list (presentational) components. Add sidebar nav item.

### Task 8: Tests

Create test project and unit tests for domain, handlers, and seeder.

## GraphQL Schema (Expected)

```graphql
type Query {
  baseData: BaseDataQuery!
}

type BaseDataQuery {
  federalStates: [FederalStateDto!]!
  federalStateByIso3166(code: String!): FederalStateDto
}

type FederalStateDto {
  id: UUID!
  name: String!
  iso3166Code: String!
}
```

## Seed Data (16 German Federal States)

```json
[
  { "name": "Baden-Wuerttemberg", "iso3166Code": "DE-BW" },
  { "name": "Bayern", "iso3166Code": "DE-BY" },
  { "name": "Berlin", "iso3166Code": "DE-BE" },
  { "name": "Brandenburg", "iso3166Code": "DE-BB" },
  { "name": "Bremen", "iso3166Code": "DE-HB" },
  { "name": "Hamburg", "iso3166Code": "DE-HH" },
  { "name": "Hessen", "iso3166Code": "DE-HE" },
  { "name": "Mecklenburg-Vorpommern", "iso3166Code": "DE-MV" },
  { "name": "Niedersachsen", "iso3166Code": "DE-NI" },
  { "name": "Nordrhein-Westfalen", "iso3166Code": "DE-NW" },
  { "name": "Rheinland-Pfalz", "iso3166Code": "DE-RP" },
  { "name": "Saarland", "iso3166Code": "DE-SL" },
  { "name": "Sachsen", "iso3166Code": "DE-SN" },
  { "name": "Sachsen-Anhalt", "iso3166Code": "DE-ST" },
  { "name": "Schleswig-Holstein", "iso3166Code": "DE-SH" },
  { "name": "Thueringen", "iso3166Code": "DE-TH" }
]
```

## Verification

1. **Backend builds:** `dotnet build src/FamilyHub.Api/FamilyHub.slnx`
2. **Tests pass:** `dotnet test tests/FamilyHub.BaseData.Tests/`
3. **GraphQL:** `{ baseData { federalStates { name iso3166Code } } }` returns 16 states
4. **Lookup:** `{ baseData { federalStateByIso3166(code: "DE-SN") { name } } }` returns "Sachsen"
5. **Frontend:** Navigate to `/base-data/federal-states`, verify 16-row table
6. **Sidebar:** "Base Data" nav item visible and navigates correctly

## Reference Files

- Module pattern: `src/FamilyHub.Api/Features/School/SchoolModule.cs`
- Value object pattern: `src/FamilyHub.Api/Features/School/Domain/ValueObjects/StudentId.cs`
- Query handler pattern: `src/FamilyHub.Api/Features/School/Application/Queries/GetStudents/`
- GraphQL namespace: `src/FamilyHub.Api/Common/Infrastructure/GraphQL/NamespaceTypes/RootQuery.cs`
- Sidebar nav: `src/frontend/family-hub-web/src/app/shared/layout/sidebar/sidebar.component.ts`
- Frontend page pattern: `src/frontend/family-hub-web/src/app/features/school/components/school-page/`
- Frontend routes pattern: `src/frontend/family-hub-web/src/app/features/school/school.routes.ts`
