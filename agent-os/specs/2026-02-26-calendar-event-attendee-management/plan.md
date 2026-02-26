# Plan: Manage Attendees of a Calendar Event

## Context

Currently, calendar event attendees are a simple join table (`CalendarEventAttendee` with only `CalendarEventId` + `UserId`). Attendees can only be set during event creation or fully replaced during update — there's no way to add or remove individual attendees. This plan adds dedicated `AddAttendee` and `RemoveAttendee` commands with creator-only permissions and an interactive people picker UI in the event detail panel.

**User story:** As a family member, I want to manage attendees of a calendar event so I can add or remove people without editing the entire event.

**Scope decisions:**

- Add/Remove individual attendees (dedicated commands, not replacing the whole list)
- Creator-only permissions (only `event.CreatedBy` can manage attendees)
- People picker UI inline in the existing event context panel
- No RSVP/status tracking — just add/remove

---

## Task 1: Add Domain Methods and Events

**Add domain error codes** to `src/FamilyHub.Common/Domain/DomainErrorCodes.cs`:

- `AttendeeAlreadyExists` = `"ATTENDEE_ALREADY_EXISTS"`
- `AttendeeNotFound` = `"ATTENDEE_NOT_FOUND"`
- `NotEventCreator` = `"NOT_EVENT_CREATOR"`

**Create two domain events:**

- `src/FamilyHub.Api/Features/Calendar/Domain/Events/AttendeeAddedToCalendarEventEvent.cs`
- `src/FamilyHub.Api/Features/Calendar/Domain/Events/AttendeeRemovedFromCalendarEventEvent.cs`

Both follow the sealed record pattern extending `DomainEvent`.

**Add two methods to `CalendarEvent` aggregate** (`src/FamilyHub.Api/Features/Calendar/Domain/Entities/CalendarEvent.cs`):

- `AddAttendee(UserId userId)`: checks cancelled, checks duplicate, adds to collection, updates `UpdatedAt`, raises event
- `RemoveAttendee(UserId userId)`: checks cancelled, finds attendee or throws, removes from collection, updates `UpdatedAt`, raises event

All invariants enforced in the aggregate — keeps handlers thin.

---

## Task 2: Create AddAttendee Command (Backend)

New subfolder: `src/FamilyHub.Api/Features/Calendar/Application/Commands/AddAttendee/`

| File | Purpose |
|------|---------|
| `AddAttendeeCommand.cs` | `record(CalendarEventId, AttendeeUserId, RequestedBy) : ICommand<AddAttendeeResult>` |
| `AddAttendeeResult.cs` | `record(CalendarEventId)` |
| `AddAttendeeCommandHandler.cs` | Loads event w/ attendees, verifies `CreatedBy == RequestedBy`, calls `calendarEvent.AddAttendee()`, saves |
| `AddAttendeeCommandValidator.cs` | NotNull on all three fields |
| `MutationType.cs` | `[ExtendObjectType(typeof(FamilyCalendarMutation))]` — resolves current user via `IUserService.GetCurrentUser()`, dispatches command, returns updated `CalendarEventDto` |

Pattern reference: `src/FamilyHub.Api/Features/Family/Application/Commands/SendInvitation/MutationType.cs`

---

## Task 3: Create RemoveAttendee Command (Backend)

New subfolder: `src/FamilyHub.Api/Features/Calendar/Application/Commands/RemoveAttendee/`

Same structure as AddAttendee — mirror pattern with `RemoveAttendeeCommand`, `RemoveAttendeeResult`, handler, validator, and MutationType.

Handler: loads event, verifies creator, calls `calendarEvent.RemoveAttendee()`, saves.

---

## Task 4: Write Backend Tests

**Create fake repository:** `tests/FamilyHub.TestCommon/Fakes/FakeCalendarEventRepository.cs`

- Implements `ICalendarEventRepository`
- Constructor accepts optional `CalendarEvent?` for existing event
- Exposes `AddedEvents` list and `SaveChangesCallCount`

**Create domain tests:** `tests/FamilyHub.Calendar.Tests/Features/Calendar/Domain/CalendarEventAttendeeTests.cs`

- `AddAttendee_ShouldAddToCollection`
- `AddAttendee_ShouldRaiseEvent`
- `AddAttendee_ShouldThrow_WhenCancelled`
- `AddAttendee_ShouldThrow_WhenDuplicate`
- `RemoveAttendee_ShouldRemoveFromCollection`
- `RemoveAttendee_ShouldRaiseEvent`
- `RemoveAttendee_ShouldThrow_WhenCancelled`
- `RemoveAttendee_ShouldThrow_WhenNotFound`

**Create handler tests:**

- `tests/FamilyHub.Calendar.Tests/Features/Calendar/Application/AddAttendeeCommandHandlerTests.cs` (5 tests: happy path, event not found, not creator, cancelled, duplicate)
- `tests/FamilyHub.Calendar.Tests/Features/Calendar/Application/RemoveAttendeeCommandHandlerTests.cs` (5 tests: happy path, event not found, not creator, cancelled, attendee not found)

Test pattern reference: `tests/FamilyHub.Family.Tests/Features/Family/Application/SendInvitationCommandHandlerTests.cs`

---

## Task 5: Frontend GraphQL Operations + Service

**Modify** `src/frontend/family-hub-web/src/app/features/calendar/graphql/calendar.operations.ts`:

- Add `ADD_ATTENDEE` mutation (`addAttendee(eventId, userId)` → full `CalendarEventDto`)
- Add `REMOVE_ATTENDEE` mutation (`removeAttendee(eventId, userId)` → full `CalendarEventDto`)

**Modify** `src/frontend/family-hub-web/src/app/features/calendar/services/calendar.service.ts`:

- Add `addAttendee(eventId: string, userId: string)` method
- Add `removeAttendee(eventId: string, userId: string)` method
- Both follow existing pattern: `apollo.mutate()` → `pipe(map, catchError)`

---

## Task 6: Frontend People Picker UI

**Modify** `src/frontend/family-hub-web/src/app/features/calendar/components/event-context/event-context.component.ts`:

**New signals/computed:**

- `isAttendeeLoading = signal(false)`
- `createdBy = signal<string | null>(null)` — set in `initializeForm()` from `event.createdBy`
- `isCreator = computed(() => currentUser?.id === createdBy())`
- `availableMembers = computed(() => familyMembers filtered by not in selectedAttendees)`

**Template changes** (replace lines 124-140 — the display-only attendee section):

- Show attendee name badges with × remove button (visible only for creator + non-cancelled)
- Show `<select>` dropdown to add new attendees from available family members (creator + non-cancelled only)
- Loading state on buttons during mutation

**New methods:**

- `onAddAttendee(event: Event)` — calls `calendarService.addAttendee()`, updates `selectedAttendees` from response, emits `eventUpdated`
- `onRemoveAttendee(userId: string)` — calls `calendarService.removeAttendee()`, updates `selectedAttendees` from response, emits `eventUpdated`

Already available in the component: `CalendarService`, `UserService`, `ToastService`, `familyMembers` signal, `resolvedAttendees` computed, `selectedAttendees` signal.

---

## Files Summary

### New files (16)

| File | Type |
|------|------|
| `Domain/Events/AttendeeAddedToCalendarEventEvent.cs` | Domain event |
| `Domain/Events/AttendeeRemovedFromCalendarEventEvent.cs` | Domain event |
| `Commands/AddAttendee/AddAttendeeCommand.cs` | Command |
| `Commands/AddAttendee/AddAttendeeResult.cs` | Result |
| `Commands/AddAttendee/AddAttendeeCommandHandler.cs` | Handler |
| `Commands/AddAttendee/AddAttendeeCommandValidator.cs` | Validator |
| `Commands/AddAttendee/MutationType.cs` | GraphQL mutation |
| `Commands/RemoveAttendee/RemoveAttendeeCommand.cs` | Command |
| `Commands/RemoveAttendee/RemoveAttendeeResult.cs` | Result |
| `Commands/RemoveAttendee/RemoveAttendeeCommandHandler.cs` | Handler |
| `Commands/RemoveAttendee/RemoveAttendeeCommandValidator.cs` | Validator |
| `Commands/RemoveAttendee/MutationType.cs` | GraphQL mutation |
| `FakeCalendarEventRepository.cs` | Test fake |
| `CalendarEventAttendeeTests.cs` | Domain tests |
| `AddAttendeeCommandHandlerTests.cs` | Handler tests |
| `RemoveAttendeeCommandHandlerTests.cs` | Handler tests |

### Modified files (5)

| File | Change |
|------|--------|
| `DomainErrorCodes.cs` | 3 new constants |
| `CalendarEvent.cs` | 2 new domain methods |
| `calendar.operations.ts` | 2 new GraphQL mutations |
| `calendar.service.ts` | 2 new service methods |
| `event-context.component.ts` | Interactive people picker (replace display-only attendees) |

### Files NOT modified

- `CalendarModule.cs` — no new DI registrations needed (Mediator auto-discovers handlers, Hot Chocolate auto-discovers `[ExtendObjectType]`)
- `Program.cs` — no changes
- `ICalendarEventRepository.cs` — existing `GetByIdWithAttendeesAsync` is sufficient
- `CalendarMutations.cs` — new mutations use per-command MutationType pattern

---

## Verification

1. **Build:** `dotnet build src/FamilyHub.Api/FamilyHub.slnx` — should compile without errors
2. **Tests:** `dotnet test` — all existing 77 tests + ~18 new tests should pass
3. **Manual testing:**
   - Create a calendar event with attendees — verify attendees saved
   - Open event in context panel — see attendee badges with × buttons
   - Click × on an attendee — verify removed via API and UI updates
   - Use dropdown to add an attendee — verify added via API and UI updates
   - Try add/remove as non-creator — verify permission denied error
   - Try add/remove on cancelled event — verify error
   - Try adding duplicate attendee — verify error
4. **GraphQL playground:** Test `addAttendee` and `removeAttendee` mutations directly
