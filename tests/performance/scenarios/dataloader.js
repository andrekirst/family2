/**
 * DataLoader Performance Benchmark for Family Hub GraphQL API
 *
 * Purpose: Validate DataLoader efficiency improvements (ADR-011)
 *
 * Expected Results (compared to N+1 queries):
 * - Query count: ≤3 queries for 100 users with families (vs 201)
 * - Latency: p95 < 200ms (vs ~8.4s without DataLoaders)
 * - These represent 67x query reduction and 42x latency improvement
 *
 * Prerequisites:
 * - API running with ASPNETCORE_ENVIRONMENT=Test
 * - Test data seeded: npm run seed:dataloader
 *
 * Usage:
 *   k6 run scenarios/dataloader.js
 *   k6 run -e K6_ENV=ci scenarios/dataloader.js
 */

import { sleep, group, check } from 'k6';
import { Trend, Counter } from 'k6/metrics';
import { textSummary } from 'https://jslib.k6.io/k6-summary/0.1.0/index.js';
import { dataLoaderThresholds } from '../config/thresholds.js';
import { getEnvironmentName, getTestUsers } from '../config/environments.js';
import { graphqlRequest, checkGraphQLResponse } from '../helpers/graphql.js';

// Custom metrics for DataLoader performance tracking
const familyMembersDuration = new Trend('dl_family_members_duration', true);
const familyInvitationsDuration = new Trend('dl_family_invitations_duration', true);
const familyOwnerDuration = new Trend('dl_family_owner_duration', true);
const nestedQueryDuration = new Trend('dl_nested_query_duration', true);
const completeQueryDuration = new Trend('dl_complete_query_duration', true);
const totalMembersLoaded = new Counter('dl_total_members_loaded');
const totalInvitationsLoaded = new Counter('dl_total_invitations_loaded');

/**
 * Test configuration
 */
export const options = {
  scenarios: {
    // Baseline: Constant low load to establish baseline metrics
    dataloader_baseline: {
      executor: 'constant-vus',
      vus: 10,
      duration: '1m',
      tags: { phase: 'baseline' },
    },
    // Load: Ramp up to validate performance under moderate load
    dataloader_load: {
      executor: 'ramping-vus',
      startVUs: 0,
      stages: [
        { duration: '30s', target: 30 },
        { duration: '2m', target: 30 },
        { duration: '30s', target: 0 },
      ],
      startTime: '1m10s', // Start after baseline completes
      tags: { phase: 'load' },
    },
  },

  // DataLoader-specific thresholds (p95 < 200ms)
  thresholds: dataLoaderThresholds,

  // Tags for filtering results
  tags: {
    scenario: 'dataloader',
    environment: getEnvironmentName(),
  },

  // Summary configuration
  summaryTrendStats: ['avg', 'min', 'med', 'max', 'p(50)', 'p(90)', 'p(95)', 'p(99)'],
};

/**
 * GraphQL queries for DataLoader benchmarking
 */
const queries = {
  // Tests UsersByFamilyGroupedDataLoader (1:N batching)
  familyWithMembers: `
    query FamilyWithMembers {
      family {
        id
        name
        members {
          id
          email
        }
      }
    }
  `,

  // Tests InvitationsByFamilyGroupedDataLoader (1:N batching)
  familyWithInvitations: `
    query FamilyWithInvitations {
      family {
        id
        name
        invitations {
          id
          email
          status
          displayCode
        }
      }
    }
  `,

  // Tests UserBatchDataLoader (1:1 batching)
  familyWithOwner: `
    query FamilyWithOwner {
      family {
        id
        name
        owner {
          id
          email
        }
      }
    }
  `,

  // Tests deep nesting: UsersByFamily + FamilyBatchDataLoader chain
  familyMembersWithFamilies: `
    query FamilyMembersWithFamilies {
      family {
        id
        name
        members {
          id
          email
          family {
            id
            name
          }
        }
      }
    }
  `,

  // Tests ALL DataLoaders together (worst case N+1 scenario)
  familyComplete: `
    query FamilyComplete {
      family {
        id
        name
        owner {
          id
          email
        }
        members {
          id
          email
        }
        invitations {
          id
          email
          status
        }
      }
    }
  `,
};

/**
 * Setup function - validates test data exists
 */
export function setup() {
  console.log(`Starting DataLoader benchmark against ${getEnvironmentName()} environment`);

  const testUsers = getTestUsers();
  if (!testUsers || testUsers.length === 0) {
    throw new Error('No test users configured. Run seed script first: npm run seed:dataloader');
  }

  console.log(`Found ${testUsers.length} test users`);

  // Warmup: Verify test data exists by querying first test user's family
  const testUserId = testUsers[0].id;
  const verifyResponse = graphqlRequest(queries.familyWithMembers, null, null, {
    headers: { 'X-Test-User-Id': testUserId },
  });

  const json = verifyResponse.json();
  if (!json.data?.family) {
    console.error('Setup verification failed:', JSON.stringify(json));
    throw new Error(
      'Test data not found. Ensure API is running in Test environment ' +
        'and data is seeded: npm run seed:dataloader'
    );
  }

  const memberCount = json.data.family.members?.length || 0;
  console.log(
    `✓ Test data verified: Family "${json.data.family.name}" with ${memberCount} members`
  );

  // Run warmup queries to prime caches
  console.log('Running warmup queries...');
  for (let i = 0; i < 3; i++) {
    graphqlRequest(queries.familyWithMembers, null, null, {
      headers: { 'X-Test-User-Id': testUserId },
    });
    graphqlRequest(queries.familyWithInvitations, null, null, {
      headers: { 'X-Test-User-Id': testUserId },
    });
  }
  console.log('✓ Warmup complete');

  return {
    testUsers: testUsers,
    startTime: new Date().toISOString(),
  };
}

/**
 * Main test function - executed by each VU
 */
export default function (data) {
  // Select test user based on VU ID for distribution across families
  const testUser = data.testUsers[__VU % data.testUsers.length];
  const authHeaders = { 'X-Test-User-Id': testUser.id };

  // Group: Family with Members (UsersByFamilyGroupedDataLoader)
  group('Family with Members', function () {
    const response = graphqlRequest(queries.familyWithMembers, null, null, {
      headers: authHeaders,
      tags: { name: 'family_members' },
    });

    familyMembersDuration.add(response.timings.duration);

    const success = checkGraphQLResponse(response, 'Family Members Query');

    if (success) {
      const json = response.json();
      const memberCount = json.data?.family?.members?.length || 0;
      totalMembersLoaded.add(memberCount);

      check(json, {
        'has members': (j) => j.data?.family?.members?.length > 0,
        'has expected member count (≥10)': (j) => j.data?.family?.members?.length >= 10,
      });
    }
  });

  sleep(0.3);

  // Group: Family with Invitations (InvitationsByFamilyGroupedDataLoader)
  group('Family with Invitations', function () {
    const response = graphqlRequest(queries.familyWithInvitations, null, null, {
      headers: authHeaders,
      tags: { name: 'family_invitations' },
    });

    familyInvitationsDuration.add(response.timings.duration);

    const success = checkGraphQLResponse(response, 'Family Invitations Query');

    if (success) {
      const json = response.json();
      const invitationCount = json.data?.family?.invitations?.length || 0;
      totalInvitationsLoaded.add(invitationCount);

      check(json, {
        'has invitations': (j) => j.data?.family?.invitations?.length > 0,
      });
    }
  });

  sleep(0.3);

  // Group: Family with Owner (UserBatchDataLoader)
  group('Family with Owner', function () {
    const response = graphqlRequest(queries.familyWithOwner, null, null, {
      headers: authHeaders,
      tags: { name: 'family_owner' },
    });

    familyOwnerDuration.add(response.timings.duration);

    const success = checkGraphQLResponse(response, 'Family Owner Query');

    if (success) {
      const json = response.json();
      check(json, {
        'has owner': (j) => j.data?.family?.owner?.id != null,
        'owner has email': (j) => j.data?.family?.owner?.email?.length > 0,
      });
    }
  });

  sleep(0.3);

  // Group: Deep Nested Query (multiple DataLoaders chained)
  group('Nested Query (Members → Family)', function () {
    const response = graphqlRequest(queries.familyMembersWithFamilies, null, null, {
      headers: authHeaders,
      tags: { name: 'family_nested' },
    });

    nestedQueryDuration.add(response.timings.duration);

    const success = checkGraphQLResponse(response, 'Nested Query');

    if (success) {
      const json = response.json();
      check(json, {
        'nested family resolved': (j) => {
          const members = j.data?.family?.members || [];
          return members.length > 0 && members[0]?.family?.id != null;
        },
      });
    }
  });

  sleep(0.3);

  // Group: Complete Query (ALL DataLoaders - worst case N+1)
  group('Complete Query (All DataLoaders)', function () {
    const response = graphqlRequest(queries.familyComplete, null, null, {
      headers: authHeaders,
      tags: { name: 'family_complete' },
    });

    completeQueryDuration.add(response.timings.duration);

    const success = checkGraphQLResponse(response, 'Complete Query');

    if (success) {
      const json = response.json();
      check(json, {
        'complete query has owner': (j) => j.data?.family?.owner?.id != null,
        'complete query has members': (j) => j.data?.family?.members?.length > 0,
        'complete query has invitations': (j) => j.data?.family?.invitations?.length >= 0,
      });
    }
  });

  // Pause between iterations
  sleep(0.5);
}

/**
 * Teardown function - runs once after test completes
 */
export function teardown(data) {
  console.log(`\nDataLoader benchmark completed`);
  console.log(`Started at: ${data.startTime}`);
  console.log(`Ended at: ${new Date().toISOString()}`);
  console.log(`\nExpected improvements (vs N+1):`);
  console.log(`- Query count: 201 → ≤3 (67x reduction)`);
  console.log(`- Latency: ~8.4s → ≤200ms (42x improvement)`);
}

/**
 * Handle summary - generates test report
 */
export function handleSummary(data) {
  const timestamp = new Date().toISOString().replace(/[:.]/g, '-');
  const environment = getEnvironmentName();

  // Add custom analysis to console output
  let consoleOutput = textSummary(data, { indent: '  ', enableColors: true });
  consoleOutput += '\n\n=== DataLoader Performance Analysis ===\n';
  consoleOutput += `Environment: ${environment}\n`;
  consoleOutput += `Expected p95 latency: < 200ms\n`;
  consoleOutput += `Expected error rate: < 1%\n`;

  // Check if thresholds passed
  const passed = Object.keys(data.metrics).every((key) => {
    const metric = data.metrics[key];
    return !metric.thresholds || Object.values(metric.thresholds).every((t) => t.ok);
  });

  consoleOutput += `\nOverall Result: ${passed ? '✓ PASSED' : '✗ FAILED'}\n`;

  return {
    // Console output with analysis
    stdout: consoleOutput,

    // JSON output for CI/CD and historical analysis
    [`results/dataloader-${environment}-${timestamp}.json`]: JSON.stringify(data, null, 2),

    // Latest results file (overwrites previous)
    [`results/dataloader-${environment}-latest.json`]: JSON.stringify(data, null, 2),
  };
}
