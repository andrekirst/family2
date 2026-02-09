---
name: angular-component
description: Create standalone Angular component with signals and inject()
category: frontend
inputs:
  - componentName: kebab-case name (e.g., invite-member)
  - featureModule: Feature module (e.g., family)
  - hasGraphQL: Whether needs Apollo
---

# Angular Component Skill

Create a standalone Angular component using inject() and signals.

## Location

`src/frontend/family-hub-web/src/app/features/{feature}/components/{name}/{name}.component.ts`

## Component

```typescript
import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-{name}',
  standalone: true,
  imports: [CommonModule],
  template: `
    @if (isLoading()) {
      <div class="animate-pulse h-16 bg-gray-200 rounded"></div>
    } @else {
      @for (item of items(); track item.id) {
        <div class="p-4 bg-white border rounded-lg">
          {{ item.name }}
        </div>
      }
    }
  `,
})
export class {Name}Component implements OnInit {
  private service = inject({Feature}Service);
  items = signal<ItemType[]>([]);
  isLoading = signal(true);

  ngOnInit(): void { this.loadItems(); }
}
```

## With Permissions

```typescript
permissions = inject(FamilyPermissionService);

// In template:
@if (permissions.canInvite()) {
  <button (click)="openInviteDialog()">Invite</button>
}
```

Note: `permissions` is public (no `private`) because template accesses it.

## Validation

- [ ] Uses standalone: true
- [ ] Uses inject() (not constructor injection)
- [ ] Uses signals for state
- [ ] Uses @if/@for control flow
- [ ] Uses Tailwind CSS classes
- [ ] Permission service public when used in template
