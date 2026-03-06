# References for School Module

## Similar Implementations

### Family Module (Primary Reference)

- **Location:** `src/FamilyHub.Api/Features/Family/`
- **Relevance:** Complete module implementation with all layers — the exact pattern to replicate
- **Key patterns:**
  - `FamilyModule.cs` — IModule registration
  - `Domain/Entities/Family.cs` — AggregateRoot with factory method and domain events
  - `Domain/ValueObjects/FamilyId.cs` — Vogen value object
  - `Domain/Events/FamilyCreatedEvent.cs` — Domain event record
  - `Domain/Repositories/IFamilyRepository.cs` — Repository interface
  - `Data/FamilyConfiguration.cs` — EF Core IEntityTypeConfiguration
  - `Infrastructure/Repositories/FamilyRepository.cs` — EF Core repository
  - `Application/Commands/CreateFamily/` — Full command pattern (Command, Handler, Validator, MutationType)
  - `Application/Queries/GetMyFamily/` — Full query pattern (Query, Handler, QueryType)
  - `Models/FamilyDto.cs` — DTO pattern
  - `Application/Mappers/FamilyMapper.cs` — Mapper pattern

### Family Module — Frontend

- **Location:** `src/frontend/family-hub-web/src/app/features/family/`
- **Relevance:** Frontend feature module pattern
- **Key patterns:**
  - `family.routes.ts` — Route configuration with lazy-loaded components
  - `family.providers.ts` — Feature provider function
  - `services/family.service.ts` — Apollo service with typed operations
  - `graphql/family.operations.ts` — gql tagged templates
  - `components/create-family-dialog/` — Dialog component pattern

### Family Module — Tests

- **Location:** `tests/FamilyHub.Family.Tests/`
- **Relevance:** Test structure and patterns
- **Key patterns:**
  - `Features/Family/Domain/FamilyAggregateTests.cs` — Aggregate creation, domain events
  - `Features/Family/Application/CreateFamilyCommandHandlerTests.cs` — Handler testing with fakes

### Shared Fakes

- **Location:** `tests/FamilyHub.TestCommon/Fakes/`
- **Relevance:** Fake repository implementations for testing
- **Key patterns:**
  - `FakeFamilyRepository.cs` — In-memory fake with AddedFamilies tracking
  - `FakeFamilyMemberRepository.cs` — Fake with constructor-based seeding

### GraphQL Namespace Types

- **Location:** `src/FamilyHub.Api/Common/Infrastructure/GraphQL/NamespaceTypes/`
- **Relevance:** How to add new module entry points to the GraphQL schema
- **Key files:**
  - `RootQuery.cs` — Add `School()` method returning `SchoolQuery`
  - `RootMutation.cs` — Add `School()` method returning `SchoolMutation`
  - `FamilyQuery.cs` — Example namespace marker class

### Sidebar Navigation

- **Location:** `src/frontend/family-hub-web/src/app/shared/layout/sidebar/sidebar.component.ts`
- **Relevance:** Where to add the School navigation item
- **Key pattern:** `navItems` array (lines 202-245) with path, label, icon, matchPrefix

### Icons

- **Location:** `src/frontend/family-hub-web/src/app/shared/icons/`
- **Relevance:** Where to add the academic-cap icon
- **Key files:**
  - `icons.ts` — Central icon registry
  - `defs/*.ts` — Individual icon SVG exports
