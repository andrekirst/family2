/**
 * GraphQL Helper Functions for k6 Performance Tests
 *
 * Provides utilities for executing GraphQL requests and validating responses.
 */

import http from 'k6/http';
import { check } from 'k6';
import { getGraphqlUrl, getTimeout } from '../config/environments.js';

/**
 * Execute a GraphQL request
 *
 * @param {string} query - GraphQL query or mutation string
 * @param {Object} variables - Variables to pass with the query (optional)
 * @param {string} authToken - Bearer token for authentication (optional)
 * @param {Object} additionalParams - Additional k6 request parameters (optional)
 * @returns {Object} k6 HTTP response object
 *
 * @example
 * // Simple query
 * const response = graphqlRequest(`
 *   query Health {
 *     health { status timestamp service }
 *   }
 * `);
 *
 * // Query with variables and auth
 * const response = graphqlRequest(
 *   `query GetUser($id: ID!) { user(id: $id) { name } }`,
 *   { id: '123' },
 *   'eyJhbGc...'
 * );
 */
export function graphqlRequest(query, variables = null, authToken = null, additionalParams = {}) {
  const url = getGraphqlUrl();

  // Build request payload
  const payload = JSON.stringify({
    query: query,
    variables: variables,
  });

  // Build headers
  const headers = {
    'Content-Type': 'application/json',
    Accept: 'application/json',
  };

  // Add authorization header if token provided
  if (authToken) {
    headers['Authorization'] = `Bearer ${authToken}`;
  }

  // Merge with additional parameters
  const params = {
    headers: headers,
    timeout: getTimeout(),
    ...additionalParams,
  };

  // Execute POST request
  return http.post(url, payload, params);
}

/**
 * Check and validate a GraphQL response
 *
 * Performs standard validation checks:
 * - HTTP status is 200
 * - Response body exists
 * - No GraphQL errors in response
 * - Response contains data field
 *
 * @param {Object} response - k6 HTTP response object
 * @param {string} name - Name for the check group (for reporting)
 * @returns {boolean} True if all checks pass
 *
 * @example
 * const response = graphqlRequest(healthQuery);
 * const success = checkGraphQLResponse(response, 'Health Query');
 */
export function checkGraphQLResponse(response, name) {
  const checkName = name || 'GraphQL Response';

  // Parse response body once
  let jsonBody = null;
  try {
    jsonBody = response.json();
  } catch (e) {
    // JSON parsing failed, will be caught by checks below
  }

  return check(response, {
    [`${checkName}: status is 200`]: (r) => r.status === 200,
    [`${checkName}: has body`]: (r) => r.body && r.body.length > 0,
    [`${checkName}: no errors`]: () => {
      if (!jsonBody) return false;
      return !jsonBody.errors || jsonBody.errors.length === 0;
    },
    [`${checkName}: has data`]: () => {
      if (!jsonBody) return false;
      return jsonBody.data !== null && jsonBody.data !== undefined;
    },
  });
}

/**
 * Execute GraphQL request with integrated validation
 *
 * Combines request execution and response validation in one call.
 *
 * @param {string} query - GraphQL query or mutation string
 * @param {string} name - Name for the check group
 * @param {Object} variables - Variables to pass with the query (optional)
 * @param {string} authToken - Bearer token for authentication (optional)
 * @param {Object} additionalParams - Additional k6 request parameters (optional)
 * @returns {Object} Object containing response and validation result
 *
 * @example
 * const { response, success } = executeAndCheck(
 *   healthQuery,
 *   'Health Query'
 * );
 */
export function executeAndCheck(
  query,
  name,
  variables = null,
  authToken = null,
  additionalParams = {}
) {
  const response = graphqlRequest(query, variables, authToken, additionalParams);
  const success = checkGraphQLResponse(response, name);

  return {
    response: response,
    success: success,
  };
}

/**
 * Check response time thresholds for a specific request
 *
 * @param {Object} response - k6 HTTP response object
 * @param {string} name - Name for the check
 * @param {number} maxDuration - Maximum acceptable duration in milliseconds
 * @returns {boolean} True if response time is within threshold
 */
export function checkResponseTime(response, name, maxDuration) {
  return check(response, {
    [`${name}: response time < ${maxDuration}ms`]: (r) => r.timings.duration < maxDuration,
  });
}

/**
 * Check specific field value in GraphQL response
 *
 * @param {Object} response - k6 HTTP response object
 * @param {string} fieldPath - Dot-notation path to the field (e.g., 'health.status')
 * @param {*} expectedValue - Expected value for the field
 * @param {string} name - Name for the check
 * @returns {boolean} True if field matches expected value
 */
export function checkFieldValue(response, fieldPath, expectedValue, name) {
  return check(response, {
    [`${name}: ${fieldPath} equals ${expectedValue}`]: (r) => {
      try {
        const json = r.json();
        const value = fieldPath.split('.').reduce((obj, key) => obj && obj[key], json.data);
        return value === expectedValue;
      } catch (e) {
        return false;
      }
    },
  });
}

/**
 * Pre-defined GraphQL queries for common operations
 */
export const queries = {
  /**
   * Health check query
   */
  health: `
    query Health {
      health {
        status
        timestamp
        service
      }
    }
  `,

  /**
   * Get authentication URL query
   */
  getAuthUrl: `
    query GetAuthUrl {
      auth {
        getAuthUrl
      }
    }
  `,
};

/**
 * Execute health query with validation
 *
 * @param {Object} additionalParams - Additional k6 request parameters
 * @returns {Object} Object containing response and success status
 */
export function executeHealthQuery(additionalParams = {}) {
  return executeAndCheck(queries.health, 'Health Query', null, null, {
    tags: { name: 'health_query' },
    ...additionalParams,
  });
}

/**
 * Execute auth URL query with validation
 *
 * @param {Object} additionalParams - Additional k6 request parameters
 * @returns {Object} Object containing response and success status
 */
export function executeAuthUrlQuery(additionalParams = {}) {
  return executeAndCheck(queries.getAuthUrl, 'Auth URL Query', null, null, {
    tags: { name: 'auth_url_query' },
    ...additionalParams,
  });
}
