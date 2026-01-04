/**
 * TypeScript Mirrors of C# Vogen Value Objects
 *
 * These classes mirror the validation logic from the backend's Vogen value objects
 * to provide type safety and consistent test data generation in E2E tests.
 *
 * Source files:
 * - Email: src/api/FamilyHub.SharedKernel/Domain/ValueObjects/Email.cs
 * - UserId: src/api/FamilyHub.SharedKernel/Domain/ValueObjects/UserId.cs
 * - FamilyId: src/api/FamilyHub.SharedKernel/Domain/ValueObjects/FamilyId.cs
 * - FamilyName: src/api/FamilyHub.SharedKernel/Domain/ValueObjects/FamilyName.cs
 */

/**
 * Email Value Object
 *
 * Validation rules (from Email.cs):
 * - Cannot be null, empty, or whitespace
 * - Maximum length: 320 characters (RFC 5321)
 * - Must match email regex pattern
 * - Normalized: trimmed and lowercase
 */
export class Email {
  private static readonly EMAIL_REGEX = /^[^@\s]+@[^@\s]+\.[^@\s]+$/;
  private static readonly MAX_LENGTH = 320;

  private constructor(public readonly value: string) {}

  /**
   * Creates an Email instance from a string value
   * @throws Error if validation fails
   */
  static from(value: string): Email {
    // Normalize input (trim and lowercase)
    const normalized = value.trim().toLowerCase();

    // Validation: cannot be empty
    if (!normalized) {
      throw new Error('Email cannot be empty.');
    }

    // Validation: max length
    if (normalized.length > Email.MAX_LENGTH) {
      throw new Error('Email cannot exceed 320 characters.');
    }

    // Validation: email format
    if (!Email.EMAIL_REGEX.test(normalized)) {
      throw new Error('Email format is invalid.');
    }

    return new Email(normalized);
  }

  /**
   * Returns the string representation of the email
   */
  toString(): string {
    return this.value;
  }
}

/**
 * UserId Value Object
 *
 * Represents a strongly-typed User identifier (GUID).
 * No validation - allows any GUID including empty for EF Core materialization.
 */
export class UserId {
  private constructor(public readonly value: string) {}

  /**
   * Creates a UserId from an existing GUID string
   */
  static from(value: string): UserId {
    return new UserId(value);
  }

  /**
   * Creates a new UserId with a random GUID
   */
  static new(): UserId {
    return new UserId(crypto.randomUUID());
  }

  /**
   * Returns the string representation of the user ID
   */
  toString(): string {
    return this.value;
  }
}

/**
 * FamilyId Value Object
 *
 * Represents a strongly-typed Family identifier (GUID).
 * No validation - allows any GUID including empty for EF Core materialization.
 */
export class FamilyId {
  private constructor(public readonly value: string) {}

  /**
   * Creates a FamilyId from an existing GUID string
   */
  static from(value: string): FamilyId {
    return new FamilyId(value);
  }

  /**
   * Creates a new FamilyId with a random GUID
   */
  static new(): FamilyId {
    return new FamilyId(crypto.randomUUID());
  }

  /**
   * Returns the string representation of the family ID
   */
  toString(): string {
    return this.value;
  }
}

/**
 * FamilyName Value Object
 *
 * Validation rules (from FamilyName.cs):
 * - Cannot be null, empty, or whitespace
 * - Maximum length: 100 characters
 * - Normalized: trimmed
 */
export class FamilyName {
  private static readonly MAX_LENGTH = 100;

  private constructor(public readonly value: string) {}

  /**
   * Creates a FamilyName instance from a string value
   * @throws Error if validation fails
   */
  static from(value: string): FamilyName {
    // Normalize input (trim whitespace)
    const normalized = value.trim();

    // Validation: cannot be empty
    if (!normalized) {
      throw new Error('Family name cannot be empty.');
    }

    // Validation: max length
    if (normalized.length > FamilyName.MAX_LENGTH) {
      throw new Error(`Family name cannot exceed ${FamilyName.MAX_LENGTH} characters.`);
    }

    return new FamilyName(normalized);
  }

  /**
   * Returns the string representation of the family name
   */
  toString(): string {
    return this.value;
  }
}
