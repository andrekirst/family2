/**
 * Profile Models
 *
 * Domain types for user profile feature.
 * Maps to backend UserProfile aggregate and DTOs.
 */

/**
 * Visibility options for profile fields.
 * Maps to backend FieldVisibilityType enum.
 */
export type FieldVisibility = 'hidden' | 'family' | 'public';

/**
 * User profile preferences.
 * Maps to backend ProfilePreferencesDto.
 */
export interface ProfilePreferences {
  language: string; // e.g., "en", "de"
  timezone: string; // e.g., "UTC", "Europe/Berlin"
  dateFormat: string; // e.g., "yyyy-MM-dd"
}

/**
 * Profile field visibility settings.
 * Maps to backend ProfileFieldVisibilityDto.
 */
export interface FieldVisibilitySettings {
  birthdayVisibility: FieldVisibility;
  pronounsVisibility: FieldVisibility;
  preferencesVisibility: FieldVisibility;
}

/**
 * User profile domain model.
 * Maps to backend UserProfileDto from myProfile query.
 */
export interface UserProfile {
  id: string;
  userId: string;
  displayName: string;
  birthday?: string; // ISO date string (DateOnly)
  age?: number; // Calculated from birthday
  pronouns?: string;
  preferences: ProfilePreferences;
  fieldVisibility: FieldVisibilitySettings;
  createdAt: string;
  updatedAt: string;
}

/**
 * Input for updating user profile.
 * Maps to backend UpdateUserProfileInput.
 */
export interface UpdateUserProfileInput {
  displayName: string;
  birthday?: string;
  pronouns?: string;
  preferences?: Partial<ProfilePreferences>;
  fieldVisibility?: Partial<FieldVisibilitySettings>;
}

/**
 * Profile state for signal-based state management.
 */
export interface ProfileState {
  profile: UserProfile | null;
  isLoading: boolean;
  isSetupComplete: boolean;
  error: string | null;
  lastUpdated: string | null;
}

/**
 * GraphQL response type for myProfile query.
 */
export interface GetMyProfileResponse {
  myProfile: UserProfile | null;
}

/**
 * GraphQL response type for updateUserProfile mutation.
 */
export interface UpdateUserProfileResponse {
  updateUserProfile: {
    profileId: string;
    displayName: string;
    updatedAt: string;
    isNewProfile: boolean;
  };
}

/**
 * Available language options for preferences.
 */
export const LANGUAGE_OPTIONS = [
  { value: 'en', label: 'English' },
  { value: 'de', label: 'German' },
  { value: 'es', label: 'Spanish' },
  { value: 'fr', label: 'French' },
] as const;

/**
 * Available timezone options for preferences.
 */
export const TIMEZONE_OPTIONS = [
  { value: 'UTC', label: 'UTC' },
  { value: 'Europe/Berlin', label: 'Europe/Berlin (CET)' },
  { value: 'Europe/London', label: 'Europe/London (GMT)' },
  { value: 'America/New_York', label: 'America/New York (EST)' },
  { value: 'America/Los_Angeles', label: 'America/Los Angeles (PST)' },
  { value: 'Asia/Tokyo', label: 'Asia/Tokyo (JST)' },
  { value: 'Australia/Sydney', label: 'Australia/Sydney (AEST)' },
] as const;

/**
 * Available date format options.
 */
export const DATE_FORMAT_OPTIONS = [
  { value: 'yyyy-MM-dd', label: 'YYYY-MM-DD (2026-01-15)' },
  { value: 'dd/MM/yyyy', label: 'DD/MM/YYYY (15/01/2026)' },
  { value: 'MM/dd/yyyy', label: 'MM/DD/YYYY (01/15/2026)' },
  { value: 'dd.MM.yyyy', label: 'DD.MM.YYYY (15.01.2026)' },
] as const;

/**
 * Visibility options for UI.
 */
export const VISIBILITY_OPTIONS = [
  { value: 'hidden' as FieldVisibility, label: 'Hidden', description: 'Only you can see this' },
  { value: 'family' as FieldVisibility, label: 'Family', description: 'Visible to family members' },
  { value: 'public' as FieldVisibility, label: 'Public', description: 'Visible to everyone' },
] as const;
