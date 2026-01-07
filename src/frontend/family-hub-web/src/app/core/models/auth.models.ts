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

export interface CompleteZitadelLoginResponse {
  completeZitadelLogin: {
    authenticationResult?: {
      user: CompleteZitadelLoginUserGQL;
      accessToken: string;
      expiresAt: string;
    };
    errors?: { message: string; code: string }[];
  };
}
