/**
 * Baseline Performance Test for Family Hub GraphQL API
 *
 * Purpose: Establish performance baselines with strict thresholds
 *
 * This test runs a small, constant load to verify the API meets
 * minimum performance requirements under light traffic.
 *
 * Usage:
 *   k6 run scenarios/baseline.js
 *   k6 run -e K6_ENV=ci scenarios/baseline.js
 *   k6 run -e GRAPHQL_URL=http://custom:5002/graphql scenarios/baseline.js
 */

import { sleep, group } from 'k6';
import { textSummary } from 'https://jslib.k6.io/k6-summary/0.1.0/index.js';
import { strictThresholds, withOperationThresholds } from '../config/thresholds.js';
import { getEnvironmentName } from '../config/environments.js';
import {
  executeHealthQuery,
  executeAuthUrlQuery,
  checkResponseTime,
  checkFieldValue,
} from '../helpers/graphql.js';

/**
 * Test configuration
 */
export const options = {
  // Constant load configuration
  vus: 10,
  duration: '1m',

  // Use strict thresholds for baseline testing
  thresholds: withOperationThresholds(strictThresholds),

  // Tags for filtering results
  tags: {
    scenario: 'baseline',
    environment: getEnvironmentName(),
  },

  // Summary configuration
  summaryTrendStats: ['avg', 'min', 'med', 'max', 'p(50)', 'p(90)', 'p(95)', 'p(99)'],
};

/**
 * Setup function - runs once before test starts
 * Can be used for authentication or data preparation
 */
export function setup() {
  console.log(`Starting baseline test against ${getEnvironmentName()} environment`);
  console.log(`Configuration: ${options.vus} VUs for ${options.duration}`);

  return {
    startTime: new Date().toISOString(),
  };
}

/**
 * Main test function - executed by each VU
 */
export default function (data) {
  // Group: Health Query Tests
  group('Health Query', function () {
    const { response, success } = executeHealthQuery();

    // Additional response time check (baseline should be very fast)
    checkResponseTime(response, 'Health Query', 100);

    // Verify expected field value
    checkFieldValue(response, 'health.status', 'Healthy', 'Health Query');
  });

  // Small pause between operations
  sleep(0.5);

  // Group: Auth URL Query Tests
  group('Auth URL Query', function () {
    const { response, success } = executeAuthUrlQuery();

    // Auth URL query may take slightly longer due to URL generation
    checkResponseTime(response, 'Auth URL Query', 150);
  });

  // Pause between iterations to simulate realistic traffic
  sleep(1);
}

/**
 * Teardown function - runs once after test completes
 */
export function teardown(data) {
  console.log(`Baseline test completed`);
  console.log(`Started at: ${data.startTime}`);
  console.log(`Ended at: ${new Date().toISOString()}`);
}

/**
 * Handle summary - generates test report
 * Outputs both console summary and JSON file
 */
export function handleSummary(data) {
  const timestamp = new Date().toISOString().replace(/[:.]/g, '-');
  const environment = getEnvironmentName();

  return {
    // Console output with human-readable summary
    stdout: textSummary(data, { indent: '  ', enableColors: true }),

    // JSON output for CI/CD and historical analysis
    [`results/baseline-${environment}-${timestamp}.json`]: JSON.stringify(data, null, 2),

    // Latest results file (overwrites previous)
    [`results/baseline-${environment}-latest.json`]: JSON.stringify(data, null, 2),
  };
}
