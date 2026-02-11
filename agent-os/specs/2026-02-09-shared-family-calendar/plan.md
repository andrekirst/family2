# Shared Family Calendar — Month View MVP

**Issue:** #116
**Phase:** Phase 1 — Critical Path
**RICE Score:** 48-50

## Plan

See the implementation plan in the GitHub issue #116 conversation or the plan mode transcript.

## Summary

Calendar is the backbone of 5 event chains — the project's flagship differentiator. This implements CRUD for calendar events with a month grid view in the frontend.

**MVP scope:** Month view only, no recurring events. Create/view/edit/cancel events with attendee assignment and color coding by event type. Custom Tailwind CSS grid (not FullCalendar).

## Tasks

1. Spec documentation
2. Backend domain model (CalendarEvent aggregate, value objects, domain events)
3. Backend persistence (EF Core config, repository, migrations, RLS)
4. Backend application layer (commands, queries, handlers, validators, mappers)
5. Backend GraphQL (mutations, queries, request models, DI registration)
6. Frontend service + GraphQL operations
7. Frontend calendar page + month grid component
8. Frontend event dialog (create/edit)
9. E2E tests (Playwright)
