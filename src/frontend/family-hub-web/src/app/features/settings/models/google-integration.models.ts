export interface LinkedAccount {
  googleAccountId: string;
  googleEmail: string;
  status: string;
  grantedScopes: string;
  lastSyncAt: string | null;
  createdAt: string;
}

export interface GoogleCalendarSyncStatus {
  isLinked: boolean;
  lastSyncAt: string | null;
  hasCalendarScope: boolean;
  status: string;
  errorMessage: string | null;
}

export interface RefreshTokenResult {
  success: boolean;
  newExpiresAt: string | null;
}
