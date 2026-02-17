# Calendar Drag-to-Create Event — Shaping Notes

## Scope

As a family member, I want to create a calendar event by marking (dragging) a time range in the week grid calendar view. This is a frontend-only interaction feature that connects to the existing backend event creation infrastructure.

**In scope:**

- Drag-to-select time range on the week grid (mousedown -> drag -> mouseup)
- Blue semi-transparent overlay with time labels during drag
- Context panel opens with pre-filled start/end times from the selection
- 15-minute snap intervals (matches existing SNAP_MINUTES constant)
- Single-column constraint (no cross-day dragging)
- Bidirectional drag support (dragging upward swaps start/end)
- Existing single-click behavior preserved (< 15px movement = click)

**Out of scope:**

- Touch/mobile support (follow-up)
- Month grid drag-to-create (follow-up)
- Cross-day drag selection
- Backend changes (all CRUD already exists)
- Recurring events
- External calendar sync

## Decisions

- **Interaction model:** Context panel (side panel) opens after drag, not a modal dialog. Matches existing click-to-create flow.
- **Event fields (MVP):** Title, Description, Start/End datetime, All-day toggle. Core fields only.
- **Click vs. drag threshold:** 15px minimum drag distance (= 15 minutes at 60px/hour). Below threshold, falls back to existing click behavior.
- **Visual feedback:** Blue semi-transparent overlay (bg-blue-500/20) with border, showing start and end time labels. z-index 30 to sit above events (z-10) but below modals.
- **Document-level listeners:** mousemove and mouseup are attached to `document` during drag (not the component) to handle cursor leaving the grid area gracefully.
- **Arrow functions for handlers:** `onMouseMove` and `onMouseUp` are class arrow properties to preserve `this` binding when used as document event listeners.

## Context

- **Visuals:** Google Calendar's drag-to-create + side panel as reference model
- **References:** Existing CalendarWeekGridComponent, EventContextComponent, CalendarPageComponent
- **Product alignment:** Calendar is Phase 1 Critical Path (Issue #116, RICE 48-50). This enhances the existing calendar UX.

## Standards Applied

- **frontend/angular-components** — Signals for state, standalone components, atomic design
- **testing/unit-testing** — Jest/xUnit test patterns for the drag interaction
