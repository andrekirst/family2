/**
 * Environment Configurations for Family Hub Performance Tests
 *
 * Defines GraphQL endpoints and settings for different environments.
 * Use K6_ENV environment variable to select the target environment.
 */

/**
 * Environment definitions with their respective configurations
 */
export const environments = {
  /**
   * Local development environment
   * Used for development and debugging tests locally
   */
  local: {
    name: 'local',
    graphqlUrl: 'http://localhost:5002/graphql',
    timeout: '30s',
    // No authentication required for health/auth endpoints
    requiresAuth: false,
  },

  /**
   * CI/CD environment
   * Used in automated pipelines (GitHub Actions, etc.)
   */
  ci: {
    name: 'ci',
    graphqlUrl: 'http://localhost:5002/graphql',
    timeout: '60s',
    requiresAuth: false,
  },

  /**
   * Staging environment
   * Pre-production testing environment
   */
  staging: {
    name: 'staging',
    graphqlUrl: 'http://staging-api.familyhub.local/graphql',
    timeout: '30s',
    requiresAuth: true,
  },

  /**
   * Production environment
   * Live production environment (use with caution)
   */
  production: {
    name: 'production',
    graphqlUrl: 'https://api.familyhub.com/graphql',
    timeout: '30s',
    requiresAuth: true,
  },
};

/**
 * Get the GraphQL URL for the current environment
 *
 * Priority:
 * 1. GRAPHQL_URL environment variable (explicit override)
 * 2. K6_ENV environment variable (selects from environments)
 * 3. Falls back to 'local' environment
 *
 * @returns {string} The GraphQL endpoint URL
 *
 * @example
 * // Run with custom URL:
 * // k6 run -e GRAPHQL_URL=http://custom:5002/graphql scenario.js
 *
 * // Run with environment selection:
 * // k6 run -e K6_ENV=staging scenario.js
 */
export function getGraphqlUrl() {
  // Check for explicit URL override first
  if (__ENV.GRAPHQL_URL) {
    return __ENV.GRAPHQL_URL;
  }

  // Get environment name from K6_ENV, default to 'local'
  const envName = __ENV.K6_ENV || 'local';

  // Get environment configuration
  const env = environments[envName];

  if (!env) {
    console.warn(`Unknown environment '${envName}', falling back to 'local'`);
    return environments.local.graphqlUrl;
  }

  return env.graphqlUrl;
}

/**
 * Get the full environment configuration
 *
 * @returns {Object} Environment configuration object
 */
export function getEnvironment() {
  const envName = __ENV.K6_ENV || 'local';
  return environments[envName] || environments.local;
}

/**
 * Get the request timeout for the current environment
 *
 * @returns {string} Timeout string (e.g., '30s')
 */
export function getTimeout() {
  const env = getEnvironment();
  return env.timeout;
}

/**
 * Check if current environment requires authentication
 *
 * @returns {boolean} True if authentication is required
 */
export function requiresAuthentication() {
  const env = getEnvironment();
  return env.requiresAuth;
}

/**
 * Get environment name for tagging and reporting
 *
 * @returns {string} Environment name
 */
export function getEnvironmentName() {
  return __ENV.K6_ENV || 'local';
}

/**
 * Test users for authenticated DataLoader benchmarks
 *
 * These users must exist in the database (created by seed script).
 * Use these with X-Test-User-Id header in Test environment.
 *
 * Prerequisites:
 *   - API running with ASPNETCORE_ENVIRONMENT=Test
 *   - Test data seeded: npm run seed:dataloader
 *
 * Usage in k6:
 *   const user = getTestUsers()[0];
 *   const response = graphqlRequest(query, null, null, {
 *     headers: { 'X-Test-User-Id': user.id }
 *   });
 */
const testUsers = {
  local: [
    { id: '00000000-0000-0000-0000-000000000001', email: 'testuser1@test.local' },
    { id: '00000000-0000-0000-0000-000000000002', email: 'testuser2@test.local' },
    { id: '00000000-0000-0000-0000-000000000003', email: 'testuser3@test.local' },
    { id: '00000000-0000-0000-0000-000000000004', email: 'testuser4@test.local' },
    { id: '00000000-0000-0000-0000-000000000005', email: 'testuser5@test.local' },
  ],
  ci: [
    { id: '00000000-0000-0000-0000-000000000001', email: 'testuser1@test.local' },
    { id: '00000000-0000-0000-0000-000000000002', email: 'testuser2@test.local' },
    { id: '00000000-0000-0000-0000-000000000003', email: 'testuser3@test.local' },
    { id: '00000000-0000-0000-0000-000000000004', email: 'testuser4@test.local' },
    { id: '00000000-0000-0000-0000-000000000005', email: 'testuser5@test.local' },
  ],
  // Staging and production don't have test users (require real authentication)
  staging: [],
  production: [],
};

/**
 * Get test users for current environment
 *
 * @returns {Array<{id: string, email: string}>} Test users
 */
export function getTestUsers() {
  const envName = __ENV.K6_ENV || 'local';
  return testUsers[envName] || testUsers.local;
}
