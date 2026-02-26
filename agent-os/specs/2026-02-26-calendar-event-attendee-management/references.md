# References for Calendar Event Attendee Management

## Similar Implementations

### Family Module — FamilyMember Entity Management

- **Location:** `src/FamilyHub.Api/Features/Family/`
- **Relevance:** FamilyMember is to Family as Attendee is to CalendarEvent — a related entity managed within an aggregate
- **Key patterns:**
  - Entity with factory method (`FamilyMember.Create()`)
  - Separate repository (`IFamilyMemberRepository`)
  - Handler tests using fake repositories

### Family Module — SendInvitation Command (Subfolder Pattern)

- **Location:** `src/FamilyHub.Api/Features/Family/Application/Commands/SendInvitation/`
- **Relevance:** Reference implementation for the subfolder-per-command layout with MutationType
- **Key patterns:**
  - `MutationType.cs` with `[ExtendObjectType(typeof(FamilyMutation))]`
  - `IUserService.GetCurrentUser()` for resolving current user
  - `ICommandBus.SendAsync()` for dispatching
  - Request DTO → Command mapping in MutationType

### Calendar Module — Existing Event CRUD

- **Location:** `src/FamilyHub.Api/Features/Calendar/`
- **Relevance:** Current attendee handling in Create/Update handlers
- **Key patterns:**
  - `CalendarEvent.Create()` factory method with domain events
  - `GetByIdWithAttendeesAsync()` for eager loading attendees
  - `CalendarEventMapper.ToDto()` for response mapping
  - `CalendarMutations.cs` as the existing monolithic mutation class

### Event Context Component (Frontend)

- **Location:** `src/frontend/family-hub-web/src/app/features/calendar/components/event-context/event-context.component.ts`
- **Relevance:** The component where the people picker UI will be added
- **Key patterns:**
  - `familyMembers` signal loaded via `InvitationService.getFamilyMembers()`
  - `resolvedAttendees` computed (maps IDs to names)
  - `selectedAttendees` signal (array of user IDs)
  - `ToastService` for user feedback

### Event Dialog Component (Frontend — Create/Update Flow)

- **Location:** `src/frontend/family-hub-web/src/app/features/calendar/components/event-dialog/event-dialog.component.ts`
- **Relevance:** Existing checkbox-based attendee picker for create/update flows
- **Key patterns:**
  - `toggleAttendee(userId)` method for add/remove
  - `invitationService.getFamilyMembers()` for loading family members
  - Pre-selecting current user as attendee for new events

### Family Handler Tests (Test Pattern Reference)

- **Location:** `tests/FamilyHub.Family.Tests/Features/Family/Application/`
- **Relevance:** Handler test patterns with fake repositories
- **Key patterns:**
  - `CreateHandler()` helper returning tuple of handler + fakes
  - `act.Should().ThrowAsync<DomainException>().WithMessage(...)` for error assertions
  - AAA pattern with FluentAssertions

### Fake Repository Pattern

- **Location:** `tests/FamilyHub.TestCommon/Fakes/`
- **Relevance:** Shared fake repositories for testing
- **Key patterns:**
  - Constructor accepts optional existing entities
  - `AddedXxx` lists for tracking mutations
  - `SaveChangesCalled` / `SaveChangesCallCount` for verification
