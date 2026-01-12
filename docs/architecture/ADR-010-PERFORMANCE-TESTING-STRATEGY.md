# ADR-010: Performance Testing Strategy

**Status:** Accepted
**Date:** 2026-01-12
**Deciders:** Andre Kirst (with Claude Code AI)
**Tags:** performance, k6, load-testing, graphql, dataloader, ci-cd
**Related ADRs:** [ADR-011](ADR-011-DATALOADER-PATTERN.md)
**Issue:** #76

## Context

Family Hub's GraphQL API uses **DataLoaders** (per [ADR-011](ADR-011-DATALOADER-PATTERN.md)) to prevent N+1 query problems. Performance testing is critical to:

1. **Validate DataLoader Efficiency**: Confirm expected latency improvements
2. **Establish Baselines**: Define acceptable performance thresholds
3. **Detect Regressions**: Catch performance degradation in CI/CD
4. **Load Testing**: Validate system behavior under concurrent users

### Problem Statement

Without systematic performance testing:

- DataLoader improvements cannot be quantified
- Performance regressions go unnoticed until production
- No baseline exists for acceptable response times
- CI/CD cannot validate performance automatically

### Performance Targets (ADR-011)

| Metric | Without DataLoader | With DataLoader | Target |
|--------|-------------------|-----------------|--------|
| Query Count (100 users) | 201 | ≤3 | 67x reduction |
| Latency (p95) | ~8.4s | ≤200ms | 42x improvement |

### Technology Stack

- **k6**: Load testing framework (JavaScript)
- **GraphQL**: API protocol
- **PostgreSQL**: Database
- **Docker Compose**: Local test infrastructure

## Decision

**Implement a k6-based performance testing framework with custom DataLoader metrics, configurable threshold sets, and CI/CD integration.**

### Test Architecture

```
┌─────────────────────────────────────────────────────────────────────────────┐
│ Performance Testing Architecture                                            │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  tests/performance/                                                         │
│  ├── scenarios/                                                             │
│  │   ├── dataloader.js      ◀── DataLoader benchmark (ADR-011 validation)  │
│  │   ├── smoke.js           ◀── Quick smoke test (baseline)                 │
│  │   ├── load.js            ◀── Standard load test                          │
│  │   └── stress.js          ◀── Stress/breaking point test                  │
│  │                                                                          │
│  ├── config/                                                                │
│  │   ├── thresholds.js      ◀── Threshold configurations                    │
│  │   └── environments.js    ◀── Environment configs (local, CI, prod)       │
│  │                                                                          │
│  ├── helpers/                                                               │
│  │   ├── graphql.js         ◀── GraphQL request helper                      │
│  │   └── auth.js            ◀── Authentication helper                       │
│  │                                                                          │
│  └── results/               ◀── JSON output (gitignored)                    │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### DataLoader Benchmark Scenario

```javascript
/**
 * DataLoader Performance Benchmark for Family Hub GraphQL API
 *
 * Expected Results (compared to N+1 queries):
 * - Query count: ≤3 queries for 100 users with families (vs 201)
 * - Latency: p95 < 200ms (vs ~8.4s without DataLoaders)
 */

import { Trend, Counter } from 'k6/metrics';

// Custom metrics for DataLoader performance tracking
const familyMembersDuration = new Trend('dl_family_members_duration', true);
const familyInvitationsDuration = new Trend('dl_family_invitations_duration', true);
const familyOwnerDuration = new Trend('dl_family_owner_duration', true);
const nestedQueryDuration = new Trend('dl_nested_query_duration', true);
const completeQueryDuration = new Trend('dl_complete_query_duration', true);
const totalMembersLoaded = new Counter('dl_total_members_loaded');
const totalInvitationsLoaded = new Counter('dl_total_invitations_loaded');

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

  // DataLoader-specific thresholds
  thresholds: dataLoaderThresholds,
};
```

### Threshold Configurations

```javascript
/**
 * DataLoader-specific thresholds for Family Hub
 * Based on ADR-011 performance targets
 */
export const dataLoaderThresholds = {
  // Overall HTTP request thresholds
  http_req_duration: [
    'p(50)<100',  // 50% under 100ms
    'p(95)<200',  // 95% under 200ms (ADR-011 target)
    'p(99)<500',  // 99% under 500ms
  ],
  http_req_failed: ['rate<0.01'],  // Less than 1% errors
  checks: ['rate>0.99'],           // 99% checks passing

  // Query-specific thresholds
  'http_req_duration{name:family_members}': ['p(95)<200'],
  'http_req_duration{name:family_invitations}': ['p(95)<200'],
  'http_req_duration{name:family_owner}': ['p(95)<150'],
  'http_req_duration{name:family_nested}': ['p(95)<300'],
  'http_req_duration{name:family_complete}': ['p(95)<300'],
};

/**
 * Default thresholds for standard load testing
 */
export const defaultThresholds = {
  http_req_duration: [
    'p(50)<200',
    'p(95)<500',
    'p(99)<1000',
  ],
  http_req_failed: ['rate<0.01'],
  iteration_duration: ['p(95)<2000'],
  checks: ['rate>0.99'],
};

/**
 * Stress thresholds (relaxed for breaking point testing)
 */
export const stressThresholds = {
  http_req_duration: [
    'p(95)<1000',
    'p(99)<3000',
  ],
  http_req_failed: ['rate<0.05'],
  iteration_duration: ['p(95)<5000'],
  checks: ['rate>0.95'],
};
```

### Test Authentication (Test Environment)

```javascript
// Test environment uses X-Test-User-Id header for auth bypass
// Enables isolated performance testing without real OAuth tokens

export default function (data) {
  const testUser = data.testUsers[__VU % data.testUsers.length];
  const authHeaders = { 'X-Test-User-Id': testUser.id };

  group('Family with Members', function () {
    const response = graphqlRequest(queries.familyWithMembers, null, null, {
      headers: authHeaders,
      tags: { name: 'family_members' },
    });
    // ...
  });
}
```

### GraphQL Queries Under Test

```javascript
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

  // Tests deep nesting (multiple DataLoaders chained)
  familyMembersWithFamilies: `
    query FamilyMembersWithFamilies {
      family {
        id
        members {
          id
          family {
            id
            name
          }
        }
      }
    }
  `,

  // Tests ALL DataLoaders (worst case N+1 scenario)
  familyComplete: `
    query FamilyComplete {
      family {
        id
        owner { id email }
        members { id email }
        invitations { id email status }
      }
    }
  `,
};
```

### Test Execution

```bash
# Run DataLoader benchmark
k6 run scenarios/dataloader.js

# Run with CI environment
k6 run -e K6_ENV=ci scenarios/dataloader.js

# Run smoke test
k6 run scenarios/smoke.js

# Run full load test
k6 run scenarios/load.js
```

## Rationale

### Why k6

| Framework | Language | License | Cloud Option | GraphQL Support |
|-----------|----------|---------|--------------|-----------------|
| **k6** | JavaScript | Apache-2.0 | Optional | Built-in |
| JMeter | XML/Java | Apache-2.0 | 3rd party | Plugin |
| Gatling | Scala | Apache-2.0 | Yes | Plugin |
| Artillery | JavaScript | MPL-2.0 | Yes | Built-in |
| Locust | Python | MIT | No | Manual |

**Decision**: k6 provides the best balance of:

- JavaScript for maintainability
- Built-in GraphQL support
- Threshold-based pass/fail
- Good cloud integration options
- Active open-source community

### Why Custom DataLoader Metrics

Standard HTTP metrics don't capture DataLoader-specific behavior:

```javascript
// Custom trends per DataLoader type
const familyMembersDuration = new Trend('dl_family_members_duration', true);
const familyOwnerDuration = new Trend('dl_family_owner_duration', true);
```

Benefits:

1. **Granular Analysis**: Identify which DataLoader is slow
2. **Specific Thresholds**: Different targets per query complexity
3. **Regression Detection**: Track individual DataLoader performance over time

### Why Test Environment Auth Bypass

```javascript
// Instead of real OAuth tokens:
const authHeaders = { 'X-Test-User-Id': testUser.id };
```

Benefits:

1. **No Token Expiration**: Tests don't fail due to token refresh
2. **Fast Setup**: No OAuth flow required
3. **Deterministic**: Same test user IDs across runs
4. **Isolated**: No external Zitadel dependency

### Why Multiple Threshold Sets

Different scenarios need different tolerance:

| Scenario | p95 Target | Error Rate | Use Case |
|----------|------------|------------|----------|
| **Strict** | <150ms | <0.1% | Baseline establishment |
| **Default** | <500ms | <1% | Normal load testing |
| **Stress** | <1000ms | <5% | Breaking point discovery |
| **DataLoader** | <200ms | <1% | ADR-011 validation |

## Alternatives Considered

### Alternative 1: JMeter

**Approach**: Use Apache JMeter for load testing.

**Rejected Because**:

- XML-based configuration is harder to version control
- Less intuitive for JavaScript/TypeScript developers
- GraphQL support requires plugins
- Heavier resource footprint

### Alternative 2: Artillery

**Approach**: Use Artillery for load testing.

```yaml
# artillery.yml
scenarios:
  - flow:
      - post:
          url: "/graphql"
          json:
            query: "{ family { id } }"
```

**Considered But Not Chosen**:

- Similar capabilities to k6
- k6 has better threshold syntax
- k6 has better cloud integration

### Alternative 3: Custom .NET Benchmark

**Approach**: Use BenchmarkDotNet for micro-benchmarks.

```csharp
[Benchmark]
public async Task FamilyWithMembers()
{
    await _client.SendQueryAsync(FamilyMembersQuery);
}
```

**Rejected Because**:

- Doesn't simulate real HTTP traffic
- Cannot test concurrent users
- Missing load testing patterns
- Better suited for micro-benchmarks, not load tests

### Alternative 4: Gatling

**Approach**: Use Gatling for load testing.

**Rejected Because**:

- Scala learning curve
- More complex setup
- Less active open-source development
- Overkill for current needs

## Consequences

### Positive

1. **Validated Improvements**: DataLoader performance improvements quantified
2. **Regression Detection**: CI/CD catches performance regressions early
3. **Clear Baselines**: Defined thresholds for acceptable performance
4. **Historical Tracking**: JSON output enables trend analysis
5. **Granular Metrics**: Custom metrics per DataLoader type

### Negative

1. **Test Data Required**: Must seed test data before running benchmarks
2. **Infrastructure**: Need Test environment API running
3. **Maintenance**: k6 scripts need updating with schema changes
4. **CI Integration**: Additional CI/CD step required

### Mitigation Strategies

| Risk | Mitigation |
|------|------------|
| Stale test data | Automated seed script (`npm run seed:dataloader`) |
| CI failures | Relaxed thresholds for CI, strict for manual runs |
| Script maintenance | Schema-driven query generation (future) |

## Implementation

### Files Created

| File | Purpose |
|------|---------|
| `tests/performance/scenarios/dataloader.js` | DataLoader benchmark scenario |
| `tests/performance/config/thresholds.js` | Threshold configurations |
| `tests/performance/config/environments.js` | Environment configs |
| `tests/performance/helpers/graphql.js` | GraphQL request helper |

### Verification

1. **Local Run**: `k6 run scenarios/dataloader.js` completes successfully
2. **Thresholds**: All DataLoader thresholds pass (p95 < 200ms)
3. **Custom Metrics**: `dl_*` metrics appear in output
4. **JSON Output**: Results written to `results/` directory
5. **CI Integration**: GitHub Actions workflow passes (future)

### Running Tests

```bash
# Prerequisites
cd tests/performance
npm install

# Seed test data
npm run seed:dataloader

# Run DataLoader benchmark
k6 run scenarios/dataloader.js

# Run with specific environment
k6 run -e K6_ENV=ci scenarios/dataloader.js

# View results
cat results/dataloader-local-latest.json | jq '.metrics["http_req_duration"]'
```

## Related Decisions

- [ADR-011: DataLoader Pattern](ADR-011-DATALOADER-PATTERN.md) - Performance targets validated by this strategy

## Future Work

- **CI/CD Integration**: GitHub Actions workflow for performance gates
- **Grafana Dashboard**: Visualize k6 metrics over time
- **Database Metrics**: Integrate PostgreSQL query count validation
- **Stress Testing**: Discover breaking points and auto-scaling triggers
- **Comparison Reports**: Generate before/after comparison reports

## References

- [k6 Documentation](https://k6.io/docs/)
- [k6 GraphQL Testing](https://k6.io/docs/examples/graphql/)
- [k6 Thresholds](https://k6.io/docs/using-k6/thresholds/)
- [Family Hub Event Chains](event-chains-reference.md)

---

**Decision**: Implement k6-based performance testing with custom DataLoader metrics (`dl_*`), configurable threshold sets (default, strict, stress, dataLoader), and Test environment auth bypass via `X-Test-User-Id` header. This validates ADR-011 performance targets (p95 < 200ms) and enables CI/CD regression detection.
