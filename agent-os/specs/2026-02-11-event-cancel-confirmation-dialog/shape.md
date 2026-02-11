# Event Cancel Confirmation Dialog — Shape

## Components

### ConfirmationDialogComponent (NEW)

- **Path:** `src/frontend/family-hub-web/src/app/shared/components/confirmation-dialog/confirmation-dialog.component.ts`
- **Type:** Standalone Angular component, inline template with Tailwind CSS
- **Inputs:** `title`, `message`, `confirmLabel`, `cancelLabel`, `variant`, `isLoading`
- **Outputs:** `confirmed`, `cancelled`
- **Behavior:** Modal overlay with dismiss via overlay click, close button, Escape key

### EventDialogComponent (MODIFIED)

- **Path:** `src/frontend/family-hub-web/src/app/features/calendar/components/event-dialog/event-dialog.component.ts`
- **Changes:** Add `showCancelConfirmation` signal, refactor `onCancel()` → show dialog, add `onCancelConfirmed()` and `onCancelDismissed()`

## Data Flow

```
User clicks "Cancel Event"
  → showCancelConfirmation.set(true)
  → ConfirmationDialogComponent appears with event name
  → User clicks "Cancel Event" (confirm)
    → calendarService.cancelCalendarEvent(id)
    → eventCancelled.emit()
  → User clicks "Go Back" / overlay / Escape
    → showCancelConfirmation.set(false)
```

## Accessibility

- `role="dialog"`, `aria-modal="true"`, `aria-labelledby`
- Escape key dismisses
- `data-testid` attributes for E2E testing
