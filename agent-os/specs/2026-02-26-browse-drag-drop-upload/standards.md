# Standards for Browse Drag-and-Drop File Upload

The following standards apply to this work.

---

## frontend/angular-components

All components are standalone (no NgModules). Use atomic design hierarchy.

### Key Rules

- Always use `standalone: true`
- Import dependencies in `imports` array
- Use Angular Signals for state
- Follow atomic design for organization

### Relevant Pattern

```typescript
import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-example',
  standalone: true,
  imports: [CommonModule],
  template: `...`
})
export class ExampleComponent {
  isActive = signal(false);
}
```

### Application to This Feature

- `isDragging` state uses `signal(false)` — follows signals pattern
- `droppedFiles` uses `signal<File[]>([])` — follows signals pattern
- `initialFiles` on UploadDialogComponent uses `input<File[]>([])` — follows input signals pattern
- No new component files needed — overlay is inline template
- All modifications stay within existing standalone components
