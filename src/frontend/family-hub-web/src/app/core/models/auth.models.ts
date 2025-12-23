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

export interface CompleteZitadelLoginResponse {
  completeZitadelLogin: {
    authenticationResult?: {
      user: User;
      accessToken: string;
      expiresAt: string;
    };
    errors?: Array<{ message: string; code: string }>;
  };
}
