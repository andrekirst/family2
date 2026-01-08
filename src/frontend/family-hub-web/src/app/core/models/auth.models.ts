export interface User {
  id: string;
  email: string;
  emailVerified: boolean;
  createdAt: Date;
}

export interface AuthState {
  isAuthenticated: boolean;
  user: User | null;
  accessToken: string | null;
  expiresAt: Date | null;
}

export interface GetZitadelAuthUrlResponse {
  auth: {
    url: {
      authorizationUrl: string;
      codeVerifier: string;
      state: string;
    };
  };
}

/**
 * Backend GraphQL response structure (matches backend schema)
 */
interface CompleteZitadelLoginUserGQL {
  id: string;
  email: string;
  emailVerified: boolean;
  auditInfo: {
    createdAt: string;
  };
}

/**
 * Error types from Hot Chocolate Mutation Conventions (discriminated union)
 */
interface ValidationError {
  __typename: 'ValidationError';
  message: string;
  field: string;
}

interface BusinessError {
  __typename: 'BusinessError';
  message: string;
  code: string;
}

interface ValueObjectError {
  __typename: 'ValueObjectError';
  message: string;
}

interface UnauthorizedError {
  __typename: 'UnauthorizedError';
  message: string;
}

interface InternalServerError {
  __typename: 'InternalServerError';
  message: string;
}

type MutationError =
  | ValidationError
  | BusinessError
  | ValueObjectError
  | UnauthorizedError
  | InternalServerError;

export interface CompleteZitadelLoginResponse {
  completeZitadelLogin: {
    authenticationResult: {
      user: CompleteZitadelLoginUserGQL;
      accessToken: string;
      expiresAt: string;
    } | null;
    errors: MutationError[];
  };
}
