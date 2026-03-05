# Angular Components

All components are standalone (no NgModules). Use atomic design hierarchy.

## Standalone Component

```typescript
import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-sidebar',
  standalone: true,  // Required!
  imports: [CommonModule, RouterModule],
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.css']
})
export class SidebarComponent {
  isCollapsed = signal(false);

  toggleSidebar() {
    this.isCollapsed.update(value => !value);
  }
}
```

## Atomic Design Hierarchy

- **Atoms:** Button, Input, Icon (basic building blocks)
- **Molecules:** FormField, SearchBar (atoms combined)
- **Organisms:** Sidebar, Header, Card (complex components)
- **Templates:** PageLayout (page structure without data)
- **Pages:** DashboardPage, FamilyPage (complete pages with data)

## File Organization

```
app/
├── components/
│   ├── atoms/
│   │   └── button/
│   ├── molecules/
│   │   └── form-field/
│   └── organisms/
│       └── sidebar/
└── pages/
    ├── dashboard/
    └── family/
```

## Overlay/Modal Components

For keyboard-driven overlays (e.g., command palette):

- Use `@HostListener('document:keydown')` for global keyboard shortcuts
- Trap focus with Tab prevention
- Restore focus to `document.activeElement` on close
- Use signal-based service for state (not component state)
- Backdrop click detection: check `event.target === event.currentTarget`
- Use `viewChild()` signal queries for element refs
- Use `effect()` for auto-focus on open

## Rules

- Always use `standalone: true`
- Import dependencies in `imports` array
- Use Angular Signals for state
- Follow atomic design for organization
