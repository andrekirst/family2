/**
 * Models for authentication and user profile
 */

/**
 * User profile information from JWT ID token
 */
export interface UserProfile {
  userId: string;
  email: string;
  name: string;
  emailVerified: boolean;
  familyId?: string;
  familyRole?: string;
}

/**
 * OAuth tokens from Keycloak
 */
export interface AuthTokens {
  accessToken: string;
  idToken: string;
  refreshToken: string;
  expiresIn: number;
  tokenType: string;
}

/**
 * Decoded JWT token payload
 */
export interface JwtPayload {
  sub: string;
  email: string;
  name: string;
  email_verified: boolean;
  family_id?: string;
  family_role?: string;
  exp: number;
  iat: number;
  iss: string;
  aud: string;
}
