# Command Palette Drill-Down Navigation -- Shaping Notes

**Feature**: Spotlight-style hierarchical sub-item navigation in the command palette
**Created**: 2026-03-06
**GitHub Issue**: #220

---

## Scope

Enhance the existing command palette (`Ctrl+K`) to support macOS Spotlight-style drill-down navigation:

- **Full page replace**: Selecting a group replaces the entire results list with its children
- **Back navigation**: Left arrow button + `ArrowLeft`/`Backspace` to go up one level
- **Breadcrumb trail**: Shows navigation path (e.g., "Calendar > Events") when drilled in
- **Scoped search**: When drilled into a group, search filters within that group's children (client-side)
- **Unlimited depth**: Any item can have children, supporting arbitrary nesting
- **Slide animation**: Smooth CSS transition (left when drilling in, right when going back)

Frontend-only changes -- no backend modifications required.

## Decisions

- **Drill-down UX**: Full page replace (not accordion or side panel) -- matches macOS Spotlight pattern the user referenced
- **Nesting depth**: Unlimited -- data model supports recursive `PaletteItem` children
- **Animation**: CSS-only transitions with Tailwind (no `@angular/animations` dependency) -- keeps bundle lean
- **Registry extraction**: `PaletteRegistryService` separates hierarchy definition from palette state management -- cleaner SRP
- **Scoped search**: Client-side filtering when drilled in (no GraphQL call) -- instant, items are already loaded
- **Backspace behavior**: Only triggers "go back" when search query is empty -- avoids conflict with typing
- **Backward compatible**: `children` and `isGroup` are optional on `PaletteItem` -- existing flat items work unchanged

## Context

- **Visuals**: macOS Spotlight sub-item drill-down screenshot (https://i0.wp.com/chriscoyier.net/wp-content/uploads/2022/12/spotlight2.jpg)
- **References**: Existing command palette at `src/frontend/family-hub-web/src/app/shared/components/command-palette/` and related spec at `agent-os/specs/2026-03-03-universal-search-command-palette/`
- **Product alignment**: Enhances the existing command palette (Phase 1 feature) with hierarchical navigation for better discoverability of module-specific actions

## Standards Applied

- `frontend/command-palette` -- Extends the existing keyboard-first command palette with signal-based state
- `frontend/angular-components` -- Standalone components, `inject()` DI, signal-based state, overlay/modal patterns
