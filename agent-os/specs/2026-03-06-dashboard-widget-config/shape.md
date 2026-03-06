# Dashboard Widget Configuration -- Shaping Notes

**Feature**: Per-widget configuration for Welcome and Upcoming Events widgets
**Created**: 2026-03-06
**GitHub Issue**: #218

---

## Scope

Add configurable behavior to two existing dashboard widgets:

1. **Welcome Widget** (`dashboard:welcome`): Personalized greeting with custom prefix text and optional time-based greeting (Good morning/afternoon/evening)
2. **Upcoming Events Widget** (`family:upcoming-events`): Configurable event display with days-ahead filter, entry limit, family/personal toggle, time visibility, and compact/detailed view modes

The configuration UI uses a gear icon in the widget container header (visible in edit mode), opening a settings panel between the header and widget body. Config is persisted immediately via the existing `UpdateWidgetConfig` GraphQL mutation.

## Decisions

- **Config UI trigger**: Gear icon in widget header (edit mode only) -- consistent with existing remove button pattern
- **Config schema format**: Informational JSON (not JSON Schema validation) -- sufficient for 2 widgets, can evolve later
- **firstName extraction**: `user.name.split(' ')[0]` from `UserService.currentUser()` -- simple, handles "John Doe" -> "John"
- **Mock data for events**: Hardcoded sample events with "Sample data" badge -- respects config options (filtering, view mode) so users can verify settings work
- **Config persistence**: Immediate save via `UpdateWidgetConfig` mutation, not deferred to layout save button
- **Time-based greeting**: Static computation on render (no timer-based refresh) -- acceptable for MVP
- **No new backend changes needed**: Existing `UpdateWidgetConfigCommand`, handler, and mutation are fully functional

## Context

- **Visuals**:
  - https://cdn.dribbble.com/userupload/4269902/file/original-e8c3daab7595ab549d1b8fe7546545ce.jpg
  - https://cdn.dribbble.com/userupload/19555367/file/original-f554cab19ce7554712cf167c0ae60bf5.png
- **References**: Existing widget implementations (welcome-widget, upcoming-events-widget, widget-container)
- **Product alignment**: N/A (no product folder)

## Standards Applied

- **graphql-input-command** -- UpdateWidgetConfig follows Input -> Command pattern (already implemented)
- **angular-components** -- Widget components use standalone pattern with signals
- **apollo-graphql** -- Frontend config persistence uses Apollo mutations
- **vogen-value-objects** -- Backend widget type IDs use Vogen VOs (WidgetTypeId)
