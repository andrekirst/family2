# References for Photo Gallery with Grid and Image Views

## Similar Implementations

### Calendar Module (Primary Reference)

- **Location**: `src/FamilyHub.Api/Features/Calendar/`
- **Relevance**: Most complete module implementation following all project patterns
- **Key patterns to borrow**:
  - `CalendarModule.cs` — IModule registration pattern
  - `CalendarEvent.cs` — Aggregate root with factory method, domain events, private setters
  - `CalendarEventId.cs` — Vogen value object pattern
  - `EventTitle.cs` — String value object with validation
  - `CalendarEventCreatedEvent.cs` — Domain event record pattern
  - `ICalendarEventRepository.cs` — Repository interface pattern
  - `CalendarEventRepository.cs` — EF Core repository implementation
  - `CalendarEventConfiguration.cs` — EF Core entity configuration
  - `CalendarMutations.cs` — GraphQL mutations with `[ExtendObjectType]`, ClaimsPrincipal auth
  - `CalendarQueries.cs` — GraphQL queries extending namespace types
  - `FamilyCalendarMutation.cs` — Empty namespace type pattern

### Calendar Frontend (Primary Reference)

- **Location**: `src/frontend/family-hub-web/src/app/features/calendar/`
- **Relevance**: Complete feature module with signals-based state, view switching, Apollo service
- **Key patterns to borrow**:
  - `calendar-page.component.ts` — Main page with view mode switching, signal state management
  - `calendar.service.ts` — Apollo wrapper with error handling
  - `calendar.operations.ts` — Typed GraphQL operations
  - `calendar.models.ts` — TypeScript interfaces and constants
  - `calendar.routes.ts` — Lazy-loaded route pattern
  - `calendar.providers.ts` — Feature provider function

### Family Module (Authorization Reference)

- **Location**: `src/FamilyHub.Api/Features/Family/`
- **Relevance**: Permission system and authorization service patterns
- **Key patterns to borrow**:
  - `FamilyAuthorizationService.cs` — Backend authorization checks
  - `FamilyRole.cs` — Permission methods on Vogen value object
  - `FamilyPermissionService` (frontend) — Computed signal permissions

### Test Infrastructure

- **Location**: `tests/FamilyHub.TestCommon/Fakes/`
- **Relevance**: Fake repository patterns for unit testing
- **Key patterns to borrow**:
  - `FakeFamilyRepository.cs` — In-memory repo with tracking properties
  - `FakeFamilyMemberRepository.cs` — Constructor-initialized test data

### GraphQL Namespace Types

- **Location**: `src/FamilyHub.Api/Common/Infrastructure/GraphQL/NamespaceTypes/`
- **Relevance**: How modules extend the GraphQL schema
- **Key files**:
  - `FamilyMutation.cs` — Parent namespace (must add `Photos()` method)
  - `FamilyCalendarMutation.cs` — Child namespace pattern to follow
  - `FamilyQuery.cs` — Query namespace extended by modules

## Architecture References

- **ADR-001**: `docs/architecture/ADR-001-MODULAR-MONOLITH-FIRST.md` — Why modular monolith
- **ADR-003**: `docs/architecture/ADR-003-GRAPHQL-INPUT-COMMAND-PATTERN.md` — Input→Command separation
- **IModule**: `src/FamilyHub.Api/Common/Modules/IModule.cs` — Module interface
- **ModuleExtensions**: `src/FamilyHub.Api/Common/Modules/ModuleExtensions.cs` — Registration helper
