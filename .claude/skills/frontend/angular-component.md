---
name: angular-component
description: Create a standalone Angular component following atomic design
category: frontend
inputs:
  - componentName: Component name in kebab-case (e.g., family-card)
  - atomicLevel: atoms, molecules, organisms, or pages
  - hasGraphQL: Whether component needs GraphQL queries
---

# Angular Component Skill

Creates a standalone Angular component following atomic design patterns.

## Files Created

1. `{atomicLevel}/{component-name}/{component-name}.component.ts`
2. `{atomicLevel}/{component-name}/{component-name}.component.html`
3. `{atomicLevel}/{component-name}/{component-name}.component.css`
4. `{atomicLevel}/{component-name}/{component-name}.component.spec.ts`

## Step 1: Create Component Class

Location: `src/app/components/{atomicLevel}/{component-name}/{component-name}.component.ts`

```typescript
import { Component, signal, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-{component-name}',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './{component-name}.component.html',
  styleUrls: ['./{component-name}.component.css']
})
export class {ComponentName}Component {
  // Input signals
  data = input<DataType>();

  // Output events
  action = output<ActionType>();

  // Internal state
  isLoading = signal(false);

  onAction() {
    this.action.emit({ ... });
  }
}
```

**Rules:**

- Always use `standalone: true`
- Use Angular Signals for state (`signal()`, `input()`, `output()`)
- Use `inject()` for dependency injection

## Step 2: Create Template

Location: `src/app/components/{atomicLevel}/{component-name}/{component-name}.component.html`

```html
<div class="component-wrapper">
  @if (isLoading()) {
    <div class="loading">Loading...</div>
  } @else {
    <div class="content">
      {{ data()?.property }}
    </div>
  }
</div>
```

**Rules:**

- Use `@if`, `@for`, `@switch` control flow (Angular 17+)
- Call signals as functions: `data()`, `isLoading()`
- Use Tailwind CSS classes

## Step 3: Create Styles (Tailwind)

Location: `src/app/components/{atomicLevel}/{component-name}/{component-name}.component.css`

```css
/* Component-specific styles only */
/* Prefer Tailwind classes in template */
:host {
  display: block;
}
```

## Step 4: Create Unit Test

Location: `src/app/components/{atomicLevel}/{component-name}/{component-name}.component.spec.ts`

```typescript
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { {ComponentName}Component } from './{component-name}.component';

describe('{ComponentName}Component', () => {
  let component: {ComponentName}Component;
  let fixture: ComponentFixture<{ComponentName}Component>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [{ComponentName}Component]
    }).compileComponents();

    fixture = TestBed.createComponent({ComponentName}Component);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
```

## With GraphQL (if hasGraphQL=true)

Add Apollo Client integration:

```typescript
import { Apollo, gql } from 'apollo-angular';
import { inject } from '@angular/core';

const GET_DATA = gql`
  query GetData {
    data { id name }
  }
`;

@Component({ ... })
export class {ComponentName}Component {
  private apollo = inject(Apollo);

  data$ = this.apollo.query({
    query: GET_DATA
  }).pipe(
    map(result => result.data.data),
    catchError(error => {
      console.error('Error:', error);
      return of(null);
    })
  );
}
```

## Atomic Design Level Guide

- **atoms/**: Button, Input, Icon, Label (basic building blocks)
- **molecules/**: FormField, SearchBar, Card (atoms combined)
- **organisms/**: Header, Sidebar, DataTable (complex components)
- **pages/**: DashboardPage, FamilyPage (route-level components)

## Verification

- [ ] Component uses `standalone: true`
- [ ] Uses Angular Signals (not BehaviorSubject)
- [ ] Uses new control flow (`@if`, `@for`)
- [ ] Follows atomic design hierarchy
- [ ] Unit test passes
