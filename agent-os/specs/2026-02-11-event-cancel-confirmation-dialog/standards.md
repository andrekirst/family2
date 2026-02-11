# Event Cancel Confirmation Dialog — Standards

## Patterns Applied

- **Standalone component** with `standalone: true` (no NgModules)
- **Angular Signals** for reactive state (`signal()`)
- **inject()** for dependency injection
- **Inline template** with Tailwind CSS utility classes
- **Modal overlay pattern:** `fixed inset-0 bg-black/50 z-50` with `$event.stopPropagation()`
- **data-testid** attributes on all interactive elements
- **Accessibility:** `role="dialog"`, `aria-modal="true"`, keyboard support
- **Service call pattern:** `isLoading` → subscribe → success/error handling
