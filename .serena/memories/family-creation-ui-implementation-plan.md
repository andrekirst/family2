# Family Hub - Family Creation UI Implementation Plan
**Multi-Agent Architectural Review**

**Feature:** Family Creation UI for authenticated users without a family  
**Approach:** Test-Driven Development (TDD) + Minimal Design System + Reactive Forms  
**Status:** Planning Complete - Ready for Implementation  
**Created:** 2025-12-30  
**Reviewed By:** Angular Architect, TypeScript Pro, UI Designer, UX Researcher (simulated perspectives)

---

## Executive Summary

This plan implements the Family Creation UI feature for authenticated users who don't have a family yet. The implementation follows a **Minimal Design System** approach (YAGNI principle), building only components needed for this feature, with comprehensive **Test-Driven Development (TDD)** and **Angular 18 best practices** throughout.

### Key Architectural Decisions

| Decision | Rationale | Agent Perspective |
|----------|-----------|-------------------|
| **Reactive Forms** (not template-driven) | Better type safety, testability, scalability | @agent-typescript-pro, @agent-angular-architect |
| **Minimal Design System** (3 components, not 9) | YAGNI principle, 1-2 days time savings | @agent-ui-designer |
| **Blocking Modal with "Skip"** | Balance user control with data integrity | @agent-ux-researcher |
| **Extend GraphQLService** (not replace) | Non-breaking changes, incremental improvement | @agent-typescript-pro |
| **Validation after blur** (not while typing) | Better UX, less cognitive load | @agent-ux-researcher |
| **focus-trap library** (not custom) | Battle-tested accessibility, fewer bugs | @agent-ux-researcher |

### User Flow

```
User logs in → Check family status (GraphQL query)  
  ├─ NO FAMILY: Show CreateFamilyModal (blocking with "Skip" option)
  │   ├─ User enters family name
  │   ├─ Form validation (required, max 50 chars, validated after blur)
  │   ├─ Submit → GraphQL mutation
  │   ├─ SUCCESS: Close modal → Navigate to dashboard
  │   ├─ ERROR: Show error message, keep modal open
  │   └─ SKIP: Logout with explanation
  └─ HAS FAMILY: Navigate directly to dashboard
```

### Timeline Comparison

| Plan | Duration | Components Built | Approach |
|------|----------|------------------|----------|
| **Original** | 6-7 days | 9 components (all Design System) | Comprehensive upfront |
| **Revised** | 6 days | 3 components (minimal) | YAGNI + Reactive Forms |
| **Savings** | -1 day | -6 components | 33% less code |

---

## Phase 0: Infrastructure Improvements (0.5 days)

**Objective:** Fix GraphQLService error handling WITHOUT breaking existing code.

**Problem:** Current GraphQLService (lines 14-29) doesn't handle GraphQL errors properly:
```typescript
// CURRENT (problematic)
async query<T>(query: string, variables?: any): Promise<T> {
  const response = await firstValueFrom(
    this.http.post<{ data: T }>(this.endpoint, { query, variables })
  );
  return response.data; // ❌ Doesn't check for errors!
}
```

GraphQL can return `{ data: null, errors: [...] }` which this code ignores.

### 0.1 Create GraphQL Types

**File:** `/home/andrekirst/git/github/andrekirst/family2/src/frontend/family-hub-web/src/app/core/models/graphql.models.ts`

```typescript
/**
 * Standard GraphQL response structure.
 * GraphQL always returns 200 OK, errors are in the response body.
 */
export interface GraphQLResponse<T> {
  data?: T;
  errors?: GraphQLError[];
}

export interface GraphQLError {
  message: string;
  locations?: Array<{
    line: number;
    column: number;
  }>;
  path?: string[];
  extensions?: Record<string, any>;
}

export class GraphQLQueryError extends Error {
  constructor(public errors: GraphQLError[]) {
    super(errors[0]?.message || 'GraphQL query failed');
    this.name = 'GraphQLQueryError';
  }
}
```

### 0.2 Extend GraphQLService

**File:** `/home/andrekirst/git/github/andrekirst/family2/src/frontend/family-hub-web/src/app/core/services/graphql.service.ts`

**Strategy:** ADD new methods, KEEP existing ones (non-breaking).

```typescript
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { firstValueFrom } from 'rxjs';
import { GraphQLResponse, GraphQLQueryError } from '../models/graphql.models';

@Injectable({
  providedIn: 'root'
})
export class GraphQLService {
  private readonly endpoint = environment.graphqlEndpoint;

  constructor(private http: HttpClient) {}

  // ✅ KEEP existing methods for backward compatibility
  async query<T>(query: string, variables?: any): Promise<T> {
    const response = await firstValueFrom(
      this.http.post<{ data: T }>(this.endpoint, { query, variables })
    );
    return response.data;
  }

  async mutate<T>(mutation: string, variables?: any): Promise<T> {
    const response = await firstValueFrom(
      this.http.post<{ data: T }>(this.endpoint, {
        query: mutation,
        variables
      })
    );
    return response.data;
  }

  // ✅ NEW: Methods with proper error handling
  async queryWithErrors<T>(query: string, variables?: any): Promise<T> {
    const response = await firstValueFrom(
      this.http.post<GraphQLResponse<T>>(this.endpoint, { query, variables })
    );
    
    if (response.errors && response.errors.length > 0) {
      throw new GraphQLQueryError(response.errors);
    }
    
    if (!response.data) {
      throw new Error('GraphQL query returned no data');
    }
    
    return response.data;
  }

  async mutateWithErrors<T>(mutation: string, variables?: any): Promise<T> {
    const response = await firstValueFrom(
      this.http.post<GraphQLResponse<T>>(this.endpoint, {
        query: mutation,
        variables
      })
    );
    
    // Note: mutations may return errors in data payload (e.g., CreateFamilyPayload.errors)
    // This method only throws for HTTP/network errors
    // Application errors are handled by the service layer
    
    if (!response.data) {
      if (response.errors && response.errors.length > 0) {
        throw new GraphQLQueryError(response.errors);
      }
      throw new Error('GraphQL mutation returned no data');
    }
    
    return response.data;
  }
}
```

**Test File:** `/home/andrekirst/git/github/andrekirst/family2/src/frontend/family-hub-web/src/app/core/services/graphql.service.spec.ts`

```typescript
import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { GraphQLService } from './graphql.service';
import { GraphQLQueryError } from '../models/graphql.models';
import { environment } from '../../../environments/environment';

describe('GraphQLService', () => {
  let service: GraphQLService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [GraphQLService]
    });
    service = TestBed.inject(GraphQLService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  describe('queryWithErrors', () => {
    it('should return data on success', async () => {
      const mockData = { user: { id: '123', email: 'test@example.com' } };
      const query = 'query { user { id email } }';

      const promise = service.queryWithErrors(query);

      const req = httpMock.expectOne(environment.graphqlEndpoint);
      req.flush({ data: mockData });

      const result = await promise;
      expect(result).toEqual(mockData);
    });

    it('should throw GraphQLQueryError when errors present', async () => {
      const query = 'query { invalid }';
      const mockErrors = [{ message: 'Field not found', path: ['invalid'] }];

      const promise = service.queryWithErrors(query);

      const req = httpMock.expectOne(environment.graphqlEndpoint);
      req.flush({ data: null, errors: mockErrors });

      await expectAsync(promise).toBeRejectedWithError(GraphQLQueryError);
    });

    it('should throw when data is null and no errors', async () => {
      const query = 'query { user }';

      const promise = service.queryWithErrors(query);

      const req = httpMock.expectOne(environment.graphqlEndpoint);
      req.flush({ data: null });

      await expectAsync(promise).toBeRejectedWithError('GraphQL query returned no data');
    });
  });

  describe('backward compatibility', () => {
    it('should keep existing query method working', async () => {
      const mockData = { user: { id: '123' } };
      const query = 'query { user { id } }';

      const promise = service.query(query);

      const req = httpMock.expectOne(environment.graphqlEndpoint);
      req.flush({ data: mockData });

      const result = await promise;
      expect(result).toEqual(mockData);
    });
  });
});
```

### Phase 0 Success Criteria

- ✅ New GraphQL types created
- ✅ GraphQLService extended with error-aware methods
- ✅ Existing methods unchanged (backward compatible)
- ✅ All tests passing (>80% coverage)
- ✅ No breaking changes to AuthService or Dashboard

---

## Phase 1: Core Components Only (2 days)

**Objective:** Build ONLY the 3 components needed for family creation feature.

**Philosophy:** YAGNI (You Aren't Gonna Need It) - Don't build Badge, Card, Toast, FormField components yet.

### 1.1 Input Component with ControlValueAccessor

**File:** `/home/andrekirst/git/github/andrekirst/family2/src/frontend/family-hub-web/src/app/shared/components/atoms/input/input.component.ts`

**Requirements:**
- Text input with Tailwind styling
- Support Reactive Forms via `formControlName` (ControlValueAccessor)
- States: default, focus, error, disabled
- Sizes: sm, md, lg
- Accessibility: aria-label, aria-describedby, aria-invalid

**Implementation:**

```typescript
import { Component, Input, forwardRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ControlValueAccessor, NG_VALUE_ACCESSOR, ReactiveFormsModule } from '@angular/forms';

export type InputSize = 'sm' | 'md' | 'lg';
export type InputType = 'text' | 'email' | 'password';

@Component({
  selector: 'fh-input',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => InputComponent),
      multi: true
    }
  ],
  template: `
    <input
      [type]="type"
      [placeholder]="placeholder"
      [disabled]="disabled"
      [attr.aria-label]="ariaLabel"
      [attr.aria-describedby]="ariaDescribedBy"
      [attr.aria-invalid]="hasError"
      [attr.maxlength]="maxLength"
      [class]="inputClasses"
      [value]="value"
      (input)="onInput($event)"
      (blur)="onTouched()"
    />
  `
})
export class InputComponent implements ControlValueAccessor {
  @Input() type: InputType = 'text';
  @Input() size: InputSize = 'md';
  @Input() placeholder = '';
  @Input() hasError = false;
  @Input() ariaLabel?: string;
  @Input() ariaDescribedBy?: string;
  @Input() maxLength?: number;

  value = '';
  disabled = false;

  // ControlValueAccessor callbacks
  private onChange: (value: string) => void = () => {};
  onTouched: () => void = () => {};

  get inputClasses(): string {
    const baseClasses = 'block w-full rounded-md border px-3 py-2 transition-colors focus:outline-none focus:ring-2 focus:ring-offset-2';
    
    const sizeClasses: Record<InputSize, string> = {
      sm: 'px-2 py-1 text-sm',
      md: 'px-3 py-2 text-base',
      lg: 'px-4 py-3 text-lg'
    };
    
    const stateClasses = this.hasError
      ? 'border-red-500 focus:border-red-500 focus:ring-red-500'
      : 'border-gray-300 focus:border-blue-500 focus:ring-blue-500';
    
    const disabledClasses = this.disabled
      ? 'bg-gray-100 text-gray-500 cursor-not-allowed'
      : 'bg-white';
    
    return `${baseClasses} ${sizeClasses[this.size]} ${stateClasses} ${disabledClasses}`;
  }

  onInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.value = input.value;
    this.onChange(this.value);
  }

  // ControlValueAccessor implementation
  writeValue(value: string): void {
    this.value = value || '';
  }

  registerOnChange(fn: (value: string) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled = isDisabled;
  }
}
```

**Test File:** 25+ test cases covering ControlValueAccessor interface, states, sizes, accessibility.

### 1.2 Icon Component

**File:** `/home/andrekirst/git/github/andrekirst/family2/src/frontend/family-hub-web/src/app/shared/components/atoms/icon/icon.component.ts`

**Icons Needed:**
- `x-mark` (close button)
- `exclamation-circle` (error icon)

**Implementation:**

```typescript
import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

export type IconName = 'x-mark' | 'exclamation-circle';
export type IconSize = 'xs' | 'sm' | 'md' | 'lg' | 'xl';

@Component({
  selector: 'fh-icon',
  standalone: true,
  imports: [CommonModule],
  template: `
    <svg
      [attr.class]="classes"
      [attr.aria-hidden]="!ariaLabel"
      [attr.aria-label]="ariaLabel"
      fill="currentColor"
      viewBox="0 0 24 24"
    >
      <path [attr.d]="iconPath" />
    </svg>
  `
})
export class IconComponent {
  @Input() name: IconName = 'x-mark';
  @Input() size: IconSize = 'md';
  @Input() ariaLabel?: string;

  private iconPaths: Record<IconName, string> = {
    'x-mark': 'M6.28 5.22a.75.75 0 00-1.06 1.06L10.94 12l-5.72 5.72a.75.75 0 101.06 1.06L12 13.06l5.72 5.72a.75.75 0 101.06-1.06L13.06 12l5.72-5.72a.75.75 0 00-1.06-1.06L12 10.94 6.28 5.22z',
    'exclamation-circle': 'M12 2.25c-5.385 0-9.75 4.365-9.75 9.75s4.365 9.75 9.75 9.75 9.75-4.365 9.75-9.75S17.385 2.25 12 2.25zM12.75 6a.75.75 0 00-1.5 0v6a.75.75 0 001.5 0V6zM12 17.25a1.125 1.125 0 100-2.25 1.125 1.125 0 000 2.25z'
  };

  get iconPath(): string {
    return this.iconPaths[this.name] || '';
  }

  get classes(): string {
    const sizeClasses: Record<IconSize, string> = {
      xs: 'h-3 w-3',
      sm: 'h-4 w-4',
      md: 'h-5 w-5',
      lg: 'h-6 w-6',
      xl: 'h-8 w-8'
    };
    return sizeClasses[this.size];
  }
}
```

### 1.3 Modal Component with Focus Trap

**Dependencies:**
```bash
npm install focus-trap
npm install -D @types/focus-trap
```

**File:** `/home/andrekirst/git/github/andrekirst/family2/src/frontend/family-hub-web/src/app/shared/components/molecules/modal/modal.component.ts`

**Requirements:**
- Overlay backdrop
- Centered responsive card
- Optional close button (controlled by parent)
- Focus trap using `focus-trap` library
- Accessibility: aria-modal, role="dialog", focus management
- Block body scroll when open

**Implementation:**

```typescript
import { Component, Input, Output, EventEmitter, OnInit, OnDestroy, ViewChild, ElementRef, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IconComponent } from '../../atoms/icon/icon.component';
import { createFocusTrap, FocusTrap } from 'focus-trap';

@Component({
  selector: 'fh-modal',
  standalone: true,
  imports: [CommonModule, IconComponent],
  template: `
    @if (isOpen) {
      <div
        class="fixed inset-0 z-50 overflow-y-auto"
        role="dialog"
        aria-modal="true"
        [attr.aria-labelledby]="titleId"
      >
        <!-- Backdrop -->
        <div
          class="fixed inset-0 bg-gray-900 bg-opacity-50 transition-opacity"
          [class.cursor-pointer]="closeOnBackdropClick"
          (click)="handleBackdropClick()"
        ></div>

        <!-- Modal content -->
        <div class="flex min-h-full items-center justify-center p-4">
          <div
            #modalContent
            class="relative bg-white rounded-lg shadow-xl w-full max-w-md max-h-[90vh] overflow-y-auto"
            (click)="$event.stopPropagation()"
          >
            <div class="p-6">
              <!-- Close button (optional) -->
              @if (showCloseButton) {
                <button
                  type="button"
                  class="absolute top-4 right-4 text-gray-400 hover:text-gray-600 focus:outline-none focus:ring-2 focus:ring-blue-500 rounded"
                  (click)="handleClose()"
                  aria-label="Close modal"
                >
                  <fh-icon name="x-mark" size="md" />
                </button>
              }

              <!-- Header slot -->
              <div [id]="titleId" class="mb-4">
                <ng-content select="[slot='header']" />
              </div>

              <!-- Body slot -->
              <div class="mb-6">
                <ng-content select="[slot='body']" />
              </div>

              <!-- Footer slot -->
              <div class="flex justify-end space-x-3">
                <ng-content select="[slot='footer']" />
              </div>
            </div>
          </div>
        </div>
      </div>
    }
  `
})
export class ModalComponent implements OnInit, OnDestroy, AfterViewInit {
  @Input() isOpen = false;
  @Input() closeOnBackdropClick = false; // Blocking modal by default
  @Input() showCloseButton = true;
  @Output() close = new EventEmitter<void>();

  @ViewChild('modalContent') modalContent?: ElementRef;

  titleId = `modal-title-${Math.random().toString(36).substr(2, 9)}`;
  private focusTrap?: FocusTrap;

  ngOnInit() {
    if (this.isOpen) {
      this.lockBodyScroll();
    }
  }

  ngAfterViewInit() {
    if (this.isOpen && this.modalContent) {
      this.setupFocusTrap();
    }
  }

  ngOnDestroy() {
    this.cleanup();
  }

  private setupFocusTrap() {
    if (!this.modalContent) return;

    this.focusTrap = createFocusTrap(this.modalContent.nativeElement, {
      initialFocus: () => {
        // Focus first input or first focusable element
        const firstInput = this.modalContent!.nativeElement.querySelector('input, textarea, select');
        return firstInput || this.modalContent!.nativeElement;
      },
      onDeactivate: () => {
        // Called when focus trap is deactivated
      },
      clickOutsideDeactivates: false,
      escapeDeactivates: false // Prevent Escape key from closing (blocking modal)
    });

    this.focusTrap.activate();
  }

  private cleanup() {
    if (this.focusTrap) {
      this.focusTrap.deactivate();
      this.focusTrap = undefined;
    }
    this.unlockBodyScroll();
  }

  handleClose() {
    this.cleanup();
    this.close.emit();
  }

  handleBackdropClick() {
    if (this.closeOnBackdropClick) {
      this.handleClose();
    }
  }

  private lockBodyScroll() {
    document.body.style.overflow = 'hidden';
  }

  private unlockBodyScroll() {
    document.body.style.overflow = '';
  }
}
```

**Test File:** Tests for focus trap, keyboard navigation, backdrop click, accessibility.

### Phase 1 Success Criteria

- ✅ Input component with ControlValueAccessor (works with Reactive Forms)
- ✅ Icon component with x-mark and exclamation-circle icons
- ✅ Modal component with focus-trap library integration
- ✅ All tests passing (>80% coverage)
- ✅ Accessibility audit passing (keyboard nav, screen readers)
- ✅ Responsive design tested (mobile, tablet, desktop)

---

## Phase 2: Family Service (1 day)

**Objective:** Create FamilyService with GraphQL integration and Angular Signals state management.

### 2.1 Family Domain Models

**File:** `/home/andrekirst/git/github/andrekirst/family2/src/frontend/family-hub-web/src/app/core/models/family.models.ts`

```typescript
/**
 * Domain model for Family (used in application layer).
 * Dates are converted from ISO strings to Date objects.
 */
export interface Family {
  id: string;
  name: string;
  ownerId: string;
  createdAt: Date;
}

/**
 * GraphQL response type for Family (as received from server).
 * Dates are ISO strings.
 */
export interface FamilyGraphQLType {
  id: string;
  name: string;
  ownerId: string;
  createdAt: string; // ISO 8601 string
}

/**
 * Input for creating a family.
 */
export interface CreateFamilyInput {
  name: string;
}

/**
 * GraphQL mutation result for createFamily.
 */
export interface CreateFamilyPayload {
  family: FamilyGraphQLType | null;
  errors: Array<{
    message: string;
    code: string;
  }> | null;
}

/**
 * GraphQL query result for getUserFamilies.
 */
export interface GetUserFamiliesResult {
  families: FamilyGraphQLType[];
}

/**
 * Mapper: GraphQL → Domain
 */
export function mapFamilyFromGraphQL(gql: FamilyGraphQLType): Family {
  return {
    id: gql.id,
    name: gql.name,
    ownerId: gql.ownerId,
    createdAt: new Date(gql.createdAt)
  };
}
```

### 2.2 Family Service Implementation

**File:** `/home/andrekirst/git/github/andrekirst/family2/src/frontend/family-hub-web/src/app/core/services/family.service.ts`

**Requirements:**
- GraphQL query: `getUserFamilies` (using `queryWithErrors`)
- GraphQL mutation: `createFamily` (using `mutateWithErrors`)
- Angular Signals for state management
- DON'T THROW errors to UI (use signal state)
- Handle: network errors, GraphQL errors, business errors

**Implementation:**

```typescript
import { Injectable, signal, computed } from '@angular/core';
import { GraphQLService } from './graphql.service';
import {
  Family,
  FamilyGraphQLType,
  CreateFamilyInput,
  CreateFamilyPayload,
  GetUserFamiliesResult,
  mapFamilyFromGraphQL
} from '../models/family.models';
import { GraphQLQueryError } from '../models/graphql.models';

@Injectable({
  providedIn: 'root'
})
export class FamilyService {
  // Reactive state with signals
  private familiesState = signal<Family[]>([]);
  private loadingState = signal(false);
  private errorState = signal<string | null>(null);

  // Computed signals (derived state)
  readonly families = computed(() => this.familiesState());
  readonly currentFamily = computed(() => this.familiesState()[0] || null);
  readonly hasFamily = computed(() => this.familiesState().length > 0);
  readonly isLoading = computed(() => this.loadingState());
  readonly error = computed(() => this.errorState());

  constructor(private graphql: GraphQLService) {}

  async getUserFamilies(): Promise<void> {
    this.loadingState.set(true);
    this.errorState.set(null);

    try {
      const query = `
        query GetUserFamilies {
          getUserFamilies {
            families {
              id
              name
              ownerId
              createdAt
            }
          }
        }
      `;

      const response = await this.graphql.queryWithErrors<{ getUserFamilies: GetUserFamiliesResult }>(query);

      const families = response.getUserFamilies.families.map(mapFamilyFromGraphQL);

      this.familiesState.set(families);
    } catch (error) {
      console.error('Failed to fetch families:', error);

      const message = error instanceof GraphQLQueryError
        ? error.message
        : 'Failed to load families. Please check your connection.';

      this.errorState.set(message);
    } finally {
      this.loadingState.set(false);
    }
  }

  async createFamily(input: CreateFamilyInput): Promise<CreateFamilyPayload> {
    this.loadingState.set(true);
    this.errorState.set(null);

    try {
      const mutation = `
        mutation CreateFamily($input: CreateFamilyInput!) {
          createFamily(input: $input) {
            family {
              id
              name
              ownerId
              createdAt
            }
            errors {
              message
              code
            }
          }
        }
      `;

      const response = await this.graphql.mutateWithErrors<{ createFamily: CreateFamilyPayload }>(
        mutation,
        { input }
      );

      const result = response.createFamily;

      // Check for business/validation errors (in payload)
      if (result.errors && result.errors.length > 0) {
        const errorMessage = result.errors[0].message;
        this.errorState.set(errorMessage);
        return result; // Return errors to component
      }

      // Success - update state
      if (result.family) {
        const family = mapFamilyFromGraphQL(result.family);
        this.familiesState.set([family]);
      }

      return result;
    } catch (error) {
      console.error('Failed to create family:', error);

      const message = error instanceof GraphQLQueryError
        ? error.message
        : 'Failed to create family. Please try again.';

      this.errorState.set(message);

      // Return error response (don't throw)
      return {
        family: null,
        errors: [{ message, code: 'NETWORK_ERROR' }]
      };
    } finally {
      this.loadingState.set(false);
    }
  }

  clearError(): void {
    this.errorState.set(null);
  }
}
```

**Test File:** 15+ tests covering all scenarios (success, validation errors, business errors, network errors).

### Phase 2 Success Criteria

- ✅ Family domain models with mappers
- ✅ FamilyService with Signals state management
- ✅ GraphQL integration using extended methods
- ✅ Error handling (no throws to UI)
- ✅ All tests passing (>80% coverage)

---

## Phase 3: Create Family Modal (1.5 days)

**Objective:** Build CreateFamilyModal with Reactive Forms, validation, and accessibility.

### 3.1 Create Family Modal Component

**File:** `/home/andrekirst/git/github/andrekirst/family2/src/frontend/family-hub-web/src/app/features/family/components/create-family-modal/create-family-modal.component.ts`

**Requirements:**
- Reactive Forms (not template-driven)
- Validation: required, maxLength 50
- Validation trigger: after blur (not while typing)
- Character counter with color-coding
- Blocking modal with "Skip for now" option
- Screen reader announcements
- Focus management

**Implementation:**

```typescript
import { Component, Input, Output, EventEmitter, computed, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ModalComponent } from '../../../../shared/components/molecules/modal/modal.component';
import { InputComponent } from '../../../../shared/components/atoms/input/input.component';
import { IconComponent } from '../../../../shared/components/atoms/icon/icon.component';
import { ButtonComponent } from '../../../../shared/components/atoms/button/button.component';
import { FamilyService } from '../../../../core/services/family.service';

@Component({
  selector: 'fh-create-family-modal',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    ModalComponent,
    InputComponent,
    IconComponent,
    ButtonComponent
  ],
  template: `
    <fh-modal
      [isOpen]="isOpen"
      [closeOnBackdropClick]="false"
      [showCloseButton]="false"
      (close)="handleCancel()"
    >
      <div slot="header">
        <h2 class="text-2xl font-bold text-gray-900">Create Your Family</h2>
        <p class="mt-2 text-sm text-gray-600">
          Get started by creating your family. You can invite members later.
        </p>
      </div>

      <div slot="body">
        <form [formGroup]="form" (ngSubmit)="handleSubmit()">
          <!-- Family Name Field -->
          <div class="space-y-1">
            <label for="familyName" class="block text-sm font-medium text-gray-700">
              Family Name <span class="text-red-500 ml-1">*</span>
            </label>

            <fh-input
              id="familyName"
              type="text"
              placeholder="e.g., Smith Family"
              formControlName="name"
              [hasError]="showNameError()"
              [ariaDescribedBy]="showNameError() ? 'name-error' : undefined"
              [maxLength]="50"
            />

            <!-- Character Counter -->
            <p [class]="characterCounterClasses()">
              {{ nameControl.value?.length || 0 }} / 50 characters
            </p>

            <!-- Validation Error -->
            @if (showNameError()) {
              <p id="name-error" class="text-sm text-red-600 flex items-center" role="alert">
                <fh-icon name="exclamation-circle" size="xs" class="inline mr-1" />
                {{ nameErrorMessage() }}
              </p>
            }
          </div>

          <!-- API Error Display -->
          @if (apiError()) {
            <div class="mt-4 p-3 bg-red-50 border border-red-200 rounded-md" role="alert" aria-live="assertive">
              <p class="text-sm text-red-800 flex items-start">
                <fh-icon name="exclamation-circle" size="sm" class="inline mr-2 mt-0.5 flex-shrink-0" />
                <span>{{ apiError() }}</span>
              </p>
            </div>
          }

          <!-- Screen Reader Announcements -->
          <div class="sr-only" role="status" aria-live="polite" aria-atomic="true">
            @if (isSubmitting()) {
              Creating your family...
            }
          </div>
        </form>
      </div>

      <div slot="footer">
        <!-- Skip Button -->
        <button
          type="button"
          class="text-sm text-gray-600 hover:text-gray-800 underline focus:outline-none focus:ring-2 focus:ring-blue-500 rounded px-2 py-1"
          (click)="handleSkip()"
          [disabled]="isSubmitting()"
        >
          Skip for now
        </button>

        <!-- Submit Button -->
        <fh-button
          variant="primary"
          type="submit"
          [loading]="isSubmitting()"
          [disabled]="!form.valid || isSubmitting()"
          (clicked)="handleSubmit()"
        >
          Create Family
        </fh-button>
      </div>
    </fh-modal>
  `,
  styles: [`
    .sr-only {
      position: absolute;
      width: 1px;
      height: 1px;
      padding: 0;
      margin: -1px;
      overflow: hidden;
      clip: rect(0, 0, 0, 0);
      white-space: nowrap;
      border-width: 0;
    }
  `]
})
export class CreateFamilyModalComponent implements OnInit {
  @Input() isOpen = false;
  @Output() success = new EventEmitter<void>();
  @Output() cancel = new EventEmitter<void>();
  @Output() skip = new EventEmitter<void>();

  // Reactive Form
  form = new FormGroup({
    name: new FormControl('', [
      Validators.required,
      Validators.maxLength(50)
    ])
  });

  // Convenience accessor
  get nameControl() {
    return this.form.controls.name;
  }

  // Component state
  isSubmitting = signal(false);
  apiError = signal<string | null>(null);
  private nameTouched = signal(false);

  // Validation error display logic
  showNameError = computed(() => {
    return this.nameTouched() && this.nameControl.invalid && this.nameControl.dirty;
  });

  nameErrorMessage = computed(() => {
    if (this.nameControl.hasError('required')) {
      return 'Family name is required.';
    }
    if (this.nameControl.hasError('maxlength')) {
      return 'Family name must be 50 characters or less.';
    }
    return null;
  });

  // Character counter color-coding
  characterCounterClasses = computed(() => {
    const length = this.nameControl.value?.length || 0;
    const baseClasses = 'text-xs';

    if (length > 50) {
      return `${baseClasses} text-red-600 font-medium`;
    } else if (length >= 41) {
      return `${baseClasses} text-amber-600`;
    } else {
      return `${baseClasses} text-gray-500`;
    }
  });

  constructor(private familyService: FamilyService) {}

  ngOnInit() {
    // Track blur event for validation display
    this.nameControl.valueChanges.subscribe(() => {
      if (this.nameControl.dirty) {
        this.nameTouched.set(true);
      }
    });

    // Clear API error when user starts typing
    this.nameControl.valueChanges.subscribe(() => {
      if (this.apiError()) {
        this.apiError.set(null);
      }
    });
  }

  async handleSubmit(): Promise<void> {
    // Mark as touched to show validation errors
    this.nameTouched.set(true);
    this.nameControl.markAsTouched();
    this.nameControl.updateValueAndValidity();

    if (!this.form.valid || this.isSubmitting()) {
      return;
    }

    this.isSubmitting.set(true);
    this.apiError.set(null);

    const result = await this.familyService.createFamily({
      name: this.nameControl.value!.trim()
    });

    this.isSubmitting.set(false);

    if (result.errors && result.errors.length > 0) {
      // Business/validation error from server
      this.apiError.set(result.errors[0].message);
    } else if (result.family) {
      // Success
      this.form.reset();
      this.nameTouched.set(false);
      this.success.emit();
    }
  }

  handleCancel(): void {
    if (!this.isSubmitting()) {
      this.cancel.emit();
    }
  }

  handleSkip(): void {
    if (!this.isSubmitting()) {
      this.skip.emit();
    }
  }
}
```

**Test File:** 20+ tests covering form validation, submission, error handling, accessibility.

### Phase 3 Success Criteria

- ✅ Reactive form with validation
- ✅ Validation errors shown after blur
- ✅ Character counter with color-coding
- ✅ API error display
- ✅ Screen reader announcements
- ✅ Blocking modal with "Skip" option
- ✅ All tests passing (>80% coverage)

---

## Phase 4: Dashboard Integration (0.5 days)

**Objective:** Update Dashboard to check family status and show modal conditionally.

### 4.1 Update Dashboard Component

**File:** `/home/andrekirst/git/github/andrekirst/family2/src/frontend/family-hub-web/src/app/features/dashboard/dashboard.component.ts`

**Changes:**
- Import FamilyService and CreateFamilyModalComponent
- Call `getUserFamilies()` on init
- Conditional rendering based on `hasFamily()` signal
- Handle "Skip" action (logout with explanation)

**Implementation:**

```typescript
import { Component, computed, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { FamilyService } from '../../core/services/family.service';
import { ButtonComponent } from '../../shared/components/atoms/button/button.component';
import { CreateFamilyModalComponent } from '../family/components/create-family-modal/create-family-modal.component';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, ButtonComponent, CreateFamilyModalComponent],
  template: `
    <div class="min-h-screen bg-gray-50">
      <!-- Header -->
      <header class="bg-white shadow">
        <div class="max-w-7xl mx-auto px-4 py-4 sm:px-6 lg:px-8 flex justify-between items-center">
          <h1 class="text-2xl font-bold text-gray-900">
            Family Hub
          </h1>
          <div class="flex items-center space-x-4">
            <span class="text-gray-700" *ngIf="user()">
              {{ user()?.email }}
            </span>
            <fh-button
              variant="tertiary"
              size="sm"
              (clicked)="logout()"
            >
              Sign Out
            </fh-button>
          </div>
        </div>
      </header>

      <!-- Main Content -->
      <main class="max-w-7xl mx-auto px-4 py-8 sm:px-6 lg:px-8">
        @if (isLoadingFamilies()) {
          <!-- Loading State -->
          <div class="flex justify-center items-center py-12">
            <fh-spinner size="lg" />
            <p class="ml-3 text-gray-600">Loading...</p>
          </div>
        } @else if (familyError()) {
          <!-- Error State -->
          <div class="bg-red-50 border border-red-200 rounded-lg p-6">
            <h3 class="font-medium text-red-900 mb-2">Error Loading Families</h3>
            <p class="text-sm text-red-700">{{ familyError() }}</p>
            <fh-button
              variant="primary"
              size="sm"
              class="mt-4"
              (clicked)="retryLoadFamilies()"
            >
              Retry
            </fh-button>
          </div>
        } @else if (hasFamily()) {
          <!-- Authenticated Dashboard (has family) -->
          <div class="bg-white rounded-lg shadow p-6">
            <h2 class="text-xl font-semibold text-gray-900 mb-4">
              Welcome to {{ currentFamily()?.name }}! 👋
            </h2>

            <div class="space-y-4">
              <div class="bg-blue-50 border border-blue-200 rounded-lg p-4">
                <h3 class="font-medium text-blue-900 mb-2">Your Family</h3>
                <dl class="space-y-1 text-sm">
                  <div class="flex">
                    <dt class="font-medium text-blue-900 w-32">Name:</dt>
                    <dd class="text-blue-700">{{ currentFamily()?.name }}</dd>
                  </div>
                  <div class="flex">
                    <dt class="font-medium text-blue-900 w-32">Created:</dt>
                    <dd class="text-blue-700">
                      {{ currentFamily()?.createdAt | date:'medium' }}
                    </dd>
                  </div>
                </dl>
              </div>

              <div class="bg-gray-50 border border-gray-200 rounded-lg p-4">
                <h3 class="font-medium text-gray-900 mb-2">Next Steps</h3>
                <ul class="list-disc list-inside space-y-1 text-sm text-gray-700">
                  <li>Invite family members</li>
                  <li>Create your first calendar event</li>
                  <li>Set up recurring tasks</li>
                  <li>Start a shopping list</li>
                </ul>
              </div>
            </div>
          </div>
        }
      </main>

      <!-- Create Family Modal (shown when no family) -->
      <fh-create-family-modal
        [isOpen]="showCreateFamilyModal()"
        (success)="handleFamilyCreated()"
        (skip)="handleSkipFamilyCreation()"
      />
    </div>
  `
})
export class DashboardComponent implements OnInit {
  user = computed(() => this.authService.currentUser());

  // Family state from service
  currentFamily = computed(() => this.familyService.currentFamily());
  hasFamily = computed(() => this.familyService.hasFamily());
  isLoadingFamilies = computed(() => this.familyService.isLoading());
  familyError = computed(() => this.familyService.error());

  // Modal state
  showCreateFamilyModal = computed(() =>
    !this.isLoadingFamilies() &&
    !this.hasFamily() &&
    !this.familyError()
  );

  constructor(
    private authService: AuthService,
    private familyService: FamilyService,
    private router: Router
  ) {}

  async ngOnInit() {
    // Fetch user families on component init
    await this.familyService.getUserFamilies();
  }

  logout(): void {
    if (confirm('Are you sure you want to sign out?')) {
      this.authService.logout();
    }
  }

  handleFamilyCreated(): void {
    // Modal closes automatically (showCreateFamilyModal computed signal updates)
    // Family state already updated by FamilyService
    console.log('Family created successfully!');
  }

  handleSkipFamilyCreation(): void {
    // User chose to skip family creation
    if (confirm(
      'You need a family to use Family Hub.\n\n' +
      'Would you like to sign out and create a family later?'
    )) {
      this.authService.logout();
    }
  }

  async retryLoadFamilies(): Promise<void> {
    await this.familyService.getUserFamilies();
  }
}
```

**Test File:** Integration tests for family status check, modal display, navigation.

### Phase 4 Success Criteria

- ✅ Dashboard fetches families on init
- ✅ Modal shown when no family
- ✅ Dashboard content shown when has family
- ✅ "Skip" action logs user out with confirmation
- ✅ All tests passing

---

## Phase 5: E2E & Accessibility Testing (0.5 days)

**Objective:** Test complete user flow with E2E tests and automated accessibility testing.

### 5.1 Setup Playwright (if not already configured)

```bash
npm install -D @playwright/test @axe-core/playwright
npx playwright install
```

### 5.2 E2E Test Scenarios

**File:** `/home/andrekirst/git/github/andrekirst/family2/src/frontend/family-hub-web/e2e/family-creation.spec.ts`

```typescript
import { test, expect } from '@playwright/test';
import { injectAxe, checkA11y } from '@axe-core/playwright';

test.describe('Family Creation Flow', () => {
  test('should show CreateFamilyModal for new user without family', async ({ page }) => {
    // Mock GraphQL response - no families
    await page.route('**/graphql', async route => {
      const postData = route.request().postDataJSON();

      if (postData.query.includes('getUserFamilies')) {
        await route.fulfill({
          json: {
            data: {
              getUserFamilies: {
                families: []
              }
            }
          }
        });
      } else {
        await route.continue();
      }
    });

    await page.goto('/dashboard');

    // Verify modal is shown
    await expect(page.locator('[role="dialog"]')).toBeVisible();
    await expect(page.locator('h2')).toContainText('Create Your Family');
  });

  test('should create family successfully', async ({ page }) => {
    // Mock GraphQL responses
    await page.route('**/graphql', async route => {
      const postData = route.request().postDataJSON();

      if (postData.query.includes('getUserFamilies')) {
        await route.fulfill({
          json: {
            data: {
              getUserFamilies: { families: [] }
            }
          }
        });
      } else if (postData.query.includes('createFamily')) {
        await route.fulfill({
          json: {
            data: {
              createFamily: {
                family: {
                  id: '123',
                  name: 'Test Family',
                  ownerId: 'user-123',
                  createdAt: new Date().toISOString()
                },
                errors: null
              }
            }
          }
        });
      } else {
        await route.continue();
      }
    });

    await page.goto('/dashboard');

    // Fill in family name
    await page.fill('input[placeholder*="Smith Family"]', 'Test Family');

    // Submit form
    await page.click('fh-button:has-text("Create Family")');

    // Wait for modal to close
    await expect(page.locator('[role="dialog"]')).not.toBeVisible();

    // Verify dashboard shows family name
    await expect(page.locator('h2')).toContainText('Welcome to Test Family');
  });

  test('should show validation error for empty name', async ({ page }) => {
    await page.goto('/dashboard');

    // Click submit without entering name
    await page.click('fh-button:has-text("Create Family")');

    // Verify validation error
    await expect(page.locator('[role="alert"]')).toContainText('Family name is required');
  });

  test('should handle "Skip for now" action', async ({ page }) => {
    await page.goto('/dashboard');

    // Click "Skip for now"
    await page.click('button:has-text("Skip for now")');

    // Confirm logout
    page.on('dialog', dialog => dialog.accept());

    // Verify redirected to login
    await expect(page).toHaveURL(/\/login/);
  });

  test('modal should be accessible', async ({ page }) => {
    await page.goto('/dashboard');

    // Inject axe-core
    await injectAxe(page);

    // Check accessibility of modal
    await checkA11y(page, '[role="dialog"]', {
      detailedReport: true,
      detailedReportOptions: {
        html: true
      }
    });
  });

  test('should trap focus within modal', async ({ page }) => {
    await page.goto('/dashboard');

    // Focus first input
    await page.focus('input[placeholder*="Smith Family"]');

    // Tab to next element (should be Submit button)
    await page.keyboard.press('Tab');
    await expect(page.locator('fh-button:has-text("Create Family")')).toBeFocused();

    // Tab again (should wrap to "Skip" link)
    await page.keyboard.press('Tab');
    await expect(page.locator('button:has-text("Skip for now")')).toBeFocused();

    // Tab again (should wrap back to input)
    await page.keyboard.press('Tab');
    await expect(page.locator('input[placeholder*="Smith Family"]')).toBeFocused();
  });
});
```

### 5.3 Manual Testing Checklist

**Browser Testing:**
- ✅ Chrome (latest)
- ✅ Firefox (latest)
- ✅ Safari (latest)
- ✅ Edge (latest)

**Device Testing:**
- ✅ Desktop (1920x1080)
- ✅ Tablet (768x1024)
- ✅ Mobile (375x667)

**Screen Reader Testing:**
- ✅ macOS VoiceOver + Safari
- ✅ NVDA + Firefox (Windows)
- ✅ JAWS + Chrome (Windows, if available)

**Keyboard Navigation:**
- ✅ Tab cycles through focusable elements
- ✅ Enter submits form
- ✅ Focus trapped in modal
- ✅ Visible focus indicators

### Phase 5 Success Criteria

- ✅ All E2E tests passing
- ✅ Automated accessibility tests passing (axe-core)
- ✅ Manual screen reader testing complete
- ✅ Cross-browser testing complete
- ✅ Responsive design verified

---

## File Structure Summary

**New Files Created:**

```
/home/andrekirst/git/github/andrekirst/family2/src/frontend/family-hub-web/src/app/

core/
  models/
    📝 graphql.models.ts                  # NEW (Phase 0)
    📝 family.models.ts                   # NEW (Phase 2)
  services/
    ✏️  graphql.service.ts                # UPDATED (Phase 0)
    📝 graphql.service.spec.ts            # NEW (Phase 0)
    📝 family.service.ts                  # NEW (Phase 2)
    📝 family.service.spec.ts             # NEW (Phase 2)

shared/
  components/
    atoms/
      ✅ button/                          # EXISTING
      ✅ spinner/                         # EXISTING
      📝 input/                           # NEW (Phase 1)
        input.component.ts
        input.component.spec.ts
      📝 icon/                            # NEW (Phase 1)
        icon.component.ts
        icon.component.spec.ts
    molecules/
      📝 modal/                           # NEW (Phase 1)
        modal.component.ts
        modal.component.spec.ts

features/
  family/
    components/
      📝 create-family-modal/             # NEW (Phase 3)
        create-family-modal.component.ts
        create-family-modal.component.spec.ts
  dashboard/
    ✏️  dashboard.component.ts            # UPDATED (Phase 4)
    ✏️  dashboard.component.spec.ts       # UPDATED (Phase 4)

e2e/
  📝 family-creation.spec.ts              # NEW (Phase 5)
```

**Dependencies to Install:**
```bash
npm install focus-trap
npm install -D @types/focus-trap
npm install -D @playwright/test @axe-core/playwright
```

**Total Files:** 16 files (12 new, 4 updated)

---

## Timeline

| Phase | Duration | Deliverables | Agent Perspective |
|-------|----------|--------------|-------------------|
| **Phase 0: Infrastructure** | 0.5 days | GraphQL types, extended service | @agent-typescript-pro |
| **Phase 1: Core Components** | 2 days | Input, Icon, Modal (ONLY) | @agent-ui-designer (YAGNI) |
| **Phase 2: Family Service** | 1 day | Domain models, service, tests | @agent-typescript-pro |
| **Phase 3: CreateFamilyModal** | 1.5 days | Reactive form, validation, a11y | @agent-angular-architect, @agent-ux-researcher |
| **Phase 4: Dashboard Integration** | 0.5 days | Family check, modal trigger | @agent-angular-architect |
| **Phase 5: E2E & A11y Testing** | 0.5 days | E2E tests, axe-core, manual QA | @agent-ux-researcher |
| **TOTAL** | **6 days** | | |

**Savings vs Original Plan:** -1 day (17% faster)

---

## Risk Mitigation

| Risk | Mitigation | Agent Perspective |
|------|------------|-------------------|
| GraphQLService changes break existing code | Extend (don't replace), keep old methods | @agent-typescript-pro |
| ControlValueAccessor complexity | Use reactive forms directly, input component stays simple | @agent-angular-architect |
| Blocking modal too aggressive | Add "Skip for now" with logout confirmation | @agent-ux-researcher |
| Focus trap bugs | Use battle-tested `focus-trap` library | @agent-ux-researcher |
| Validation UX too strict | Show errors only after blur, not while typing | @agent-ux-researcher |
| Mobile modal layout issues | Max-height + overflow, responsive padding | @agent-ui-designer |

---

## Critical Files for Implementation

1. **/home/andrekirst/git/github/andrekirst/family2/src/frontend/family-hub-web/src/app/core/services/graphql.service.ts**
   - **Reason:** Foundation for error handling - must be fixed first
   - **Agent:** @agent-typescript-pro

2. **/home/andrekirst/git/github/andrekirst/family2/src/frontend/family-hub-web/src/app/shared/components/atoms/input/input.component.ts**
   - **Reason:** Reactive forms integration via ControlValueAccessor
   - **Agent:** @agent-angular-architect

3. **/home/andrekirst/git/github/andrekirst/family2/src/frontend/family-hub-web/src/app/shared/components/molecules/modal/modal.component.ts**
   - **Reason:** Focus trap, accessibility, blocking pattern
   - **Agent:** @agent-ux-researcher

4. **/home/andrekirst/git/github/andrekirst/family2/src/frontend/family-hub-web/src/app/core/services/family.service.ts**
   - **Reason:** Business logic layer with Signals state management
   - **Agent:** @agent-typescript-pro, @agent-angular-architect

5. **/home/andrekirst/git/github/andrekirst/family2/src/frontend/family-hub-web/src/app/features/family/components/create-family-modal/create-family-modal.component.ts**
   - **Reason:** Feature orchestration - validation, API, UX
   - **Agent:** All perspectives integrated

---

## Success Criteria

**Functional:**
- ✅ User without family sees blocking modal on login
- ✅ User with family goes directly to dashboard
- ✅ Form validates after blur (not while typing)
- ✅ Character counter color-coded (gray → amber → red)
- ✅ API errors displayed with clear messages
- ✅ "Skip for now" logs user out with confirmation
- ✅ Modal closes on successful creation

**Non-Functional:**
- ✅ Test coverage >80% (unit + integration)
- ✅ All E2E scenarios passing
- ✅ Accessibility audit passing (WCAG 2.1 AA)
- ✅ Focus trap working correctly
- ✅ Screen reader announcements working
- ✅ Responsive design (mobile, tablet, desktop)
- ✅ No console errors
- ✅ TypeScript strict mode for new files

**Code Quality:**
- ✅ Reactive Forms (not template-driven)
- ✅ Angular Signals for state management
- ✅ Type-safe domain models with mappers
- ✅ No breaking changes to existing code
- ✅ YAGNI principle applied (minimal components)

---

**END OF MULTI-AGENT IMPLEMENTATION PLAN**

This plan incorporates insights from 4 specialized perspectives:
- **@agent-angular-architect:** Reactive forms, Signals, architecture patterns
- **@agent-typescript-pro:** Type safety, error handling, domain modeling
- **@agent-ui-designer:** YAGNI, Tailwind patterns, visual consistency
- **@agent-ux-researcher:** Accessibility, validation UX, user flow
