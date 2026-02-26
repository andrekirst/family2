# Calendar Event Attendee Management — Shaping Notes

## Scope

As a family member, I want to manage attendees of a calendar event. This adds dedicated AddAttendee and RemoveAttendee commands so attendees can be individually added or removed without editing the entire event.

**In scope:**

- AddAttendee command (backend + GraphQL mutation)
- RemoveAttendee command (backend + GraphQL mutation)
- Creator-only permission enforcement (only `event.CreatedBy` can manage attendees)
- Domain methods on CalendarEvent aggregate with proper invariant checks
- Domain events for attendee changes
- People picker UI in the event context panel (inline, not a separate page)
- Unit tests for domain logic and command handlers

**Out of scope (future):**

- RSVP / attendee response status (Accepted, Declined, Maybe, Pending)
- Notifications when attendees are added/removed
- Event chain integration (auto-adding attendees from other modules)
- Attendee capacity limits
- Non-creator attendee management (e.g., admins managing attendees)

## Decisions

- **Add/Remove only, no RSVP:** Keep it simple — just a list of user IDs with add/remove operations
- **Creator-only permissions:** Only the user who created the event (`CreatedBy`) can manage attendees. No role-based checks beyond creator identity.
- **Subfolder command pattern:** New commands use the recommended subfolder layout (`Commands/AddAttendee/`) even though existing Calendar commands use flat files. This is a progressive migration.
- **Domain invariants in aggregate:** All business rules (cancelled check, duplicate check, not-found check) live in `CalendarEvent.AddAttendee()` / `RemoveAttendee()` methods, keeping handlers thin.
- **People picker UI:** A dropdown-to-add + chip-with-×-to-remove pattern in the event context panel. Simple and inline, not a separate dialog.

## Context

- **Visuals:** People picker style UI (dropdown to add, chips with × to remove)
- **References:** Family module's FamilyMember entity management pattern, SendInvitation command subfolder pattern
- **Product alignment:** Calendar is a Core Domain feature in Phase 1 MVP. Attendee management is a natural extension of the existing event creation flow.

## Standards Applied

- **backend/graphql-input-command** — New commands follow Input→Command→Handler pattern with per-command MutationType
- **backend/domain-events** — Sealed records extending DomainEvent, raised via RaiseDomainEvent()
- **backend/permission-system** — Creator-only check (simpler than role-based, but same enforcement pattern)
- **backend/vogen-value-objects** — CalendarEventId and UserId used in commands (no primitives)
- **backend/user-context** — IUserService.GetCurrentUser() for resolving current user in MutationType
- **database/ef-core-migrations** — No schema change needed (existing join table is sufficient)
- **frontend/angular-components** — Standalone component with signals
- **frontend/apollo-graphql** — Typed mutation operations
- **testing/unit-testing** — xUnit + FluentAssertions + fake repository pattern
