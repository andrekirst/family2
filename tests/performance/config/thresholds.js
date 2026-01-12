/**
 * k6 Threshold Configurations for Family Hub Performance Tests
 *
 * Thresholds define pass/fail criteria for performance metrics.
 * Different threshold sets are used for different test scenarios.
 */

/**
 * Default thresholds for standard load testing
 * Suitable for normal production-like traffic patterns
 */
export const defaultThresholds = {
  // HTTP request duration thresholds
  http_req_duration: [
    'p(50)<200', // 50% of requests under 200ms
    'p(95)<500', // 95% of requests under 500ms
    'p(99)<1000', // 99% of requests under 1s
  ],
  // HTTP request failure rate
  http_req_failed: ['rate<0.01'], // Less than 1% errors
  // Iteration duration (full test iteration)
  iteration_duration: ['p(95)<2000'], // 95% of iterations under 2s
  // Checks passing rate
  checks: ['rate>0.99'], // 99% of checks must pass
};

/**
 * Strict thresholds for baseline/smoke testing
 * Used to establish performance baselines with tighter constraints
 */
export const strictThresholds = {
  // Tighter HTTP request duration thresholds
  http_req_duration: [
    'p(50)<50', // 50% of requests under 50ms
    'p(95)<150', // 95% of requests under 150ms
    'p(99)<300', // 99% of requests under 300ms
  ],
  // Very low error tolerance
  http_req_failed: ['rate<0.001'], // Less than 0.1% errors
  // Iteration duration
  iteration_duration: ['p(95)<1000'], // 95% of iterations under 1s
  // All checks must pass
  checks: ['rate>0.999'], // 99.9% of checks must pass
};

/**
 * Relaxed thresholds for stress testing
 * Used when pushing the system beyond normal capacity
 */
export const stressThresholds = {
  // More lenient response time expectations under stress
  http_req_duration: [
    'p(95)<1000', // 95% of requests under 1s (relaxed)
    'p(99)<3000', // 99% of requests under 3s
  ],
  // Higher error tolerance during stress
  http_req_failed: ['rate<0.05'], // Less than 5% errors acceptable
  // Iteration duration relaxed
  iteration_duration: ['p(95)<5000'], // 95% of iterations under 5s
  // Lower check pass rate acceptable
  checks: ['rate>0.95'], // 95% of checks must pass
};

/**
 * Tagged thresholds for specific GraphQL operations
 * Use with request tags: { tags: { name: 'health_query' } }
 */
export const operationThresholds = {
  // Health query should be very fast
  'http_req_duration{name:health_query}': ['p(95)<100'],
  // Auth URL query (may involve more processing)
  'http_req_duration{name:auth_url_query}': ['p(95)<200'],
};

/**
 * Combine base thresholds with operation-specific thresholds
 * @param {Object} baseThresholds - Base threshold configuration
 * @returns {Object} Combined thresholds
 */
export function withOperationThresholds(baseThresholds) {
  return {
    ...baseThresholds,
    ...operationThresholds,
  };
}
