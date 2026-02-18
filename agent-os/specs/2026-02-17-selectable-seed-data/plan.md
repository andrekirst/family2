# Selectable Seed Data — Implementation Plan

## Context

Developers currently have no way to quickly populate the database with test data during development. Every time the database is wiped (auto-migration, Docker restart), they must manually create users and families through the GraphQL API or Keycloak. This feature adds a **dev-only UI panel on the login page** that lets developers select and apply predefined seed data scenarios with one click.

**Scope decisions:**

- Predefined scenarios + modular mix-and-match datasets
- Dev-only (hidden in production via separate assembly + environment check)
- User chooses: wipe-and-reseed OR additive
- Collapsible panel below "Sign in with Keycloak" button
- Backend GraphQL API (unauthenticated — used before login)
- **Separate assembly** (`FamilyHub.Dev`) — dev code never ships in Release builds

---

## Task 1: Save Spec Documentation

Create `agent-os/specs/2026-02-17-selectable-seed-data/` with:

- `plan.md` — This plan
- `shape.md` — Shaping decisions and context
- `standards.md` — Relevant standards (graphql-input-command, angular-components, apollo-graphql, unit-testing)
- `references.md` — Reference implementations studied (CreateFamily mutation, LoginComponent)

---

## Task 2: Create FamilyHub.Dev Project

New project: `src/FamilyHub.Dev/FamilyHub.Dev.csproj`

**References:** FamilyHub.Api (one-way, for access to domain entities + AppDbContext)

**No circular dependency:** FamilyHub.Api does NOT reference FamilyHub.Dev at compile-time. Instead:

- `FamilyHub.Api.csproj` gets a conditional build-only dependency (`ReferenceOutputAssembly=false`, Debug only)
- A MSBuild `Target` copies `FamilyHub.Dev.dll` to Api's output in Debug builds
- `Program.cs` loads the assembly at runtime via `Assembly.Load("FamilyHub.Dev")`

**Why no Mediator:** The Mediator source generator runs at compile time per-assembly. Since Dev is separate, its handlers wouldn't be discovered. Instead, the Dev GraphQL resolvers call seed services **directly** (no CQRS pipeline). This is appropriate — seed data is a dev tool, not business logic.

### Files to create/modify

**`src/FamilyHub.Dev/FamilyHub.Dev.csproj`**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\FamilyHub.Api\FamilyHub.Api.csproj" />
  </ItemGroup>
</Project>
```

**Modify `src/FamilyHub.Api/FamilyHub.Api.csproj`** — Add conditional build dep + copy target:

```xml
<ItemGroup Condition="'$(Configuration)' == 'Debug'">
    <ProjectReference Include="..\FamilyHub.Dev\FamilyHub.Dev.csproj">
        <ReferenceOutputAssembly>false</ReferenceOutputAssembly>
    </ProjectReference>
</ItemGroup>

<Target Name="CopyDevModule" AfterTargets="Build"
        Condition="'$(Configuration)' == 'Debug'">
    <Copy SourceFiles="../FamilyHub.Dev/bin/$(Configuration)/$(TargetFramework)/FamilyHub.Dev.dll"
          DestinationFolder="$(OutputPath)"
          SkipUnchangedFiles="true" />
</Target>
```

**Modify `src/FamilyHub.Api/FamilyHub.slnx`** — Add FamilyHub.Dev project entry

**Modify `src/FamilyHub.Api/Program.cs`** — Dynamic loading:

```csharp
// Dev-only module (seed data, dev tools) - separate assembly, loaded dynamically
if (builder.Environment.IsDevelopment())
{
    try
    {
        var devAssembly = Assembly.Load("FamilyHub.Dev");
        var setup = devAssembly.GetType("FamilyHub.Dev.DevSetup")!;
        setup.GetMethod("RegisterServices")!.Invoke(null, [builder.Services, builder.Configuration]);
    }
    catch (FileNotFoundException) { /* Dev assembly not available */ }
}
```

And in the GraphQL builder chain:

```csharp
var graphqlBuilder = builder.Services
    .AddGraphQLServer()
    // ...existing config...
    .AddTypeExtensionsFromAssembly(typeof(Program).Assembly);

if (builder.Environment.IsDevelopment())
{
    try
    {
        var devAssembly = Assembly.Load("FamilyHub.Dev");
        graphqlBuilder.AddTypeExtensionsFromAssembly(devAssembly);
    }
    catch (FileNotFoundException) { }
}
```

---

## Task 3: Seed Data Infrastructure (in FamilyHub.Dev)

### Folder structure

```
src/FamilyHub.Dev/
├── DevSetup.cs                    # Static class: RegisterServices()
├── SeedData/
│   ├── ISeedDataset.cs            # Interface for atomic datasets
│   ├── ISeedScenario.cs           # Interface for predefined scenarios
│   ├── SeedContext.cs             # Shared context between datasets (pass entity refs)
│   ├── SeedMode.cs                # Enum: WipeAndReseed, Additive
│   ├── ISeedDataRegistry.cs       # Registry interface
│   ├── SeedDataRegistry.cs        # Discovers datasets/scenarios, topological sort
│   ├── SeedDataApplier.cs         # Orchestrates wipe + apply in order
│   ├── Datasets/
│   │   ├── UsersSeedDataset.cs        # 5 users
│   │   ├── FamilySeedDataset.cs       # 1 family (depends: users)
│   │   ├── FamilyMembersSeedDataset.cs # 4 members (depends: users, family)
│   │   ├── InvitationsSeedDataset.cs   # 3 pending (depends: users, family)
│   │   └── CalendarEventsSeedDataset.cs # 3 events (depends: users, family, members)
│   └── Scenarios/
│       ├── EmptyStateScenario.cs
│       ├── SingleUserScenario.cs
│       ├── FamilyWithOwnerScenario.cs
│       ├── FamilyWith5MembersScenario.cs
│       ├── FamilyWithInvitationsScenario.cs
│       └── FullDemoScenario.cs
└── GraphQL/
    ├── DevQuery.cs                # Namespace type (no [Authorize])
    ├── DevMutation.cs             # Namespace type (no [Authorize])
    ├── RootQueryDevExtension.cs   # [ExtendObjectType(typeof(RootQuery))]
    ├── RootMutationDevExtension.cs # [ExtendObjectType(typeof(RootMutation))]
    ├── SeedScenariosQueryType.cs  # [ExtendObjectType(typeof(DevQuery))]
    ├── ApplySeedDataMutationType.cs # [ExtendObjectType(typeof(DevMutation))]
    ├── ResetDatabaseMutationType.cs # [ExtendObjectType(typeof(DevMutation))]
    └── Models/
        ├── SeedScenarioDto.cs
        ├── SeedDatasetDto.cs
        ├── ApplySeedDataRequest.cs
        └── ApplySeedDataResult.cs
```

### Key interfaces

**`ISeedDataset`** — Atomic unit of seed data:

- `Id`, `Name`, `Description`, `Category`, `RecordCount`
- `DependsOn` — list of dataset IDs that must run first
- `ApplyAsync(AppDbContext, SeedContext, CancellationToken)` — creates entities using domain factory methods, calls `ClearDomainEvents()`, saves via `context.SaveChangesAsync()`

**`ISeedScenario`** — Named combination of datasets:

- `Id`, `Name`, `Description`
- `DatasetIds` — ordered list of dataset IDs

**`SeedContext`** — Dictionary-based bag for passing entities between datasets (e.g., Users dataset stores users, Family dataset reads them)

**`SeedDataRegistry`** — Collects all ISeedDataset/ISeedScenario via DI, provides `ResolveInOrder()` with topological sort

**`SeedDataApplier`** — Orchestrates: optional wipe (delete in reverse FK order) → resolve datasets → apply in order

### Seed data details

| Dataset ID | Category | Records | Depends On |
|---|---|---|---|
| `users-basic` | users | 5 | — |
| `family-basic` | families | 1 | users-basic |
| `family-members-basic` | families | 4 | users-basic, family-basic |
| `invitations-pending` | invitations | 3 | users-basic, family-basic |
| `calendar-events-basic` | calendar | 3 | users-basic, family-basic, family-members-basic |

Datasets use domain factory methods (`User.Register()`, `Family.Create()`, `FamilyMember.Create()`, `FamilyInvitation.Create()`, `CalendarEvent.Create()`) then call `ClearDomainEvents()` to prevent event handlers firing during seeding.

### Scenarios

| ID | Name | Datasets |
|---|---|---|
| `empty-state` | Empty State | (none — just wipes) |
| `single-user` | Single User | users-basic |
| `family-with-owner` | Family with Owner | users-basic, family-basic |
| `family-with-5-members` | Family with 5 Members | users-basic, family-basic, family-members-basic |
| `family-with-invitations` | Family with Invitations | users-basic, family-basic, family-members-basic, invitations-pending |
| `full-demo` | Full Demo | users-basic, family-basic, family-members-basic, invitations-pending, calendar-events-basic |

### GraphQL Schema

```graphql
# No [Authorize] — accessible from login page
type DevQuery {
  seedScenarios: [SeedScenario!]!
  seedDatasets: [SeedDataset!]!
}

type DevMutation {
  applySeedData(input: ApplySeedDataInput!): ApplySeedDataResult!
  resetDatabase: ResetDatabaseResult!
}

type SeedScenario { id: String!, name: String!, description: String!, datasets: [String!]! }
type SeedDataset { id: String!, name: String!, description: String!, category: String!, recordCount: Int! }
input ApplySeedDataInput { scenarioId: String, datasetIds: [String!], mode: SeedMode! }
enum SeedMode { WIPE_AND_RESEED, ADDITIVE }
type ApplySeedDataResult { success: Boolean!, message: String!, appliedDatasets: [String!]!, recordsCreated: Int! }
type ResetDatabaseResult { success: Boolean!, message: String! }
```

GraphQL resolvers call `SeedDataApplier` directly (no Mediator pipeline).

---

## Task 4: Frontend Dev Seed Panel

### Files to create

```
src/frontend/family-hub-web/src/app/features/dev/
├── dev-seed-panel/
│   └── dev-seed-panel.component.ts   # Standalone component (inline template)
├── graphql/
│   └── dev-seed.operations.ts        # gql queries + mutations
└── models/
    └── seed-data.models.ts           # TypeScript interfaces
```

### Component behavior

- **Collapsible panel** below login button, toggle with "Developer Tools" header + chevron
- **Two tabs:** "Scenarios" (dropdown) and "Mix & Match" (checkboxes grouped by category)
- **Mode selector:** Radio buttons for "Wipe & Reseed" / "Additive"
- **Confirmation:** When "Wipe & Reseed" is selected, "Apply" shows a confirm step
- **Apply button** with loading spinner + success/error toast
- **Auto-detection:** Queries `dev.seedScenarios` on init; if the query fails (production), panel auto-hides

### Modify existing file

**`src/frontend/family-hub-web/src/app/features/auth/login/login.component.ts`**

- Import `DevSeedPanelComponent`
- Add `@if (!environment.production) { <app-dev-seed-panel /> }` after the terms text
- Uses `environment.production` from `environments/environment.ts` (build-time, tree-shakeable)

### GraphQL operations

```typescript
// Query: load available scenarios + datasets
query GetDevSeedData {
  dev {
    seedScenarios { id, name, description, datasets }
    seedDatasets { id, name, description, category, recordCount }
  }
}

// Mutation: apply seed data
mutation ApplySeedData($input: ApplySeedDataInput!) {
  dev {
    applySeedData(input: $input) { success, message, appliedDatasets, recordsCreated }
  }
}
```

---

## Task 5: Tests

### Backend: `tests/FamilyHub.Dev.Tests/FamilyHub.Dev.Tests.csproj`

**SeedDataRegistry tests:**

- Topological sort resolves correct order
- Circular dependency detection throws
- Missing dependency throws
- Scenario/dataset lookup by ID

**SeedDataApplier tests (using in-memory SQLite or mocked context):**

- Wipe mode clears existing data before seeding
- Additive mode preserves existing data
- Datasets applied in correct dependency order
- Runtime environment check blocks non-dev execution

**Individual dataset tests:**

- Each dataset creates expected number of entities
- Entities use correct domain factory methods
- Domain events are cleared (not published)

### Frontend

- Panel renders only when `environment.production` is false
- Scenarios load from GraphQL and display
- Apply button calls mutation with correct variables

---

## Critical Files to Modify (Existing)

| File | Change |
|---|---|
| `src/FamilyHub.Api/Program.cs` | Dynamic assembly loading for FamilyHub.Dev + GraphQL type registration |
| `src/FamilyHub.Api/FamilyHub.Api.csproj` | Conditional build dep on FamilyHub.Dev + copy target |
| `src/FamilyHub.Api/FamilyHub.slnx` | Add FamilyHub.Dev + FamilyHub.Dev.Tests project entries |
| `src/frontend/.../auth/login/login.component.ts` | Conditionally render `<app-dev-seed-panel />` |

## Reusable Existing Code

| What | Location | How Used |
|---|---|---|
| `AggregateRoot.ClearDomainEvents()` | `src/FamilyHub.Common/Domain/AggregateRoot.cs:41` | Clear events after seeding entities |
| `User.Register()` | `src/FamilyHub.Api/Features/Auth/Domain/Entities/User.cs:94` | Create seed users |
| `Family.Create()` | `src/FamilyHub.Api/Features/Family/Domain/Entities/Family.cs:27` | Create seed families |
| `FamilyMember.Create()` | `src/FamilyHub.Api/Features/Family/Domain/Entities/FamilyMember.cs:23` | Create seed members |
| `FamilyInvitation.Create()` | `src/FamilyHub.Api/Features/Family/Domain/Entities/FamilyInvitation.cs:31` | Create seed invitations |
| `IModule` pattern | `src/FamilyHub.Api/Common/Modules/IModule.cs` | DevSetup follows same structure |
| `RootQuery` / `RootMutation` | `src/FamilyHub.Api/Common/Infrastructure/GraphQL/NamespaceTypes/` | Extended by Dev GraphQL types |
| `environment.production` | `src/frontend/.../environments/environment.ts` | Frontend dev detection |

## Verification

1. **Build:** `dotnet build src/FamilyHub.Api/FamilyHub.slnx` — verify FamilyHub.Dev builds and DLL is copied to Api output
2. **Schema check:** Run API in Development, query `{ __schema { queryType { fields { name } } } }` — verify `dev` field appears
3. **Schema check (prod):** Build in Release, verify `dev` field does NOT appear in schema
4. **Seed scenarios query:** `{ dev { seedScenarios { id name } seedDatasets { id name category } } }` — returns 6 scenarios + 5 datasets
5. **Apply seed data:** `mutation { dev { applySeedData(input: { scenarioId: "full-demo", mode: WIPE_AND_RESEED }) { success recordsCreated } } }` — returns success with expected record count
6. **Frontend:** `ng serve` → navigate to `/login` → verify dev panel appears below sign-in button
7. **Tests:** `dotnet test tests/FamilyHub.Dev.Tests/` — all pass
