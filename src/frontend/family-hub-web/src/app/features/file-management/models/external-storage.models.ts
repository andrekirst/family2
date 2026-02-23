export interface ExternalConnectionDto {
  id: string;
  familyId: string;
  providerType: string;
  displayName: string;
  status: string;
  isTokenExpired: boolean;
  tokenExpiresAt: string | null;
  connectedBy: string;
  connectedAt: string;
}

export interface ConnectExternalStorageInput {
  providerType: string;
  displayName: string;
  encryptedAccessToken: string;
  encryptedRefreshToken?: string;
  tokenExpiresAt?: string;
}
