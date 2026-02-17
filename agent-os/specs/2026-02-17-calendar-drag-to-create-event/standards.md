# Standards for Calendar Drag-to-Create Event

The following standards apply to this work.

---

## frontend/angular-components

All components are standalone (no NgModules). Use atomic design hierarchy.

### Standalone Component

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

### Atomic Design Hierarchy

- **Atoms:** Button, Input, Icon (basic building blocks)
- **Molecules:** FormField, SearchBar (atoms combined)
- **Organisms:** Sidebar, Header, Card (complex components)
- **Templates:** PageLayout (page structure without data)
- **Pages:** DashboardPage, FamilyPage (complete pages with data)

### Rules

- Always use `standalone: true`
- Import dependencies in `imports` array
- Use Angular Signals for state
- Follow atomic design for organization

---

## testing/unit-testing

xUnit + FluentAssertions with fake repository pattern (backend). Jest for Angular frontend tests.

### Rules

- FluentAssertions for all assertions (never xUnit Assert)
- Fake repositories as inner classes (NSubstitute migration planned)
- Arrange-Act-Assert pattern
- Call static `Handler.Handle()` directly with fakes

### Frontend Testing Notes

For this feature, Angular component tests use:

- `TestBed.configureTestingModule()` with standalone component imports
- `fixture.detectChanges()` for change detection
- `dispatchEvent(new MouseEvent(...))` for simulating drag interactions
- `jest.spyOn()` for output event emission verification
