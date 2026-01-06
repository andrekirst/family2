# Family Hub - Angular Component Specifications

**Version:** 1.0
**Date:** 2025-12-19
**Status:** Technical Specification
**Author:** UI Designer (Claude Code)

---

## Table of Contents

1. [Component Architecture](#component-architecture)
2. [Atomic Design Structure](#atomic-design-structure)
3. [Component Catalog](#component-catalog)
4. [Component APIs](#component-apis)
5. [Theming Implementation](#theming-implementation)
6. [Code Examples](#code-examples)

---

## Component Architecture

### Technology Stack

```json
{
  "framework": "Angular v21",
  "language": "TypeScript 5.3+",
  "styling": "Tailwind CSS 3.4+",
  "icons": "@heroicons/angular",
  "stateManagement": "Angular Signals (built-in)",
  "formHandling": "Angular Reactive Forms",
  "testing": "Jest + Testing Library"
}
```

### Project Structure

```
src/
├── app/
│   ├── core/                    # Core services, guards, interceptors
│   │   ├── services/
│   │   ├── guards/
│   │   └── interceptors/
│   │
│   ├── shared/                  # Shared module
│   │   ├── components/          # Shared components
│   │   │   ├── atoms/           # Atomic components
│   │   │   ├── molecules/       # Molecule components
│   │   │   └── organisms/       # Organism components
│   │   ├── directives/
│   │   ├── pipes/
│   │   └── shared.module.ts
│   │
│   ├── features/                # Feature modules
│   │   ├── calendar/
│   │   ├── tasks/
│   │   ├── shopping/
│   │   └── dashboard/
│   │
│   └── layouts/                 # Layout components
│       ├── main-layout/
│       ├── auth-layout/
│       └── mobile-layout/
│
├── assets/
│   ├── images/
│   ├── icons/
│   └── illustrations/
│
└── styles/
    ├── tailwind.css
    ├── themes/
    │   ├── light-theme.css
    │   └── dark-theme.css
    └── components/
        └── custom-components.css
```

---

## Atomic Design Structure

### Atoms (Smallest Components)

**Definition**: Basic building blocks that can't be broken down further.

```
atoms/
├── button/
│   ├── button.component.ts
│   ├── button.component.html
│   ├── button.component.spec.ts
│   └── button.types.ts
│
├── icon/
├── badge/
├── avatar/
├── input/
├── checkbox/
├── radio/
├── toggle/
├── spinner/
└── divider/
```

### Molecules (Simple Combinations)

**Definition**: Groups of atoms functioning together as a unit.

```
molecules/
├── form-field/          # Label + Input + Error
├── search-bar/          # Icon + Input + Clear button
├── list-item/           # Checkbox + Text + Icon
├── card-header/         # Title + Badge + Action button
├── notification-item/   # Icon + Text + Timestamp + Close
└── progress-indicator/  # Label + Progress bar + Percentage
```

### Organisms (Complex Components)

**Definition**: Complex UI components composed of atoms and molecules.

```
organisms/
├── navigation/
│   ├── top-nav/
│   ├── bottom-nav/
│   └── sidebar/
│
├── modal/
├── toast-container/
├── calendar-month-view/
├── shopping-list/
├── task-list/
└── event-chain-builder/
```

### Templates & Pages

**Templates**: Page-level layouts without specific content.
**Pages**: Specific instances of templates with real content.

```
layouts/
├── dashboard-template/
├── detail-view-template/
└── list-view-template/

pages/
├── calendar-page/
├── tasks-page/
└── shopping-lists-page/
```

---

## Component Catalog

### Atom Components

#### 1. Button Component

**File**: `src/app/shared/components/atoms/button/button.component.ts`

```typescript
import { Component, Input, Output, EventEmitter } from '@angular/core';

export type ButtonVariant = 'primary' | 'secondary' | 'tertiary' | 'danger';
export type ButtonSize = 'sm' | 'md' | 'lg';

@Component({
  selector: 'fh-button',
  standalone: true,
  template: `
    <button
      [type]="type"
      [disabled]="disabled || loading"
      [class]="buttonClasses"
      (click)="handleClick($event)"
    >
      @if (loading) {
        <fh-spinner [size]="spinnerSize" class="mr-2" />
      }
      @if (iconLeft && !loading) {
        <fh-icon [name]="iconLeft" [size]="iconSize" class="mr-2" />
      }
      <ng-content />
      @if (iconRight) {
        <fh-icon [name]="iconRight" [size]="iconSize" class="ml-2" />
      }
    </button>
  `,
})
export class ButtonComponent {
  @Input() variant: ButtonVariant = 'primary';
  @Input() size: ButtonSize = 'md';
  @Input() type: 'button' | 'submit' | 'reset' = 'button';
  @Input() disabled = false;
  @Input() loading = false;
  @Input() fullWidth = false;
  @Input() iconLeft?: string;
  @Input() iconRight?: string;

  @Output() clicked = new EventEmitter<MouseEvent>();

  get buttonClasses(): string {
    const baseClasses = 'inline-flex items-center justify-center font-medium rounded-md focus:outline-none focus:ring-2 focus:ring-offset-2 transition-colors duration-200 disabled:opacity-50 disabled:cursor-not-allowed';

    const variantClasses = {
      primary: 'bg-blue-600 text-white hover:bg-blue-700 focus:ring-blue-500',
      secondary: 'bg-blue-50 text-blue-700 border border-blue-200 hover:bg-blue-100 focus:ring-blue-500',
      tertiary: 'bg-transparent text-gray-700 hover:bg-gray-100 focus:ring-gray-500',
      danger: 'bg-red-600 text-white hover:bg-red-700 focus:ring-red-500',
    };

    const sizeClasses = {
      sm: 'px-3 py-1.5 text-sm',
      md: 'px-4 py-2 text-base',
      lg: 'px-6 py-3 text-lg',
    };

    const widthClass = this.fullWidth ? 'w-full' : '';

    return `${baseClasses} ${variantClasses[this.variant]} ${sizeClasses[this.size]} ${widthClass}`;
  }

  get iconSize(): 'sm' | 'md' {
    return this.size === 'sm' ? 'sm' : 'md';
  }

  get spinnerSize(): 'sm' | 'md' {
    return this.size === 'sm' ? 'sm' : 'md';
  }

  handleClick(event: MouseEvent): void {
    if (!this.disabled && !this.loading) {
      this.clicked.emit(event);
    }
  }
}
```

**Usage**:

```html
<!-- Primary button -->
<fh-button variant="primary" (clicked)="handleSave()">
  Save Changes
</fh-button>

<!-- Button with icon -->
<fh-button variant="primary" iconLeft="plus" (clicked)="createTask()">
  Create Task
</fh-button>

<!-- Loading state -->
<fh-button variant="primary" [loading]="isSaving" (clicked)="handleSave()">
  Save
</fh-button>

<!-- Full width -->
<fh-button variant="primary" [fullWidth]="true">
  Continue
</fh-button>
```

#### 2. Icon Component

```typescript
import { Component, Input } from '@angular/core';

@Component({
  selector: 'fh-icon',
  standalone: true,
  template: `
    <svg
      [class]="iconClasses"
      fill="none"
      stroke="currentColor"
      viewBox="0 0 24 24"
      aria-hidden="true"
    >
      <use [attr.href]="'#icon-' + name" />
    </svg>
  `,
})
export class IconComponent {
  @Input() name!: string;
  @Input() size: 'xs' | 'sm' | 'md' | 'lg' | 'xl' = 'md';
  @Input() color?: string;

  get iconClasses(): string {
    const sizeClasses = {
      xs: 'w-4 h-4',
      sm: 'w-5 h-5',
      md: 'w-6 h-6',
      lg: 'w-8 h-8',
      xl: 'w-12 h-12',
    };

    const colorClass = this.color ? `text-${this.color}` : '';

    return `${sizeClasses[this.size]} ${colorClass}`;
  }
}
```

#### 3. Input Component

```typescript
import { Component, Input, Output, EventEmitter, forwardRef } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

@Component({
  selector: 'fh-input',
  standalone: true,
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => InputComponent),
      multi: true,
    },
  ],
  template: `
    <div class="relative">
      @if (label) {
        <label [for]="id" class="block text-sm font-medium text-gray-700 mb-1">
          {{ label }}
          @if (required) {
            <span class="text-red-500">*</span>
          }
        </label>
      }

      <div class="relative">
        @if (iconLeft) {
          <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
            <fh-icon [name]="iconLeft" size="sm" class="text-gray-400" />
          </div>
        }

        <input
          [id]="id"
          [type]="type"
          [placeholder]="placeholder"
          [disabled]="disabled"
          [required]="required"
          [class]="inputClasses"
          [(ngModel)]="value"
          (blur)="onTouched()"
          (input)="onChange($event)"
        />

        @if (iconRight || clearable && value) {
          <div class="absolute inset-y-0 right-0 pr-3 flex items-center">
            @if (clearable && value) {
              <button
                type="button"
                (click)="clear()"
                class="text-gray-400 hover:text-gray-600"
              >
                <fh-icon name="x-mark" size="sm" />
              </button>
            } @else if (iconRight) {
              <fh-icon [name]="iconRight" size="sm" class="text-gray-400" />
            }
          </div>
        }
      </div>

      @if (error) {
        <p class="mt-1 text-sm text-red-600">{{ error }}</p>
      }

      @if (hint && !error) {
        <p class="mt-1 text-sm text-gray-500">{{ hint }}</p>
      }
    </div>
  `,
})
export class InputComponent implements ControlValueAccessor {
  @Input() id = `input-${Math.random().toString(36).substr(2, 9)}`;
  @Input() label?: string;
  @Input() type: 'text' | 'email' | 'password' | 'number' | 'tel' | 'url' = 'text';
  @Input() placeholder = '';
  @Input() disabled = false;
  @Input() required = false;
  @Input() error?: string;
  @Input() hint?: string;
  @Input() iconLeft?: string;
  @Input() iconRight?: string;
  @Input() clearable = false;

  @Output() valueChange = new EventEmitter<string>();

  value = '';

  onChange: any = () => {};
  onTouched: any = () => {};

  get inputClasses(): string {
    const baseClasses = 'block w-full px-3 py-2 text-base text-gray-900 placeholder-gray-400 bg-white border rounded-md focus:outline-none focus:ring-2 focus:border-transparent disabled:bg-gray-100 disabled:cursor-not-allowed transition-colors duration-200';

    const stateClasses = this.error
      ? 'border-red-500 focus:ring-red-500'
      : 'border-gray-300 focus:ring-blue-500';

    const paddingClasses = [];
    if (this.iconLeft) paddingClasses.push('pl-10');
    if (this.iconRight || this.clearable) paddingClasses.push('pr-10');

    return `${baseClasses} ${stateClasses} ${paddingClasses.join(' ')}`;
  }

  writeValue(value: string): void {
    this.value = value;
  }

  registerOnChange(fn: any): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: any): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled = isDisabled;
  }

  clear(): void {
    this.value = '';
    this.onChange(this.value);
    this.valueChange.emit(this.value);
  }
}
```

#### 4. Badge Component

```typescript
import { Component, Input } from '@angular/core';

export type BadgeVariant = 'success' | 'error' | 'warning' | 'info' | 'neutral';
export type BadgeSize = 'sm' | 'md' | 'lg';

@Component({
  selector: 'fh-badge',
  standalone: true,
  template: `
    <span [class]="badgeClasses">
      @if (icon) {
        <fh-icon [name]="icon" size="xs" class="mr-1" />
      }
      <ng-content />
    </span>
  `,
})
export class BadgeComponent {
  @Input() variant: BadgeVariant = 'neutral';
  @Input() size: BadgeSize = 'md';
  @Input() icon?: string;

  get badgeClasses(): string {
    const baseClasses = 'inline-flex items-center font-medium rounded-full';

    const variantClasses = {
      success: 'bg-green-100 text-green-800',
      error: 'bg-red-100 text-red-800',
      warning: 'bg-amber-100 text-amber-800',
      info: 'bg-blue-100 text-blue-800',
      neutral: 'bg-gray-100 text-gray-800',
    };

    const sizeClasses = {
      sm: 'px-2 py-0.5 text-xs',
      md: 'px-2.5 py-0.5 text-sm',
      lg: 'px-3 py-1 text-base',
    };

    return `${baseClasses} ${variantClasses[this.variant]} ${sizeClasses[this.size]}`;
  }
}
```

### Molecule Components

#### 1. Form Field Component

```typescript
import { Component, Input } from '@angular/core';

@Component({
  selector: 'fh-form-field',
  standalone: true,
  imports: [InputComponent],
  template: `
    <div class="mb-4">
      <fh-input
        [id]="id"
        [label]="label"
        [type]="type"
        [placeholder]="placeholder"
        [disabled]="disabled"
        [required]="required"
        [error]="error"
        [hint]="hint"
        [iconLeft]="iconLeft"
        [iconRight]="iconRight"
        [clearable]="clearable"
        [(ngModel)]="value"
      />
    </div>
  `,
})
export class FormFieldComponent {
  @Input() id!: string;
  @Input() label!: string;
  @Input() type = 'text';
  @Input() placeholder = '';
  @Input() disabled = false;
  @Input() required = false;
  @Input() error?: string;
  @Input() hint?: string;
  @Input() iconLeft?: string;
  @Input() iconRight?: string;
  @Input() clearable = false;

  value = '';
}
```

#### 2. Card Component

```typescript
import { Component, Input } from '@angular/core';

@Component({
  selector: 'fh-card',
  standalone: true,
  template: `
    <div [class]="cardClasses">
      @if (header) {
        <div class="px-6 py-4 border-b border-gray-200 bg-gray-50">
          <ng-content select="[card-header]" />
        </div>
      }

      <div [class]="bodyClasses">
        <ng-content />
      </div>

      @if (footer) {
        <div class="px-6 py-4 border-t border-gray-200 bg-gray-50">
          <ng-content select="[card-footer]" />
        </div>
      }
    </div>
  `,
})
export class CardComponent {
  @Input() header = false;
  @Input() footer = false;
  @Input() clickable = false;
  @Input() padding: 'none' | 'sm' | 'md' | 'lg' = 'md';

  get cardClasses(): string {
    const baseClasses = 'bg-white border border-gray-200 rounded-lg shadow-sm overflow-hidden';
    const hoverClass = this.clickable ? 'hover:shadow-md hover:border-blue-300 cursor-pointer transition-all duration-200' : '';
    return `${baseClasses} ${hoverClass}`;
  }

  get bodyClasses(): string {
    const paddingClasses = {
      none: '',
      sm: 'px-4 py-3',
      md: 'px-6 py-4',
      lg: 'px-8 py-6',
    };
    return paddingClasses[this.padding];
  }
}
```

#### 3. List Item Component

```typescript
import { Component, Input, Output, EventEmitter } from '@angular/core';

@Component({
  selector: 'fh-list-item',
  standalone: true,
  template: `
    <div [class]="listItemClasses">
      @if (checkbox) {
        <input
          type="checkbox"
          [checked]="checked"
          (change)="toggleCheck()"
          class="w-4 h-4 text-blue-600 border-gray-300 rounded focus:ring-2 focus:ring-blue-500"
        />
      }

      <div class="flex-1 min-w-0">
        <div [class]="titleClasses">
          <ng-content select="[list-item-title]" />
        </div>

        @if (subtitle) {
          <div class="text-sm text-gray-500 mt-1">
            <ng-content select="[list-item-subtitle]" />
          </div>
        }
      </div>

      @if (badge || action) {
        <div class="flex items-center space-x-2">
          @if (badge) {
            <ng-content select="[list-item-badge]" />
          }
          @if (action) {
            <ng-content select="[list-item-action]" />
          }
        </div>
      }
    </div>
  `,
})
export class ListItemComponent {
  @Input() checkbox = false;
  @Input() checked = false;
  @Input() subtitle = false;
  @Input() badge = false;
  @Input() action = false;
  @Input() clickable = false;

  @Output() checkedChange = new EventEmitter<boolean>();
  @Output() itemClick = new EventEmitter<void>();

  get listItemClasses(): string {
    const baseClasses = 'flex items-center gap-3 p-4 border-b border-gray-200 last:border-b-0';
    const hoverClass = this.clickable ? 'hover:bg-gray-50 cursor-pointer' : '';
    return `${baseClasses} ${hoverClass}`;
  }

  get titleClasses(): string {
    return this.checked ? 'text-gray-500 line-through' : 'text-gray-900';
  }

  toggleCheck(): void {
    this.checked = !this.checked;
    this.checkedChange.emit(this.checked);
  }
}
```

### Organism Components

#### 1. Modal Component

```typescript
import { Component, Input, Output, EventEmitter, OnInit, OnDestroy } from '@angular/core';

@Component({
  selector: 'fh-modal',
  standalone: true,
  template: `
    @if (isOpen) {
      <!-- Overlay -->
      <div
        class="fixed inset-0 bg-gray-900 bg-opacity-50 z-40 transition-opacity duration-300"
        [class.opacity-0]="!showContent"
        (click)="handleOverlayClick()"
      ></div>

      <!-- Modal -->
      <div class="fixed inset-0 z-50 overflow-y-auto">
        <div class="flex min-h-full items-center justify-center p-4">
          <div
            [class]="modalClasses"
            [class.scale-95]="!showContent"
            [class.opacity-0]="!showContent"
          >
            <!-- Close button -->
            @if (closeable) {
              <button
                (click)="close()"
                class="absolute top-4 right-4 text-gray-400 hover:text-gray-600 focus:outline-none focus:ring-2 focus:ring-blue-500 rounded-md"
              >
                <fh-icon name="x-mark" size="md" />
              </button>
            }

            <!-- Title -->
            @if (title) {
              <h2 class="text-2xl font-bold text-gray-900 mb-4 pr-8">
                {{ title }}
              </h2>
            }

            <!-- Content -->
            <div [class]="contentClasses">
              <ng-content />
            </div>

            <!-- Footer -->
            @if (footer) {
              <div class="flex justify-end space-x-3 mt-6 pt-4 border-t border-gray-200">
                <ng-content select="[modal-footer]" />
              </div>
            }
          </div>
        </div>
      </div>
    }
  `,
})
export class ModalComponent implements OnInit, OnDestroy {
  @Input() isOpen = false;
  @Input() title?: string;
  @Input() size: 'sm' | 'md' | 'lg' | 'xl' | 'full' = 'md';
  @Input() closeable = true;
  @Input() closeOnOverlay = true;
  @Input() footer = false;

  @Output() closed = new EventEmitter<void>();

  showContent = false;

  ngOnInit(): void {
    if (this.isOpen) {
      setTimeout(() => (this.showContent = true), 10);
      document.body.style.overflow = 'hidden';
    }
  }

  ngOnDestroy(): void {
    document.body.style.overflow = '';
  }

  get modalClasses(): string {
    const baseClasses = 'relative bg-white rounded-lg shadow-xl p-6 transform transition-all duration-300';

    const sizeClasses = {
      sm: 'max-w-sm w-full',
      md: 'max-w-lg w-full',
      lg: 'max-w-2xl w-full',
      xl: 'max-w-4xl w-full',
      full: 'max-w-full w-full mx-4',
    };

    return `${baseClasses} ${sizeClasses[this.size]}`;
  }

  get contentClasses(): string {
    return this.title ? 'mt-2' : '';
  }

  close(): void {
    this.showContent = false;
    setTimeout(() => {
      this.closed.emit();
      document.body.style.overflow = '';
    }, 300);
  }

  handleOverlayClick(): void {
    if (this.closeOnOverlay) {
      this.close();
    }
  }
}
```

#### 2. Toast Container Component

```typescript
import { Component, Injectable, signal } from '@angular/core';

export interface Toast {
  id: string;
  type: 'success' | 'error' | 'warning' | 'info';
  title: string;
  message?: string;
  duration?: number;
}

@Injectable({ providedIn: 'root' })
export class ToastService {
  private toasts = signal<Toast[]>([]);

  getToasts = this.toasts.asReadonly();

  show(toast: Omit<Toast, 'id'>): void {
    const id = Math.random().toString(36).substr(2, 9);
    const newToast: Toast = { ...toast, id };

    this.toasts.update(toasts => [...toasts, newToast]);

    if (toast.duration !== 0) {
      setTimeout(() => this.remove(id), toast.duration || 5000);
    }
  }

  remove(id: string): void {
    this.toasts.update(toasts => toasts.filter(t => t.id !== id));
  }

  success(title: string, message?: string): void {
    this.show({ type: 'success', title, message });
  }

  error(title: string, message?: string): void {
    this.show({ type: 'error', title, message });
  }

  warning(title: string, message?: string): void {
    this.show({ type: 'warning', title, message });
  }

  info(title: string, message?: string): void {
    this.show({ type: 'info', title, message });
  }
}

@Component({
  selector: 'fh-toast-container',
  standalone: true,
  template: `
    <div class="fixed bottom-6 right-6 z-50 space-y-3 max-w-sm w-full">
      @for (toast of toastService.getToasts()(); track toast.id) {
        <div
          [class]="getToastClasses(toast.type)"
          [@slideIn]
        >
          <div class="flex items-start">
            <div class="flex-shrink-0">
              <fh-icon [name]="getToastIcon(toast.type)" size="md" [class]="getIconColor(toast.type)" />
            </div>

            <div class="ml-3 w-0 flex-1">
              <p class="text-sm font-medium text-gray-900">
                {{ toast.title }}
              </p>
              @if (toast.message) {
                <p class="mt-1 text-sm text-gray-500">
                  {{ toast.message }}
                </p>
              }
            </div>

            <div class="ml-4 flex flex-shrink-0">
              <button
                (click)="toastService.remove(toast.id)"
                class="inline-flex text-gray-400 hover:text-gray-600 focus:outline-none focus:ring-2 focus:ring-blue-500 rounded-md"
              >
                <fh-icon name="x-mark" size="sm" />
              </button>
            </div>
          </div>
        </div>
      }
    </div>
  `,
  animations: [
    trigger('slideIn', [
      transition(':enter', [
        style({ transform: 'translateX(100%)', opacity: 0 }),
        animate('300ms ease-out', style({ transform: 'translateX(0)', opacity: 1 })),
      ]),
      transition(':leave', [
        animate('200ms ease-in', style({ transform: 'translateX(100%)', opacity: 0 })),
      ]),
    ]),
  ],
})
export class ToastContainerComponent {
  constructor(public toastService: ToastService) {}

  getToastClasses(type: Toast['type']): string {
    return 'bg-white shadow-lg rounded-lg pointer-events-auto ring-1 ring-black ring-opacity-5 overflow-hidden p-4';
  }

  getToastIcon(type: Toast['type']): string {
    const icons = {
      success: 'check-circle',
      error: 'x-circle',
      warning: 'exclamation-triangle',
      info: 'information-circle',
    };
    return icons[type];
  }

  getIconColor(type: Toast['type']): string {
    const colors = {
      success: 'text-green-500',
      error: 'text-red-500',
      warning: 'text-amber-500',
      info: 'text-blue-500',
    };
    return colors[type];
  }
}
```

---

## Component APIs

### Standard Props Interface

All components should follow this structure:

```typescript
interface ComponentProps {
  // Visual
  variant?: string;
  size?: string;
  color?: string;

  // State
  disabled?: boolean;
  loading?: boolean;
  error?: string;

  // Content
  label?: string;
  placeholder?: string;

  // Behavior
  onClick?: () => void;
  onChange?: (value: any) => void;

  // Accessibility
  ariaLabel?: string;
  ariaDescribedby?: string;
  role?: string;
}
```

### Event Naming Convention

```typescript
// Use descriptive event names with past tense
@Output() clicked = new EventEmitter<MouseEvent>();
@Output() changed = new EventEmitter<string>();
@Output() submitted = new EventEmitter<FormData>();
@Output() deleted = new EventEmitter<string>();
@Output() selected = new EventEmitter<any>();
```

---

## Theming Implementation

### Theme Service

```typescript
import { Injectable, signal } from '@angular/core';

export type Theme = 'light' | 'dark' | 'auto';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private currentTheme = signal<Theme>('auto');
  private systemTheme = signal<'light' | 'dark'>('light');

  theme = this.currentTheme.asReadonly();

  constructor() {
    this.loadTheme();
    this.setupSystemThemeListener();
  }

  setTheme(theme: Theme): void {
    this.currentTheme.set(theme);
    localStorage.setItem('theme', theme);
    this.applyTheme();
  }

  private loadTheme(): void {
    const savedTheme = localStorage.getItem('theme') as Theme;
    if (savedTheme) {
      this.currentTheme.set(savedTheme);
    }
    this.applyTheme();
  }

  private setupSystemThemeListener(): void {
    const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');
    this.systemTheme.set(mediaQuery.matches ? 'dark' : 'light');

    mediaQuery.addEventListener('change', (e) => {
      this.systemTheme.set(e.matches ? 'dark' : 'light');
      if (this.currentTheme() === 'auto') {
        this.applyTheme();
      }
    });
  }

  private applyTheme(): void {
    const theme = this.currentTheme();
    const effectiveTheme = theme === 'auto' ? this.systemTheme() : theme;

    if (effectiveTheme === 'dark') {
      document.documentElement.classList.add('dark');
    } else {
      document.documentElement.classList.remove('dark');
    }
  }
}
```

---

## Code Examples

### Complete Form Example

```typescript
import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

@Component({
  selector: 'fh-create-task-form',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    FormFieldComponent,
    ButtonComponent,
    ModalComponent,
  ],
  template: `
    <fh-modal
      [isOpen]="isOpen"
      title="Create New Task"
      size="md"
      [footer]="true"
      (closed)="handleClose()"
    >
      <form [formGroup]="taskForm" (ngSubmit)="handleSubmit()">
        <fh-form-field
          id="title"
          label="Task Title"
          formControlName="title"
          placeholder="Enter task title"
          [required]="true"
          [error]="getFieldError('title')"
        />

        <fh-form-field
          id="description"
          label="Description"
          formControlName="description"
          placeholder="Add details..."
        />

        <fh-form-field
          id="assignee"
          label="Assign To"
          type="select"
          formControlName="assignee"
        >
          <option value="">Select a family member</option>
          <option value="user1">John</option>
          <option value="user2">Jane</option>
        </fh-form-field>

        <fh-form-field
          id="dueDate"
          label="Due Date"
          type="date"
          formControlName="dueDate"
        />

        <div modal-footer>
          <fh-button variant="secondary" (clicked)="handleClose()">
            Cancel
          </fh-button>
          <fh-button
            variant="primary"
            type="submit"
            [disabled]="taskForm.invalid"
            [loading]="isSubmitting"
          >
            Create Task
          </fh-button>
        </div>
      </form>
    </fh-modal>
  `,
})
export class CreateTaskFormComponent {
  isOpen = false;
  isSubmitting = false;

  taskForm: FormGroup;

  constructor(private fb: FormBuilder) {
    this.taskForm = this.fb.group({
      title: ['', Validators.required],
      description: [''],
      assignee: [''],
      dueDate: [''],
    });
  }

  getFieldError(field: string): string | undefined {
    const control = this.taskForm.get(field);
    if (control?.touched && control?.errors) {
      if (control.errors['required']) {
        return 'This field is required';
      }
    }
    return undefined;
  }

  handleSubmit(): void {
    if (this.taskForm.valid) {
      this.isSubmitting = true;
      // Submit logic
    }
  }

  handleClose(): void {
    this.isOpen = false;
    this.taskForm.reset();
  }
}
```

---

**Document Status:** Component Specifications Complete
**Next Steps:** Implement components, create Storybook stories, write tests
**Related Documents:** design-system.md, wireframes.md, responsive-design-guide.md
