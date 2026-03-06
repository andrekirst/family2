# References for Command Palette Drill-Down Navigation

## Similar Implementations

### Existing Command Palette

- **Location**: `src/frontend/family-hub-web/src/app/shared/components/command-palette/command-palette.component.ts`
- **Relevance**: This is the component being enhanced -- already has keyboard navigation, search, NLP, grouped results
- **Key patterns**: Signal-based state via `CommandPaletteService`, `@HostListener` for keyboard, Tailwind inline template

### Command Palette Service

- **Location**: `src/frontend/family-hub-web/src/app/shared/services/command-palette.service.ts`
- **Relevance**: State management service that will be extended with navigation stack
- **Key patterns**: `signal()` for state, `computed()` for derived state, `getDefaultItems()` for initial items

### Search Models

- **Location**: `src/frontend/family-hub-web/src/app/shared/models/search.models.ts`
- **Relevance**: `PaletteItem` interface that will be extended with `children` and `isGroup`
- **Key patterns**: Type-discriminated items (`PaletteItemType`)

### Universal Search Command Palette Spec

- **Location**: `agent-os/specs/2026-03-03-universal-search-command-palette/`
- **Relevance**: Original spec for the command palette -- this drill-down feature builds on top of it
- **Key patterns**: Provider registry pattern, hybrid module boundary, NLP parsing

### NLP Parser

- **Location**: `src/frontend/family-hub-web/src/app/core/nlp/`
- **Relevance**: Client-side NLP that remains unchanged but coexists with drill-down navigation
- **Key patterns**: Rule-based parsing, locale-aware (EN/DE), confidence scoring

## External References

### macOS Spotlight

- **Visual reference**: https://i0.wp.com/chriscoyier.net/wp-content/uploads/2022/12/spotlight2.jpg
- **Relevance**: The UX pattern being implemented -- full page replace with back navigation when drilling into categories
- **Key patterns**: Category groups with chevron indicators, breadcrumb-style back navigation, scoped search within category
