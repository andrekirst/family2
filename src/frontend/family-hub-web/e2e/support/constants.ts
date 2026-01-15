/**
 * Test Constants for E2E Tests
 *
 * Eliminates magic strings and provides centralized configuration
 * for URLs, storage keys, selectors, and timeouts.
 */

/**
 * URL Configuration
 */
export const URLS = {
  /** Frontend base URL (Angular dev server) */
  BASE: 'http://localhost:4200',

  /** Backend API base URL */
  API: 'http://localhost:5002',

  /** GraphQL endpoint */
  GRAPHQL: 'http://localhost:5002/graphql',

  /** Dashboard page */
  DASHBOARD: '/dashboard',

  /** Family creation wizard */
  FAMILY_CREATE: '/family/create',

  /** Login page */
  LOGIN: '/login',
} as const;

/**
 * LocalStorage Keys
 * Must match keys used in Angular AuthService
 */
export const STORAGE_KEYS = {
  /** OAuth access token key */
  ACCESS_TOKEN: 'family_hub_access_token',

  /** Token expiration timestamp key */
  TOKEN_EXPIRES: 'family_hub_token_expires',
} as const;

/**
 * Test Data Configuration
 */
export const TEST_DATA = {
  /** Mock JWT token for testing (matches Cypress mock) */
  MOCK_ACCESS_TOKEN: 'mock-jwt-token-for-testing',

  /** Token expiry duration in hours */
  TOKEN_EXPIRY_HOURS: 1,
} as const;

/**
 * Timeout Configuration (in milliseconds)
 */
export const TIMEOUTS = {
  /** GraphQL request timeout */
  GRAPHQL: 10000,

  /** Page navigation timeout */
  NAVIGATION: 30000,

  /** Testcontainer startup timeout */
  TESTCONTAINER_START: 120000,

  /** Service health check interval */
  HEALTH_CHECK_INTERVAL: 1000,

  /** RabbitMQ message consumption timeout */
  RABBITMQ_CONSUME: 1000,
} as const;

/**
 * Selector Patterns
 *
 * IMPORTANT: These selectors should be migrated to data-testid attributes
 * for better stability and maintainability.
 *
 * TODO (Phase 3): Add data-testid attributes to Angular components
 * and update these selectors to use data-testid.
 */
export const SELECTORS = {
  /** Family name input field (aria-label selector) */
  FAMILY_NAME_INPUT: 'input[aria-label="Family name"]',

  /** Create family button */
  CREATE_FAMILY_BUTTON: 'button:has-text("Create Family")',

  /** Wizard title heading */
  WIZARD_TITLE: 'h1:has-text("Create Your Family")',

  /** Progress indicator text */
  PROGRESS_INDICATOR: 'text="Step 1 of 1"',

  /** Alert role for error/success messages */
  ALERT_ROLE: '[role="alert"]',

  /** Loading overlay */
  LOADING_OVERLAY: '[data-testid="loading-overlay"]',

  /** Character counter */
  CHARACTER_COUNTER: '[data-testid="character-counter"]',
} as const;

/**
 * RabbitMQ Configuration
 */
export const RABBITMQ = {
  /** Connection URL */
  URL: 'amqp://familyhub:Dev123!@localhost:5672',

  /** Main exchange for production events */
  EXCHANGE: 'familyhub',

  /** Test exchange for E2E test events */
  TEST_EXCHANGE: 'familyhub.test',

  /** Exchange type */
  EXCHANGE_TYPE: 'topic',
} as const;

/**
 * GraphQL Operation Names
 *
 * Centralized list of GraphQL operations for interception
 */
export const GRAPHQL_OPERATIONS = {
  /** Get current user's family */
  GET_CURRENT_FAMILY: 'GetCurrentFamily',

  /** Create a new family */
  CREATE_FAMILY: 'CreateFamily',

  /** Get user families */
  GET_USER_FAMILIES: 'GetUserFamilies',
} as const;

/**
 * GraphQL Schema Validation Configuration
 *
 * Constants for schema introspection and validation tests.
 * @see Issue #74 - E2E Tests - GraphQL Schema Validation
 */
export const SCHEMA = {
  /** Schema introspection timeout (ms) */
  INTROSPECTION_TIMEOUT: 5000,

  /** Maximum acceptable schema size (KB) */
  MAX_SCHEMA_SIZE_KB: 500,

  /** Performance warning threshold (ms) */
  PERFORMANCE_WARNING_MS: 2000,

  /**
   * Expected Auth module types
   * These types should always be present in the schema
   */
  AUTH_MODULE_TYPES: [
    'User',
    'UserRoleType',
    'CreateFamilyPayload',
    'AcceptInvitationPayload',
  ] as const,

  /**
   * Expected Family module types
   * These types should always be present in the schema
   */
  FAMILY_MODULE_TYPES: [
    'Family',
    'PendingInvitationType',
    'CreateFamilyInput',
    'AcceptInvitationInput',
  ] as const,

  /**
   * Expected error types (union members)
   * These follow the MutationConventions pattern from Hot Chocolate
   */
  ERROR_TYPES: [
    'ValidationError',
    'BusinessError',
    'ValueObjectError',
    'UnauthorizedError',
  ] as const,

  /**
   * Critical query fields that must exist
   * Removing these would be a breaking change
   */
  CRITICAL_QUERY_FIELDS: ['family', 'familyMembers', 'invitations'] as const,

  /**
   * Critical mutation fields that must exist
   * Removing these would be a breaking change
   */
  CRITICAL_MUTATION_FIELDS: ['createFamily', 'acceptInvitation'] as const,
} as const;

/**
 * WCAG Accessibility Rules
 *
 * Configuration for axe-core accessibility testing
 */
export const A11Y_RULES = {
  /** Color contrast rule */
  'color-contrast': { enabled: true },

  /** Valid ARIA attributes */
  'valid-aria-attr': { enabled: true },

  /** Required ARIA attributes */
  'aria-required-attr': { enabled: true },

  /** Valid ARIA attribute values */
  'aria-valid-attr-value': { enabled: true },

  /** Label rule */
  label: { enabled: true },

  /** Button name rule */
  'button-name': { enabled: true },

  /** Region rule */
  region: { enabled: true },
} as const;
