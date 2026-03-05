# Command Palette Pattern

Keyboard-first overlay (Ctrl+K) with signal-based state management.

## Architecture

- `CommandPaletteService` — Root-provided singleton managing state via Angular signals
- `CommandPaletteComponent` — Standalone component with inline template
- `NlpParserService` — Client-side NLP intent recognition

## State Signals

```typescript
readonly isOpen = signal(false);
readonly query = signal('');
readonly selectedIndex = signal(0);
readonly isLoading = signal(false);
readonly error = signal<string | null>(null);
readonly items = signal<PaletteItem[]>([]);
readonly hasItems = computed(() => this.items().length > 0);
```

## PaletteItem Types

`'nlp' | 'result' | 'command' | 'hint' | 'navigation'`

- `nlp` — NLP-parsed suggestion (client-side)
- `result` — Search result from backend
- `command` — Command from backend registry
- `hint` — Daily-rotating NLP example phrase
- `navigation` — Static navigation shortcut

## Default Items (Empty Query)

8 items shown when palette opens: 2 NLP hints (daily rotation), 2 quick actions, 4 navigation items.
Daily rotation: `Math.floor(Date.now() / 86_400_000) % poolSize`

## Keyboard Handling

- `Ctrl+K` / `Cmd+K` — Toggle palette
- `Escape` — Close
- `ArrowUp/Down` — Navigate items
- `Enter` — Execute selected
- `Tab` — Trapped (focus stays in modal)

## Search Flow

1. User types → 300ms debounce
2. NLP parser runs client-side (independent of API)
3. GraphQL search query fires to backend
4. Results merged: NLP suggestions + search results + commands

## Rules

- Use `inject()` for all DI (never constructor injection)
- Signal-based state (no RxJS subjects for UI state)
- LOCALE_ID injection for i18n hint pools (EN/DE)
- Hint click fills search input (teaches NLP usage)
