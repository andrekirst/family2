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
      familyId
      permissions
      preferredLocale
    }
  }
`;

/**
 * Fetch current user profile with family membership.
 *
 * Uses hierarchical query: me { profile { ... } }
 *
 * Returns null if user not found (e.g., JWT valid but user not in database).
 */
/**
 * Update the current user's preferred locale in the backend.
 * Called after locale switch to persist the preference cross-device.
 */
export const UPDATE_MY_LOCALE_MUTATION = gql`
  mutation UpdateMyLocale($input: UpdateUserLocaleRequestInput!) {
    updateMyLocale(input: $input)
  }
`;

export const GET_CURRENT_USER_QUERY = gql`
  query GetMyProfile {
    me {
      profile {
        id
        email
        name
        emailVerified
        isActive
        familyId
        avatarId
        permissions
        preferredLocale
      }
    }
  }
`;
