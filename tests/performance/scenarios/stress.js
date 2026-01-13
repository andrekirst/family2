/**
 * Stress Performance Test for Family Hub GraphQL API
 *
 * Purpose: Find breaking points and verify graceful degradation
 *
 * This test pushes the system beyond normal capacity to identify
 * performance limits and verify the system degrades gracefully
 * under extreme load.
 *
 * Stress Profile:
 *   - Ramp up: 10 -> 200 VUs over 30 seconds (aggressive)
 *   - Sustain: 200 VUs for 1 minute (peak stress)
 *   - Ramp down: 200 -> 10 VUs over 30 seconds
 *   - Recovery: 10 VUs for 1 minute (verify recovery)
 *   - Total: ~3 minutes
 *
 * Note: This test uses relaxed thresholds as some degradation is expected.
 *
 * Usage:
 *   k6 run scenarios/stress.js
 *   k6 run -e K6_ENV=staging scenarios/stress.js
 *   k6 run --out csv=results/stress-data.csv scenarios/stress.js
 */

import { group } from 'k6';
import { Counter, Trend, Rate } from 'k6/metrics';
import { textSummary } from 'https://jslib.k6.io/k6-summary/0.1.0/index.js';
import { stressThresholds } from '../config/thresholds.js';
import { getEnvironmentName } from '../config/environments.js';
import { graphqlRequest, checkGraphQLResponse, queries } from '../helpers/graphql.js';

// Custom metrics for stress analysis
const requestDuration = new Trend('stress_request_duration', true);
const errorRate = new Rate('stress_error_rate');
const timeoutCount = new Counter('timeout_count');
const successCount = new Counter('success_count');
const errorCount = new Counter('error_count');

/**
 * Test configuration with aggressive stress pattern
 */
export const options = {
  // Aggressive stress pattern
  stages: [
    { duration: '30s', target: 200 }, // Rapid ramp up to 200 VUs
    { duration: '1m', target: 200 }, // Sustain peak load
    { duration: '30s', target: 10 }, // Ramp down
    { duration: '1m', target: 10 }, // Recovery period
  ],

  // Use relaxed stress thresholds
  thresholds: {
    ...stressThresholds,
    // Additional stress-specific thresholds
    stress_request_duration: ['p(95)<2000'], // More lenient
    stress_error_rate: ['rate<0.1'], // Up to 10% errors acceptable
    timeout_count: ['count<50'], // Some timeouts expected
  },

  // Tags for filtering results
  tags: {
    scenario: 'stress',
    environment: getEnvironmentName(),
  },

  // Shorter timeout to identify issues faster
  httpDebug: 'full', // Enable for debugging (remove in production)

  // Summary configuration
  summaryTrendStats: ['avg', 'min', 'med', 'max', 'p(50)', 'p(90)', 'p(95)', 'p(99)', 'count'],
};

/**
 * Setup function - runs once before test starts
 */
export function setup() {
  console.log(`Starting STRESS test against ${getEnvironmentName()} environment`);
  console.log('WARNING: This test will push the system beyond normal capacity!');
  console.log('');
  console.log('Stress profile:');
  console.log('  - Ramp up: 10 -> 200 VUs (30s) - AGGRESSIVE');
  console.log('  - Peak: 200 VUs (1m) - MAXIMUM STRESS');
  console.log('  - Ramp down: 200 -> 10 VUs (30s)');
  console.log('  - Recovery: 10 VUs (1m) - Verify system recovers');
  console.log('  - Total duration: ~3 minutes');
  console.log('');

  return {
    startTime: new Date().toISOString(),
    peakVUs: 200,
  };
}

/**
 * Main test function - high frequency requests, no sleep
 *
 * Unlike load tests, stress tests fire requests as fast as possible
 * to maximize pressure on the system.
 */
export default function (data) {
  // Execute health query at maximum speed (no sleep between iterations)
  group('Stress: Health Query', function () {
    const startTime = Date.now();

    // Execute request with short timeout for stress testing
    const response = graphqlRequest(queries.health, null, null, {
      tags: { name: 'stress_health_query' },
      timeout: '10s', // Shorter timeout for stress
    });

    const duration = Date.now() - startTime;

    // Track metrics
    requestDuration.add(duration);

    // Check for timeout (connection issues under stress)
    if (response.status === 0) {
      timeoutCount.add(1);
      errorCount.add(1);
      errorRate.add(true);
      return;
    }

    // Validate response
    const success = checkGraphQLResponse(response, 'Stress Health');

    if (success) {
      successCount.add(1);
      errorRate.add(false);
    } else {
      errorCount.add(1);
      errorRate.add(true);
    }
  });

  // NO SLEEP - maximum pressure on the system
  // This is intentional for stress testing
}

/**
 * Teardown function - runs once after test completes
 */
export function teardown(data) {
  console.log(`\n${'='.repeat(60)}`);
  console.log(`STRESS TEST COMPLETED`);
  console.log(`${'='.repeat(60)}`);
  console.log(`Started at: ${data.startTime}`);
  console.log(`Ended at: ${new Date().toISOString()}`);
  console.log(`Peak VUs: ${data.peakVUs}`);
  console.log(`${'='.repeat(60)}\n`);
}

/**
 * Handle summary - generates test report with stress analysis
 */
export function handleSummary(data) {
  const timestamp = new Date().toISOString().replace(/[:.]/g, '-');
  const environment = getEnvironmentName();

  // Extract key metrics for summary
  const httpReqs = data.metrics.http_reqs;
  const duration = data.metrics.stress_request_duration;
  const errors = data.metrics.stress_error_rate;

  // Build stress analysis summary
  let stressAnalysis = '\n\n=== STRESS TEST ANALYSIS ===\n\n';

  if (httpReqs) {
    stressAnalysis += `Total Requests: ${httpReqs.values.count}\n`;
    stressAnalysis += `Requests/sec: ${httpReqs.values.rate?.toFixed(2) || 'N/A'}\n`;
  }

  if (duration) {
    stressAnalysis += `\nResponse Times:\n`;
    stressAnalysis += `  - p50: ${duration.values['p(50)']?.toFixed(2) || 'N/A'}ms\n`;
    stressAnalysis += `  - p95: ${duration.values['p(95)']?.toFixed(2) || 'N/A'}ms\n`;
    stressAnalysis += `  - p99: ${duration.values['p(99)']?.toFixed(2) || 'N/A'}ms\n`;
    stressAnalysis += `  - max: ${duration.values.max?.toFixed(2) || 'N/A'}ms\n`;
  }

  if (errors) {
    const errorPct = (errors.values.rate * 100).toFixed(2);
    stressAnalysis += `\nError Rate: ${errorPct}%\n`;

    // Provide interpretation
    if (errors.values.rate < 0.01) {
      stressAnalysis += 'Assessment: EXCELLENT - System handled stress with minimal errors\n';
    } else if (errors.values.rate < 0.05) {
      stressAnalysis += 'Assessment: GOOD - Acceptable degradation under stress\n';
    } else if (errors.values.rate < 0.1) {
      stressAnalysis += 'Assessment: FAIR - Noticeable degradation, consider scaling\n';
    } else {
      stressAnalysis += 'Assessment: POOR - Significant failures, investigate bottlenecks\n';
    }
  }

  stressAnalysis += '\n=============================\n';

  // Check if thresholds passed
  const thresholdsPassed = Object.values(data.metrics)
    .filter((m) => m.thresholds)
    .every((m) => Object.values(m.thresholds).every((t) => t.ok));

  console.log(stressAnalysis);
  console.log(`Overall Result: ${thresholdsPassed ? 'PASSED' : 'FAILED'}`);

  return {
    // Console output with human-readable summary
    stdout: textSummary(data, { indent: '  ', enableColors: true }),

    // JSON output for CI/CD and historical analysis
    [`results/stress-${environment}-${timestamp}.json`]: JSON.stringify(data, null, 2),

    // Latest results file (overwrites previous)
    [`results/stress-${environment}-latest.json`]: JSON.stringify(data, null, 2),
  };
}
