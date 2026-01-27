/**
 * User model representing the authenticated user.
 */
export interface User {
  id: string;
  email: string;
  emailVerified: boolean;
  familyId?: string;
  createdAt: Date;
}

/**
 * Authentication state for the application.
 */
export interface AuthState {
  isAuthenticated: boolean;
  user: User | null;
  accessToken: string | null;
  refreshToken: string | null;
  expiresAt: Date | null;
}

/**
 * Result of register mutation.
 */
export interface RegisterResult {
  userId: string;
  email: string;
  emailVerificationRequired: boolean;
  accessToken?: string;
  refreshToken?: string;
}

/**
 * Result of login mutation.
 */
export interface LoginResult {
  userId: string;
  email: string;
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
  familyId?: string;
  emailVerified: boolean;
}

/**
 * Result of token refresh mutation.
 */
export interface RefreshTokenResult {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
}

/**
 * Result of password validation query.
 */
export interface PasswordValidationResult {
  isValid: boolean;
  score: number;
  strength: string;
  errors: string[];
  suggestions: string[];
}

/**
 * Error structure from GraphQL payloads.
 */
export interface PayloadError {
  code: string;
  message: string;
}

/**
 * Generic GraphQL mutation response with errors.
 */
export interface MutationResponse<T> {
  data?: T;
  errors?: PayloadError[];
}

// ============================================
// GraphQL Response Types
// ============================================

export interface RegisterMutationResponse {
  register: {
    userId?: string;
    email?: string;
    emailVerificationRequired?: boolean;
    accessToken?: string;
    refreshToken?: string;
    errors?: PayloadError[];
  };
}

export interface LoginMutationResponse {
  login: {
    userId?: string;
    email?: string;
    accessToken?: string;
    refreshToken?: string;
    expiresIn?: number;
    familyId?: string;
    emailVerified?: boolean;
    errors?: PayloadError[];
  };
}

export interface LogoutMutationResponse {
  logout: {
    success?: boolean;
    revokedSessionCount?: number;
    errors?: PayloadError[];
  };
}

export interface RefreshTokenMutationResponse {
  refreshToken: {
    accessToken?: string;
    refreshToken?: string;
    expiresIn?: number;
    errors?: PayloadError[];
  };
}

export interface RequestPasswordResetMutationResponse {
  requestPasswordReset: {
    success?: boolean;
    message?: string;
    errors?: PayloadError[];
  };
}

export interface ResetPasswordMutationResponse {
  resetPassword: {
    success?: boolean;
    errors?: PayloadError[];
  };
}

export interface ResetPasswordWithCodeMutationResponse {
  resetPasswordWithCode: {
    success?: boolean;
    errors?: PayloadError[];
  };
}

export interface VerifyEmailMutationResponse {
  verifyEmail: {
    success?: boolean;
    message?: string;
    errors?: PayloadError[];
  };
}

export interface ResendVerificationEmailMutationResponse {
  resendVerificationEmail: {
    success?: boolean;
    message?: string;
    errors?: PayloadError[];
  };
}

export interface ChangePasswordMutationResponse {
  changePassword: {
    success?: boolean;
    errors?: PayloadError[];
  };
}

export interface ValidatePasswordQueryResponse {
  auth: {
    validatePassword: {
      isValid: boolean;
      score: number;
      strength: string;
      errors: string[];
      suggestions: string[];
    };
  };
}
