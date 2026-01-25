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

## Rules

- Always use `standalone: true`
- Import dependencies in `imports` array
- Use Angular Signals for state
- Follow atomic design for organization
