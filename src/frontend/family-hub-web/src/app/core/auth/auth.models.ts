/**
 * Models for authentication and user profile
 */

/**
 * User profile information from JWT ID token
 * Family context is fetched from GraphQL API, not stored in JWT
 */
export interface UserProfile {
  userId: string;
  email: string;
  name: string;
  emailVerified: boolean;
}

/**
 * OAuth tokens from Keycloak (snake_case as returned by OAuth server)
 */
export interface AuthTokens {
  access_token: string;
  id_token: string;
  refresh_token: string;
  expires_in: number;
  token_type: string;
}

/**
 * Decoded JWT token payload (standard OIDC claims only)
 */
export interface JwtPayload {
  sub: string;
  email: string;
  name: string;
  email_verified: boolean;
  exp: number;
  iat: number;
  iss: string;
  aud: string;
}
