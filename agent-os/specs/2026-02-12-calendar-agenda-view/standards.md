# Standards for Calendar Agenda View

The following standards apply to this work.

---

## frontend/angular-components

All components are standalone (no NgModules). Use atomic design hierarchy.

### Standalone Component

```typescript
import { Component, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-calendar-agenda',
  standalone: true,  // Required!
  imports: [CommonModule],
  template: `...`
})
export class CalendarAgendaComponent {
  events = signal<CalendarEventDto[]>([]);
  dayGroups = computed(() => groupEventsByDay(this.events(), this.batchCount()));
}
```

### Signal-Backed Inputs Pattern

```typescript
// Bridge Angular @Input to internal signal
events = signal<CalendarEventDto[]>([]);

@Input() set eventsInput(value: CalendarEventDto[]) {
  this.events.set(value);
}

// Derive state with computed
dayGroups = computed(() => groupEventsByDay(this.events(), this.batchCount()));
```

### Atomic Design Hierarchy

- **Atoms:** Button, Input, Icon (basic building blocks)
- **Molecules:** FormField, SearchBar (atoms combined)
- **Organisms:** CalendarAgendaComponent, CalendarViewSwitcher (complex components)
- **Templates:** PageLayout (page structure without data)
- **Pages:** CalendarPageComponent (complete page with data orchestration)

### Rules

- Always use `standalone: true`
- Import dependencies in `imports` array
- Use Angular Signals for state
- Follow atomic design for organization
- Use `data-testid` attributes on key interactive elements
- Use `i18n` attributes with `@@` IDs for translateable text

---

## testing/unit-testing

Vitest for Angular frontend tests. Follows existing `week.utils.spec.ts` patterns.

### Rules

- `vi.useFakeTimers()` / `vi.setSystemTime()` for deterministic date testing
- `localStorage` setup/teardown in `beforeEach`/`afterEach` for locale/format prefs
- Shared `makeEvent()` helper for creating test event DTOs
- Descriptive `describe` blocks per function, focused `it` blocks per behavior
- Test edge cases: empty inputs, cancelled events, cross-day spans, locale variants

### Test Pattern

```typescript
describe('groupEventsByDay', () => {
  beforeEach(() => {
    vi.useFakeTimers();
    vi.setSystemTime(new Date(2026, 1, 25, 10, 0));
    localStorage.setItem(LOCALE_STORAGE_KEY, 'en');
  });
  afterEach(() => {
    vi.useRealTimers();
    localStorage.removeItem(LOCALE_STORAGE_KEY);
  });

  it('returns empty array when no events', () => {
    const result = groupEventsByDay([], 1);
    expect(result).toEqual([]);
  });
});
```
