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
  zitadelAuthUrl: {
    authorizationUrl: string;
    codeVerifier: string;
    state: string;
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
type ValidationError = {
  __typename: 'ValidationError';
  message: string;
  field: string;
};

type BusinessError = {
  __typename: 'BusinessError';
  message: string;
  code: string;
};

type ValueObjectError = {
  __typename: 'ValueObjectError';
  message: string;
};

type UnauthorizedError = {
  __typename: 'UnauthorizedError';
  message: string;
};

type InternalServerError = {
  __typename: 'InternalServerError';
  message: string;
};

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
