---
name: apollo-query
description: Create typed Apollo GraphQL query with error handling
category: frontend
module-aware: false
inputs:
  - queryName: Query name (e.g., GetCurrentFamily)
  - fields: GraphQL fields to fetch
---

# Apollo GraphQL Query Skill

Create a typed Apollo Client query with proper error handling and loading states.

## Steps

### 1. Define GraphQL Query

**Location:** `src/app/graphql/queries/{query-name}.query.ts`

```typescript
import { gql } from 'apollo-angular';

export const {QUERY_NAME} = gql`
  query {QueryName} {
    {queryField} {
      id
      {field1}
      {field2}
      createdAt
    }
  }
`;
```

### 2. Define TypeScript Interface

**Location:** `src/app/models/{entity}.model.ts`

```typescript
export interface {Entity} {
  id: string;
  {field1}: {type1};
  {field2}: {type2};
  createdAt: string;
}

export interface {QueryName}Response {
  {queryField}: {Entity};
}
```

### 3. Use in Component

```typescript
import { Component, inject, signal } from '@angular/core';
import { Apollo } from 'apollo-angular';
import { map, catchError } from 'rxjs/operators';
import { of } from 'rxjs';
import { {QUERY_NAME} } from '../graphql/queries/{query-name}.query';
import { {Entity}, {QueryName}Response } from '../models/{entity}.model';

@Component({
  selector: 'app-{entity}',
  standalone: true,
  imports: [CommonModule],
  template: `
    @if (loading()) {
      <div class="loading">Loading...</div>
    } @else if (error()) {
      <div class="error">{{ error() }}</div>
    } @else if (data()) {
      <div class="data">{{ data()?.name }}</div>
    }
  `
})
export class {Entity}Component {
  private apollo = inject(Apollo);

  loading = signal(true);
  error = signal<string | null>(null);
  data = signal<{Entity} | null>(null);

  constructor() {
    this.loadData();
  }

  private loadData(): void {
    this.apollo.query<{QueryName}Response>({
      query: {QUERY_NAME}
    }).pipe(
      map(result => result.data.{queryField}),
      catchError(error => {
        console.error('GraphQL Error:', error);
        this.error.set(error.message);
        return of(null);
      })
    ).subscribe(data => {
      this.data.set(data);
      this.loading.set(false);
    });
  }
}
```

## Query with Variables

```typescript
export const GET_FAMILY_BY_ID = gql`
  query GetFamilyById($id: UUID!) {
    family(id: $id) {
      id
      name
      members {
        id
        email
        role
      }
    }
  }
`;

// Usage
this.apollo.query<GetFamilyByIdResponse>({
  query: GET_FAMILY_BY_ID,
  variables: { id: familyId }
}).subscribe(result => {
  this.data.set(result.data.family);
});
```

## With RxJS Observables

```typescript
// Reactive approach with automatic updates
family$ = this.apollo.watchQuery<GetFamilyResponse>({
  query: GET_CURRENT_FAMILY,
  fetchPolicy: 'cache-and-network'
}).valueChanges.pipe(
  map(result => result.data.currentFamily),
  catchError(error => {
    console.error('Error:', error);
    return of(null);
  })
);
```

## Validation

- [ ] Query defined with gql template literal
- [ ] TypeScript interfaces match GraphQL schema
- [ ] Error handling with catchError
- [ ] Loading state managed
- [ ] Component uses standalone: true
