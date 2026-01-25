---
name: component-test
description: Create Angular component test with Jasmine and TestBed
category: testing
module-aware: false
inputs:
  - componentName: Component being tested
  - hasGraphQL: Whether component uses Apollo Client
---

# Component Test Skill

Create Angular component unit tests with Jasmine and TestBed.

## Steps

### 1. Create Basic Component Test

**Location:** `src/app/components/{component}/{component}.component.spec.ts`

```typescript
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { {Component}Component } from './{component}.component';

describe('{Component}Component', () => {
  let component: {Component}Component;
  let fixture: ComponentFixture<{Component}Component>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [{Component}Component]  // Standalone component
    }).compileComponents();

    fixture = TestBed.createComponent({Component}Component);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should render title', () => {
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('h1')?.textContent).toContain('Expected Title');
  });

  it('should toggle state on click', () => {
    expect(component.isActive()).toBeFalse();

    component.toggle();

    expect(component.isActive()).toBeTrue();
  });
});
```

### 2. Test with Mocked Service

```typescript
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { {Component}Component } from './{component}.component';
import { {Service}Service } from '../../services/{service}.service';
import { of } from 'rxjs';

describe('{Component}Component with Service', () => {
  let component: {Component}Component;
  let fixture: ComponentFixture<{Component}Component>;
  let serviceMock: jasmine.SpyObj<{Service}Service>;

  beforeEach(async () => {
    serviceMock = jasmine.createSpyObj('{Service}Service', ['getData', 'saveData']);
    serviceMock.getData.and.returnValue(of({ id: '1', name: 'Test' }));

    await TestBed.configureTestingModule({
      imports: [{Component}Component],
      providers: [
        { provide: {Service}Service, useValue: serviceMock }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent({Component}Component);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should load data on init', () => {
    expect(serviceMock.getData).toHaveBeenCalled();
    expect(component.data()).toEqual({ id: '1', name: 'Test' });
  });

  it('should call saveData when form submitted', () => {
    serviceMock.saveData.and.returnValue(of({ success: true }));

    component.onSubmit();

    expect(serviceMock.saveData).toHaveBeenCalled();
  });
});
```

### 3. Test with Apollo Client

```typescript
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ApolloTestingModule, ApolloTestingController } from 'apollo-angular/testing';
import { {Component}Component } from './{component}.component';
import { GET_DATA } from '../../graphql/queries/get-data.query';

describe('{Component}Component with Apollo', () => {
  let component: {Component}Component;
  let fixture: ComponentFixture<{Component}Component>;
  let apolloController: ApolloTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        {Component}Component,
        ApolloTestingModule
      ]
    }).compileComponents();

    fixture = TestBed.createComponent({Component}Component);
    component = fixture.componentInstance;
    apolloController = TestBed.inject(ApolloTestingController);
  });

  afterEach(() => {
    apolloController.verify();
  });

  it('should load data from GraphQL', () => {
    fixture.detectChanges();

    const op = apolloController.expectOne(GET_DATA);
    expect(op.operation.operationName).toBe('GetData');

    op.flush({
      data: {
        getData: { id: '1', name: 'Test Data' }
      }
    });

    expect(component.data()?.name).toBe('Test Data');
  });

  it('should handle GraphQL error', () => {
    fixture.detectChanges();

    const op = apolloController.expectOne(GET_DATA);

    op.graphqlErrors([{ message: 'Error loading data' }]);

    expect(component.error()).toBe('Error loading data');
  });
});
```

### 4. Test User Interactions

```typescript
import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';

describe('User Interactions', () => {
  it('should update on button click', fakeAsync(() => {
    const button = fixture.nativeElement.querySelector('button');

    button.click();
    tick();
    fixture.detectChanges();

    expect(component.clicked()).toBeTrue();
  }));

  it('should emit event on submit', () => {
    spyOn(component.submitted, 'emit');

    component.onSubmit();

    expect(component.submitted.emit).toHaveBeenCalledWith(jasmine.objectContaining({
      name: 'Expected Name'
    }));
  });

  it('should validate form inputs', () => {
    component.nameInput.set('');
    fixture.detectChanges();

    const errorElement = fixture.nativeElement.querySelector('.error');
    expect(errorElement?.textContent).toContain('Name is required');
  });
});
```

### 5. Test with Router

```typescript
import { RouterTestingModule } from '@angular/router/testing';
import { Router } from '@angular/router';

describe('Component with Router', () => {
  let router: Router;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        {Component}Component,
        RouterTestingModule.withRoutes([
          { path: 'success', component: SuccessComponent }
        ])
      ]
    }).compileComponents();

    router = TestBed.inject(Router);
  });

  it('should navigate on success', fakeAsync(() => {
    spyOn(router, 'navigate');

    component.onSuccess();
    tick();

    expect(router.navigate).toHaveBeenCalledWith(['/success']);
  }));
});
```

## Validation

- [ ] Test file next to component (.spec.ts)
- [ ] Uses TestBed.configureTestingModule
- [ ] Mocks external dependencies
- [ ] Tests user interactions
- [ ] Tests error states
- [ ] Cleans up after each test (Apollo verify)
