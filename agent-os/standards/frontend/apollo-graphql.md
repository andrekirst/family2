# Apollo GraphQL

Use Apollo Client for GraphQL with typed operations.

## Query Pattern

```typescript
import { gql, Apollo } from 'apollo-angular';

const GET_CURRENT_FAMILY = gql`
  query GetCurrentFamily {
    currentFamily {
      id
      name
      members { id email role }
    }
  }
`;

@Component({ ... })
export class FamilyComponent {
  private apollo = inject(Apollo);

  family$ = this.apollo.query({
    query: GET_CURRENT_FAMILY
  }).pipe(
    map(result => result.data.currentFamily)
  );
}
```

## Mutation Pattern

```typescript
const CREATE_FAMILY = gql`
  mutation CreateFamily($input: CreateFamilyInput!) {
    createFamily(input: $input) {
      familyId
      name
    }
  }
`;

createFamily(name: string) {
  this.apollo.mutate({
    mutation: CREATE_FAMILY,
    variables: { input: { name } }
  }).subscribe({
    next: (result) => console.log('Created:', result.data),
    error: (error) => console.error('Error:', error)
  });
}
```

## Error Handling

```typescript
family$ = this.apollo.query({ query: GET_CURRENT_FAMILY }).pipe(
  map(result => result.data.currentFamily),
  catchError(error => {
    console.error('GraphQL Error:', error);
    return of(null);
  })
);
```

## Rules

- Use `inject(Apollo)` for dependency injection
- Handle errors with catchError
- Use typed operations (gql tagged templates)
