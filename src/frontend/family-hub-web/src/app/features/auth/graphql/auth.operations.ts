import { gql } from 'apollo-angular';

/**
 * Register or update user in backend after OAuth login.
 *
 * Backend extracts user data from JWT claims (sub, email, name, email_verified).
 * Input fields are required by GraphQL schema but values come from JWT.
 *
 * This mutation should be called once after successful OAuth callback
 * to ensure the user exists in the backend database.
 */
export const REGISTER_USER_MUTATION = gql`
  mutation RegisterUser($input: RegisterUserRequestInput!) {
    registerUser(input: $input) {
      id
      email
      name
      emailVerified
      isActive
    }
  }
`;

/**
 * Fetch current user profile with family membership.
 *
 * Used by dashboard and protected routes to get user data from backend.
 *
 * Returns null if user not found (e.g., JWT valid but user not in database).
 */
export const GET_CURRENT_USER_QUERY = gql`
  query GetCurrentUser {
    getCurrentUser {
      id
      email
      name
      emailVerified
      isActive
      family {
        id
        name
      }
    }
  }
`;
