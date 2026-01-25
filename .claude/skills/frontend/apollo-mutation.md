---
name: apollo-mutation
description: Create typed Apollo GraphQL mutation with error handling
category: frontend
module-aware: false
inputs:
  - mutationName: Mutation name (e.g., CreateFamily)
  - inputFields: Input fields for mutation
---

# Apollo GraphQL Mutation Skill

Create a typed Apollo Client mutation with proper error handling and optimistic updates.

## Steps

### 1. Define GraphQL Mutation

**Location:** `src/app/graphql/mutations/{mutation-name}.mutation.ts`

```typescript
import { gql } from 'apollo-angular';

export const {MUTATION_NAME} = gql`
  mutation {MutationName}($input: {MutationName}Input!) {
    {mutationField}(input: $input) {
      id
      {field1}
      {field2}
    }
  }
`;
```

### 2. Define TypeScript Interfaces

**Location:** `src/app/models/{entity}.model.ts`

```typescript
export interface {MutationName}Input {
  {field1}: {type1};
  {field2}?: {type2};  // Optional field
}

export interface {MutationName}Response {
  {mutationField}: {
    id: string;
    {field1}: {type1};
    {field2}: {type2};
  };
}
```

### 3. Create Service Method

**Location:** `src/app/services/{entity}.service.ts`

```typescript
import { Injectable, inject } from '@angular/core';
import { Apollo } from 'apollo-angular';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { {MUTATION_NAME} } from '../graphql/mutations/{mutation-name}.mutation';
import { {MutationName}Input, {MutationName}Response } from '../models/{entity}.model';

@Injectable({ providedIn: 'root' })
export class {Entity}Service {
  private apollo = inject(Apollo);

  {mutationMethod}(input: {MutationName}Input): Observable<{MutationName}Response> {
    return this.apollo.mutate<{MutationName}Response>({
      mutation: {MUTATION_NAME},
      variables: { input }
    }).pipe(
      map(result => {
        if (result.errors) {
          throw new Error(result.errors[0].message);
        }
        return result.data!;
      })
    );
  }
}
```

### 4. Use in Component

```typescript
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { {Entity}Service } from '../services/{entity}.service';

@Component({
  selector: 'app-create-{entity}',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <form (ngSubmit)="submit()">
      <input [(ngModel)]="name" name="name" placeholder="Name" />
      <button type="submit" [disabled]="loading()">
        {{ loading() ? 'Creating...' : 'Create' }}
      </button>
      @if (error()) {
        <div class="error">{{ error() }}</div>
      }
    </form>
  `
})
export class Create{Entity}Component {
  private service = inject({Entity}Service);

  name = '';
  loading = signal(false);
  error = signal<string | null>(null);

  submit(): void {
    if (!this.name.trim()) return;

    this.loading.set(true);
    this.error.set(null);

    this.service.{mutationMethod}({ name: this.name }).subscribe({
      next: (result) => {
        console.log('Created:', result);
        this.loading.set(false);
        // Navigate or show success
      },
      error: (error) => {
        console.error('Error:', error);
        this.error.set(error.message);
        this.loading.set(false);
      }
    });
  }
}
```

## With Cache Update

```typescript
this.apollo.mutate<CreateFamilyResponse>({
  mutation: CREATE_FAMILY,
  variables: { input },
  update: (cache, { data }) => {
    // Update cache after mutation
    const existing = cache.readQuery<GetFamiliesResponse>({
      query: GET_FAMILIES
    });

    if (existing && data) {
      cache.writeQuery({
        query: GET_FAMILIES,
        data: {
          families: [...existing.families, data.createFamily]
        }
      });
    }
  }
});
```

## Error Handling Patterns

```typescript
// Handle GraphQL errors
.pipe(
  catchError(error => {
    if (error.graphQLErrors?.length > 0) {
      // Business logic errors
      const message = error.graphQLErrors[0].message;
      this.error.set(message);
    } else if (error.networkError) {
      // Network errors
      this.error.set('Network error. Please try again.');
    }
    return throwError(() => error);
  })
)
```

## Validation

- [ ] Mutation defined with gql template literal
- [ ] Input interface matches GraphQL schema
- [ ] Service method wraps Apollo mutate
- [ ] Error handling in component
- [ ] Loading state managed
