/**
 * Load Performance Test for Family Hub GraphQL API
 *
 * Purpose: Verify API performance under normal to high traffic load
 *
 * This test simulates realistic traffic patterns with gradual ramp-up
 * and ramp-down phases to measure how the system handles sustained load.
 *
 * Load Profile:
 *   - Ramp up: 0 -> 50 VUs over 1 minute
 *   - Sustain: 50 VUs for 3 minutes
 *   - Ramp up: 50 -> 100 VUs over 1 minute
 *   - Sustain: 100 VUs for 3 minutes
 *   - Ramp down: 100 -> 0 VUs over 1 minute
 *   - Total: ~10 minutes
 *
 * Usage:
 *   k6 run scenarios/load.js
 *   k6 run -e K6_ENV=staging scenarios/load.js
 *   k6 run --out influxdb=http://localhost:8086/k6 scenarios/load.js
 */

import { sleep, group } from 'k6';
import { Counter, Trend } from 'k6/metrics';
import { textSummary } from 'https://jslib.k6.io/k6-summary/0.1.0/index.js';
import { defaultThresholds, withOperationThresholds } from '../config/thresholds.js';
import { getEnvironmentName } from '../config/environments.js';
import { executeHealthQuery, executeAuthUrlQuery, checkResponseTime } from '../helpers/graphql.js';

// Custom metrics for detailed tracking
const healthQueryDuration = new Trend('health_query_duration', true);
const authQueryDuration = new Trend('auth_query_duration', true);
const failedRequests = new Counter('failed_requests');

/**
 * Test configuration with staged load pattern
 */
export const options = {
  // Staged load pattern for gradual ramp-up/down
  stages: [
    { duration: '1m', target: 50 }, // Ramp up to 50 VUs
    { duration: '3m', target: 50 }, // Stay at 50 VUs
    { duration: '1m', target: 100 }, // Ramp up to 100 VUs
    { duration: '3m', target: 100 }, // Stay at 100 VUs
    { duration: '1m', target: 0 }, // Ramp down to 0
  ],

  // Use default thresholds for load testing
  thresholds: {
    ...withOperationThresholds(defaultThresholds),
    // Custom metric thresholds
    health_query_duration: ['p(95)<200', 'p(99)<500'],
    auth_query_duration: ['p(95)<300', 'p(99)<600'],
    failed_requests: ['count<100'],
  },

  // Tags for filtering results
  tags: {
    scenario: 'load',
    environment: getEnvironmentName(),
  },

  // Summary configuration
  summaryTrendStats: ['avg', 'min', 'med', 'max', 'p(50)', 'p(90)', 'p(95)', 'p(99)'],
};

/**
 * Setup function - runs once before test starts
 */
export function setup() {
  console.log(`Starting load test against ${getEnvironmentName()} environment`);
  console.log('Load profile:');
  console.log('  - Ramp up: 0 -> 50 VUs (1m)');
  console.log('  - Sustain: 50 VUs (3m)');
  console.log('  - Ramp up: 50 -> 100 VUs (1m)');
  console.log('  - Sustain: 100 VUs (3m)');
  console.log('  - Ramp down: 100 -> 0 VUs (1m)');
  console.log('  - Total duration: ~10 minutes');

  return {
    startTime: new Date().toISOString(),
  };
}

/**
 * Main test function - executed by each VU
 */
export default function (data) {
  // Health Query - Primary endpoint under test
  group('Health Query', function () {
    const { response, success } = executeHealthQuery();

    // Track duration in custom metric
    healthQueryDuration.add(response.timings.duration);

    // Check response time threshold
    const withinThreshold = checkResponseTime(response, 'Health Query', 200);

    // Track failures
    if (!success || !withinThreshold) {
      failedRequests.add(1);
    }
  });

  // Small pause between different query types
  sleep(0.3);

  // Auth URL Query - Secondary endpoint
  group('Auth URL Query', function () {
    const { response, success } = executeAuthUrlQuery();

    // Track duration in custom metric
    authQueryDuration.add(response.timings.duration);

    // Check response time threshold
    const withinThreshold = checkResponseTime(response, 'Auth URL Query', 300);

    // Track failures
    if (!success || !withinThreshold) {
      failedRequests.add(1);
    }
  });

  // Pause between iterations - simulate think time
  // Randomize slightly to avoid thundering herd
  sleep(Math.random() * 0.5 + 0.5); // 0.5-1.0 seconds
}

/**
 * Teardown function - runs once after test completes
 */
export function teardown(data) {
  console.log(`\nLoad test completed`);
  console.log(`Started at: ${data.startTime}`);
  console.log(`Ended at: ${new Date().toISOString()}`);
}

/**
 * Handle summary - generates test report
 */
export function handleSummary(data) {
  const timestamp = new Date().toISOString().replace(/[:.]/g, '-');
  const environment = getEnvironmentName();

  // Calculate pass/fail based on thresholds
  const thresholdsPassed = Object.values(data.metrics)
    .filter((m) => m.thresholds)
    .every((m) => Object.values(m.thresholds).every((t) => t.ok));

  console.log(`\nOverall Result: ${thresholdsPassed ? 'PASSED' : 'FAILED'}`);

  return {
    // Console output with human-readable summary
    stdout: textSummary(data, { indent: '  ', enableColors: true }),

    // JSON output for CI/CD and historical analysis
    [`results/load-${environment}-${timestamp}.json`]: JSON.stringify(data, null, 2),

    // Latest results file (overwrites previous)
    [`results/load-${environment}-latest.json`]: JSON.stringify(data, null, 2),
  };
}
