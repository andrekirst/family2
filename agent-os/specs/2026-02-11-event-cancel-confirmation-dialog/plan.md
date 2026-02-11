# Event Cancel Confirmation Dialog

**Issue:** #121
**Phase:** Phase 1 — MVP
**Type:** Frontend-only feature

## Summary

Add a confirmation dialog when canceling an event to prevent accidental deletion. This also establishes the first reusable shared UI component (`ConfirmationDialogComponent`) under `shared/components/`.

**Scope:** Frontend-only. Backend cancel command, handler, domain event, and GraphQL mutation are already implemented.

## Tasks

1. Spec documentation
2. Create reusable `ConfirmationDialogComponent` in `shared/components/`
3. Integrate confirmation dialog into `event-dialog.component.ts`
4. Write unit tests for both components
5. Verify all tests pass
