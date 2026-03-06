# Standards for Command Palette Drill-Down Navigation

The following standards apply to this work.

---

## frontend/command-palette

Keyboard-first overlay (Ctrl+K) with signal-based state management.

### Architecture

- `CommandPaletteService` -- Root-provided singleton managing state via Angular signals
- `CommandPaletteComponent` -- Standalone component with inline template
- `NlpParserService` -- Client-side NLP intent recognition

### State Signals

```typescript
readonly isOpen = signal(false);
readonly query = signal('');
readonly selectedIndex = signal(0);
readonly isLoading = signal(false);
readonly error = signal<string | null>(null);
readonly items = signal<PaletteItem[]>([]);
readonly hasItems = computed(() => this.items().length > 0);
```

### Keyboard Handling

- `Ctrl+K` / `Cmd+K` -- Toggle palette
- `Escape` -- Close
- `ArrowUp/Down` -- Navigate items
- `Enter` -- Execute selected
- `Tab` -- Trapped (focus stays in modal)

### Rules

- Use `inject()` for all DI (never constructor injection)
- Signal-based state (no RxJS subjects for UI state)
- LOCALE_ID injection for i18n hint pools (EN/DE)
- Hint click fills search input (teaches NLP usage)

---

## frontend/angular-components

All components are standalone (no NgModules). Use atomic design hierarchy.

### Overlay/Modal Components

For keyboard-driven overlays (e.g., command palette):

- Use `@HostListener('document:keydown')` for global keyboard shortcuts
- Trap focus with Tab prevention
- Restore focus to `document.activeElement` on close
- Use signal-based service for state (not component state)
- Backdrop click detection: check `event.target === event.currentTarget`
- Use `viewChild()` signal queries for element refs
- Use `effect()` for auto-focus on open

### Rules

- Always use `standalone: true`
- Import dependencies in `imports` array
- Use Angular Signals for state
- Follow atomic design for organization
