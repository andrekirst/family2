# References for Base Data - Federal States

## Similar Implementations

### School Module (Primary Reference)

- **Location:** `src/FamilyHub.Api/Features/School/`
- **Relevance:** Most recently added module; simplest IModule implementation. Structural template for BaseData module.
- **Key patterns:**
  - `SchoolModule.cs` -- `[ModuleOrder(1000)] IModule` with repository + search provider registration
  - `Domain/ValueObjects/StudentId.cs` -- Vogen value object with Guid validation
  - `Application/Queries/GetStudents/` -- IReadOnlyQuery + IQueryHandler + QueryType pattern
  - `Data/StudentConfiguration.cs` -- EF Core config with schema separation ("school")

### School Frontend (Page + List Pattern)

- **Location:** `src/frontend/family-hub-web/src/app/features/school/`
- **Relevance:** Container/presentational component split, signals-based state, TopBarService integration
- **Key patterns:**
  - `school-page.component.ts` -- Container with `signal()`, `effect()`, `ngOnInit`
  - `student-list.component.ts` -- Presentational with `@Input()` props
  - `school.routes.ts` -- Lazy-loaded routes
  - `school.providers.ts` -- Feature provider function

### DevDataSeeder (Seeding Pattern)

- **Location:** `src/FamilyHub.Api/Common/Development/DevDataSeeder.cs`
- **Relevance:** IHostedService pattern for data seeding at startup
- **Key patterns:**
  - Scoped service creation via `IServiceScopeFactory`
  - Idempotency checks before insertion
  - Note: DevDataSeeder runs only in development; BaseDataSeeder runs in all environments

### RootQuery (GraphQL Namespace Pattern)

- **Location:** `src/FamilyHub.Api/Common/Infrastructure/GraphQL/NamespaceTypes/RootQuery.cs`
- **Relevance:** Shows how to add namespace entry points for new modules
- **Key patterns:**
  - `[Authorize] public SchoolQuery School() => new();` -- each module gets a namespace type
  - All namespace types are empty marker classes

### Sidebar Navigation

- **Location:** `src/frontend/family-hub-web/src/app/shared/layout/sidebar/sidebar.component.ts`
- **Relevance:** NavItem array structure for adding new navigation entries
- **Key patterns:**
  - `path`, `label` (with i18n), `icon` (sanitized SVG), `matchPrefix` for active state
