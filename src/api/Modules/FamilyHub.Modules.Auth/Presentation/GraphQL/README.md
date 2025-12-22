# GraphQL API - Auth Module

## Mutations

### registerUser

Registers a new user with email and password authentication.

**Endpoint:** `/graphql`

**Mutation:**

```graphql
mutation RegisterUser($input: RegisterUserInput!) {
  registerUser(input: $input) {
    user {
      id
      email
      emailVerified
      createdAt
    }
    errors {
      message
      code
      field
    }
  }
}
```

**Variables:**

```json
{
  "input": {
    "email": "user@example.com",
    "password": "SecureP@ss123"
  }
}
```

**Password Requirements:**
- Minimum 8 characters
- Maximum 128 characters
- At least 1 uppercase letter (A-Z)
- At least 1 lowercase letter (a-z)
- At least 1 digit (0-9)
- At least 1 special character (!@#$%^&*, etc.)

**Success Response:**

```json
{
  "data": {
    "registerUser": {
      "user": {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "email": "user@example.com",
        "emailVerified": false,
        "createdAt": "2025-12-22T07:30:00Z"
      },
      "errors": null
    }
  }
}
```

**Validation Error Response:**

```json
{
  "data": {
    "registerUser": {
      "user": null,
      "errors": [
        {
          "message": "Password must be at least 8 characters long.",
          "code": "VALIDATION_ERROR",
          "field": "Password"
        },
        {
          "message": "Password must contain at least one uppercase letter.",
          "code": "VALIDATION_ERROR",
          "field": "Password"
        }
      ]
    }
  }
}
```

**Duplicate Email Error Response:**

```json
{
  "data": {
    "registerUser": {
      "user": null,
      "errors": [
        {
          "message": "A user with email 'user@example.com' already exists.",
          "code": "DUPLICATE_EMAIL",
          "field": "email"
        }
      ]
    }
  }
}
```

**Error Codes:**

| Code | Description | Field |
|------|-------------|-------|
| `VALIDATION_ERROR` | Input validation failed (email format, password strength) | Varies |
| `DUPLICATE_EMAIL` | Email already registered | `email` |
| `INTERNAL_ERROR` | Unexpected server error | `null` |

---

### login

Authenticates a user with email and password, returning JWT access and refresh tokens.

**Endpoint:** `/graphql`

**Mutation:**

```graphql
mutation Login($input: LoginInput!) {
  login(input: $input) {
    authentication {
      user {
        id
        email
        emailVerified
        createdAt
      }
      accessToken
      refreshToken
      expiresAt
    }
    errors {
      message
      code
      field
    }
  }
}
```

**Variables:**

```json
{
  "input": {
    "email": "user@example.com",
    "password": "SecureP@ss123"
  }
}
```

**Success Response:**

```json
{
  "data": {
    "login": {
      "authentication": {
        "user": {
          "id": "038625d8-b3af-4973-97aa-277fea682add",
          "email": "testuser@example.com",
          "emailVerified": false,
          "createdAt": "2025-12-22T07:14:30.689Z"
        },
        "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
        "refreshToken": "alCh+58ZDmqgwhkwvNSBLAfQBoLFeylpxV8HWz6P6hNc9GAPvbL05VqkXPLspAElB9QnQomL7lrNZxuF2dNmXQ==",
        "expiresAt": "2025-12-22T07:29:30.623Z"
      },
      "errors": null
    }
  }
}
```

**Invalid Credentials Response:**

```json
{
  "data": {
    "login": {
      "authentication": null,
      "errors": [
        {
          "message": "Invalid email or password.",
          "code": "INVALID_CREDENTIALS",
          "field": null
        }
      ]
    }
  }
}
```

**JWT Token Claims:**

The access token contains the following claims:
- `sub` (subject): User ID (GUID)
- `email`: User's email address
- `jti` (JWT ID): Unique token identifier
- `iat` (issued at): Unix timestamp when token was created
- `nbf` (not before): Unix timestamp when token becomes valid
- `exp` (expires): Unix timestamp when token expires (15 minutes from issuance)
- `iss` (issuer): API URL (http://localhost:5002)
- `aud` (audience): Client identifier (family-hub-client)

**Token Expiration:**
- Access Token: 15 minutes
- Refresh Token: 7 days

**Error Codes:**

| Code | Description | Field |
|------|-------------|-------|
| `VALIDATION_ERROR` | Input validation failed (email format required) | Varies |
| `INVALID_CREDENTIALS` | Email or password is incorrect | `null` |
| `INTERNAL_ERROR` | Unexpected server error | `null` |

---

## Testing with GraphQL Playground

1. Start the API:
   ```bash
   dotnet run --project FamilyHub.Api
   ```

2. Open Banana Cake Pop (Hot Chocolate GraphQL IDE):
   ```
   http://localhost:5000/graphql
   ```

3. Use the mutation example above to register a user

4. Verify in database:
   ```bash
   docker exec familyhub-postgres psql -U familyhub -d familyhub -c "SELECT id, email, email_verified, created_at FROM auth.users;"
   ```

---

## Integration with Frontend

**TypeScript/Apollo Client Example:**

```typescript
import { gql, useMutation } from '@apollo/client';

const REGISTER_USER = gql`
  mutation RegisterUser($input: RegisterUserInput!) {
    registerUser(input: $input) {
      user {
        id
        email
        emailVerified
        createdAt
      }
      errors {
        message
        code
        field
      }
    }
  }
`;

function RegisterForm() {
  const [registerUser, { data, loading, error }] = useMutation(REGISTER_USER);

  const handleSubmit = async (email: string, password: string) => {
    const result = await registerUser({
      variables: {
        input: { email, password }
      }
    });

    if (result.data.registerUser.errors) {
      // Handle validation/business errors
      console.error(result.data.registerUser.errors);
    } else {
      // Success - user registered
      console.log('Registered:', result.data.registerUser.user);
    }
  };

  return (/* ... */);
}
```

---

## Security Considerations

1. **Password Hashing:** Passwords are hashed using BCrypt with work factor 12 (~250ms)
2. **Email Uniqueness:** Checked at both database (unique constraint) and application level
3. **Validation:** Automatic validation via FluentValidation pipeline behavior
4. **SQL Injection:** Protected by EF Core parameterized queries
5. **Rate Limiting:** TODO - Implement rate limiting for registration endpoint

---

## Next Steps

- [ ] Add email verification flow
- [x] Implement login mutation with JWT tokens
- [ ] Implement refresh token mutation
- [ ] Integrate Zitadel OAuth 2.0
- [ ] Add rate limiting (e.g., max 5 registrations/logins per IP per hour)
